using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.GameServices.ArcDps.Models.UnofficialExtras;
using Blish_HUD.GameServices.ArcDps.V2;
using Blish_HUD.GameServices.ArcDps.V2.Processors;

namespace Blish_HUD.GameServices.ArcDps {

    internal class ArcDpsClient : IArcDpsClient {
#if DEBUG
        public static long Counter;
#endif

        private static readonly Logger _logger = Logger.GetLogger<ArcDpsServiceV2>();
        private readonly BlockingCollection<byte[]>[] _messageQueues;
        private readonly Dictionary<int, MessageProcessor> _processors = new Dictionary<int, MessageProcessor>();
        private readonly ArcDpsBridgeVersion _arcDpsBridgeVersion;
        private bool _isConnected = false;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _linkedTokenSource;
        private CancellationToken _linkedToken;
        private bool _disposedValue;
        private CancellationToken _ct;

        public event EventHandler<SocketError> Error;

        public bool IsConnected => _isConnected && (Client?.Connected ?? false);

        public TcpClient Client { get; private set; }

        public event Action Disconnected;

        public ArcDpsClient(ArcDpsBridgeVersion arcDpsBridgeVersion) {
            this._arcDpsBridgeVersion = arcDpsBridgeVersion;

            _processors.Add(1, new ImGuiProcessor());

            if (arcDpsBridgeVersion == ArcDpsBridgeVersion.V1) {
                _processors.Add((int)MessageType.CombatEventArea, new LegacyCombatProcessor());
                _processors.Add((int)MessageType.CombatEventLocal, new LegacyCombatProcessor());
            } else {
                _processors.Add((int)MessageType.CombatEventArea, new CombatEventProcessor());
                _processors.Add((int)MessageType.CombatEventLocal, new CombatEventProcessor());
                _processors.Add((int)MessageType.UserInfo, new UnofficialExtrasUserInfoProcessor());
                _processors.Add((int)MessageType.ChatMessage, new UnofficialExtrasMessageInfoProcessor());
            }

            // hardcoded message queue size. One Collection per message type. This is done just for optimizations
            _messageQueues = new BlockingCollection<byte[]>[byte.MaxValue];

        }

        public bool IsMessageTypeAvailable(MessageType type)
            => this._processors.ContainsKey((int)type);

        public void RegisterMessageTypeListener<T>(int type, Func<T, CancellationToken, Task> listener)
            where T : struct {
            var processor = (MessageProcessor<T>)_processors[type];
            if (_messageQueues[type] == null) {
                _messageQueues[type] = new BlockingCollection<byte[]>();

                try {
                    Task.Run(() => ProcessMessage(processor, _messageQueues[type]));
                } catch (OperationCanceledException) {
                    // NOP
                }
            }

            processor.RegisterListener(listener);
        }

        private void ProcessMessage(MessageProcessor processor, BlockingCollection<byte[]> messageQueue) {
            while (!_linkedToken.IsCancellationRequested) {
                _linkedToken.ThrowIfCancellationRequested();
                Task.Delay(1).Wait();
                foreach (var item in messageQueue.GetConsumingEnumerable()) {
                    _linkedToken.ThrowIfCancellationRequested();
                    processor.Process(item, _linkedToken);
                    ArrayPool<byte>.Shared.Return(item);
                }
            }

            _linkedToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Initializes the client and connects to the arcdps "server"
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="ct">CancellationToken to cancel the whole client</param>
        public void Initialize(IPEndPoint endpoint, CancellationToken ct) {
            this._ct = ct;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct, this._cancellationTokenSource.Token);
            _linkedToken = _linkedTokenSource.Token;
            Client?.Dispose();
            Client = new TcpClient();
            Client.ReceiveBufferSize = 4096;
            Client.Connect(endpoint);
            _logger.Info("Connected to arcdps endpoint on: " + endpoint.ToString());

            _networkStream = Client.GetStream();
            _isConnected = true;

            try {
                if (_arcDpsBridgeVersion == ArcDpsBridgeVersion.V1) {
                    Task.Run(async () => await LegacyReceive(_linkedToken), _linkedToken);
                } else {
                    Task.Run(async () => await Receive(_linkedToken), _linkedToken);
                }
            } catch (OperationCanceledException) {
                // NOP
            }
        }

