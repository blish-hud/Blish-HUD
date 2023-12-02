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
using Blish_HUD.GameServices.ArcDps.V2;

namespace Blish_HUD.GameServices.ArcDps {
    internal class ArcDpsClient : IArcDpsClient {
#if DEBUG
        public static long Counter;
#endif

        private static readonly Logger _logger = Logger.GetLogger<ArcDpsServiceV2>();
        private readonly BlockingCollection<byte[]>[] messageQueues;
        private readonly Dictionary<int, MessageProcessor> processors = new Dictionary<int, MessageProcessor>() {
            { 0, new CombatEventProcessor() },
        };

        private bool isConnected = false;
        private NetworkStream networkStream;
        private CancellationToken ct;

        public bool IsConnected => this.isConnected && this.Client.Connected;

        public TcpClient Client { get; }

        public event Action Disconnected;

        public ArcDpsClient() {
            // hardcoded message queue size. One Collection per message type. This is done just for optimizations
            this.messageQueues = new BlockingCollection<byte[]>[4];

            for (int i = 0; i < this.messageQueues.Length; i++) {
                this.messageQueues[i] = new BlockingCollection<byte[]>();
            }
            this.Client = new TcpClient();
        }

        public void RegisterMessageTypeListener<T>(int type, Func<T, CancellationToken, Task> listener)
            where T : struct {
            var processor = (MessageProcessor<T>)this.processors[type];
            if (messageQueues[type] == null) {
                messageQueues[type] = new BlockingCollection<byte[]>();

                try {
                    Task.Run(() => this.ProcessMessage(processor, messageQueues[type]));
                } catch (OperationCanceledException) {
                    // NOP
                }
            }

            processor.RegisterListener(listener);
        }

        private void ProcessMessage(MessageProcessor processor, BlockingCollection<byte[]> messageQueue) {
            foreach (var item in messageQueue.GetConsumingEnumerable()) {
                ct.ThrowIfCancellationRequested();
                processor.Process(item, ct);
            }
        }

        /// <summary>
        /// Initializes the client and connects to the arcdps "server"
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="ct">CancellationToken to cancel the whole client</param>
        public void Initialize(IPEndPoint endpoint, CancellationToken ct) {
            this.Client.Connect(endpoint);
            _logger.Info("Connected to arcdps endpoint on: " + endpoint.ToString());

            this.networkStream = this.Client.GetStream();
            this.isConnected = true;

            try {
                Task.Run(async () => await this.Receive(ct), ct);
            } catch (OperationCanceledException) {
                // NOP
            }
        }

        public void Disconnect() {
            if (isConnected) {
                if (this.Client.Connected) {
                    this.Client.Close();
                    this.Client.Dispose();
                    _logger.Info("Disconnected from arcdps endpoint");
                }

                this.isConnected = false;
                this.Disconnected?.Invoke();
            }
        }

        private async Task Receive(CancellationToken ct) {
            _logger.Info($"Start Receive Task for {this.Client.Client.RemoteEndPoint?.ToString()}");
            try {
                var messageHeaderBuffer = new byte[5];
                ArrayPool<byte> pool = ArrayPool<byte>.Shared;
                while (this.Client.Connected) {
                    ct.ThrowIfCancellationRequested();

                    if (this.Client.Available == 0) {
                        await Task.Delay(1, ct);
                    }

                    ReadFromStream(this.networkStream, messageHeaderBuffer, 5);

                    var messageLength = Unsafe.ReadUnaligned<int>(ref messageHeaderBuffer[0]);
                    var messageType = messageHeaderBuffer[4];

                    var messageBuffer = pool.Rent(messageLength);
                    ReadFromStream(this.networkStream, messageBuffer, messageLength);
                    pool.Return(messageBuffer);
                    this.messageQueues[messageType]?.Add(messageBuffer);
#if DEBUG
                    Interlocked.Increment(ref Counter);
#endif
                }
            } catch (Exception ex) {
                _logger.Error(ex.ToString());
                this.Disconnect();
            }

            _logger.Info($"Receive Task for {this.Client.Client?.RemoteEndPoint?.ToString()} stopped");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadFromStream(Stream stream, byte[] buffer, int length) {
            int bytesRead = 0;
            while (bytesRead != length) {
                bytesRead += stream.Read(buffer, bytesRead, length - bytesRead);
            }
        }
    }
}