        public void Disconnect() {
            if (_isConnected) {
                if (Client?.Connected ?? false) {
                    Client.Close();
                    Client.Dispose();
                    _logger.Info("Disconnected from arcdps endpoint");
                }

                _isConnected = false;
                Disconnected?.Invoke();
            }
        }

        private async Task LegacyReceive(CancellationToken ct) {
            _logger.Info($"Start Legacy Receive Task for {Client?.Client.RemoteEndPoint?.ToString()}");
            try {
                var messageHeaderBuffer = new byte[9];
                ArrayPool<byte> pool = ArrayPool<byte>.Shared;
                while (Client?.Connected ?? false) {
                    ct.ThrowIfCancellationRequested();

                    if (Client.Available == 0) {
                        await Task.Delay(1, ct);
                        continue;
                    }

                    ReadFromStream(_networkStream, messageHeaderBuffer, 9);

                    var messageLength = Unsafe.ReadUnaligned<int>(ref messageHeaderBuffer[0]) - 1;
                    var messageType = messageHeaderBuffer[8];

                    ReadMessage(pool, messageLength, _networkStream, _messageQueues, messageType);
#if DEBUG
                    Interlocked.Increment(ref Counter);
#endif

                }
            } catch (ObjectDisposedException) {
                // We ignore DisposedExceptions, because it is clear, that the cancellation token already got cancelled
            } catch (OperationCanceledException) {
                // We ignore OperationCanceledExceptions, because we dont need to handle this, we just want to quit quietly
            } catch (Exception ex) {
                _logger.Error(ex.ToString());
                Error?.Invoke(this, SocketError.SocketError);
                Disconnect();
            }

            _logger.Info($"Legacy Receive Task for {Client?.Client.RemoteEndPoint?.ToString()} stopped");
        }

        private async Task Receive(CancellationToken ct) {
            _logger.Info($"Start Receive Task for {Client?.Client.RemoteEndPoint?.ToString()}");
            try {
                var messageHeaderBuffer = new byte[5];
                ArrayPool<byte> pool = ArrayPool<byte>.Shared;
                while (Client?.Connected ?? false) {
                    ct.ThrowIfCancellationRequested();

                    if (Client.Available == 0) {
                        await Task.Delay(1, ct);
                        continue;
                    }

                    ReadFromStream(_networkStream, messageHeaderBuffer, 5);

                    var messageLength = Unsafe.ReadUnaligned<int>(ref messageHeaderBuffer[0]) - 1;
                    var messageType = messageHeaderBuffer[4];

                    ReadMessage(pool, messageLength, _networkStream, _messageQueues, messageType);
#if DEBUG
                    Interlocked.Increment(ref Counter);
#endif
                }

                // Reconnect if the bridge closes the connection.
                // Pass on the cancellationToken from the creator of this class
                this.Initialize((IPEndPoint)this.Client.Client.RemoteEndPoint, this._ct);
            } catch (ObjectDisposedException) {
                // We ignore DisposedExceptions, because it is clear, that the cancellation token already got cancelled
            } catch (OperationCanceledException) {
                // We ignore OperationCanceledExceptions, because we dont need to handle this, we just want to quit quietly
            } catch (Exception ex) {
                _logger.Error(ex.ToString());
                Error?.Invoke(this, SocketError.SocketError);
                Disconnect();
            }

            _logger.Info($"Receive Task for {Client?.Client.RemoteEndPoint?.ToString()} stopped");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadMessage(ArrayPool<byte> pool, int messageLength, Stream networkStream, BlockingCollection<byte[]>[] messageQueues, byte messageType) {
            var messageBuffer = pool.Rent(messageLength);
            ReadFromStream(networkStream, messageBuffer, messageLength);

            if (messageQueues[messageType] != null) {
                messageQueues[messageType]?.Add(messageBuffer);
            } else {
                pool.Return(messageBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadFromStream(Stream stream, byte[] buffer, int length) {
            int bytesRead = 0;
            while (bytesRead != length) {
                bytesRead += stream.Read(buffer, bytesRead, length - bytesRead);
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    _cancellationTokenSource.Cancel();
                    Client?.Dispose();
                    if (_messageQueues != null) {
                        foreach (var item in _messageQueues) {
                            if (item != null) {
                                foreach (var message in item) {
                                    if (message != null) {
                                        ArrayPool<byte>.Shared.Return(message);
                                    }
                                }
                            }
                        }
                    }
                    _networkStream?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
