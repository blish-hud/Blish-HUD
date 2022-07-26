using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Blish_HUD.ArcDps {

    public sealed class SocketListener {
        private const int MESSAGE_HEADER_SIZE = 8;

        private static readonly Logger Logger = Logger.GetLogger<SocketListener>();

        private readonly int _bufferSize;

        private CancellationTokenSource _cancellationTokenSource;

        private int _connectionTries = 0;

        public SocketListener(int bufferSize) {
            _bufferSize = bufferSize;
        }

        /// <summary>
        ///     Indicates whether the socket listener is running.
        /// </summary>
        public bool Running { get; private set; }

        public event EventHandler<MessageData> ReceivedMessage;

        public event EventHandler<SocketError> OnSocketError;

        public void Start(IPEndPoint localEndPoint) {
            if (_cancellationTokenSource is { IsCancellationRequested: false }) {
                Logger.Warn("Start() was called more than once. Call Stop() before calling Start() again.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                ReceiveBufferSize = _bufferSize
            };

            _cancellationTokenSource.Token.Register(() => Release(listenSocket));

            var socketEventArgs = new SocketAsyncEventArgs();
            socketEventArgs.Completed += OnIoCompleted;
            socketEventArgs.SetBuffer(new byte[_bufferSize], 0, _bufferSize);
            socketEventArgs.AcceptSocket = listenSocket;
            socketEventArgs.RemoteEndPoint = localEndPoint;

            this._connectionTries = 0;

            try {
                this.Connect(socketEventArgs);
            } catch (Exception e) {
                Logger.Warn(e, "Failed to connect to Arcdps-BHUD bridge.");
            }
        }

        public void Stop() {
            _cancellationTokenSource?.Cancel();
            this.Running = false;
            Logger.Debug("Stopped ArcDPS SocketListener.");
        }

        public void Release(Socket listenSocket) {
            try {
                if (listenSocket.Connected) {
                    listenSocket.Shutdown(SocketShutdown.Receive);
                }

                listenSocket.Close();
            } catch (Exception reason) {
                Logger.Warn(reason, "An unexpected error occurred when disconnecting the Arcdps-BHUD bridge.");
            }
        }

        private void OnIoCompleted(object sender, SocketAsyncEventArgs e) {
            // Operation completed async
            switch (e.LastOperation) {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
            }
        }

        private void Connect(SocketAsyncEventArgs socketEventArgs) {
            this._connectionTries++;

            if (this._connectionTries >= 10) {
                Logger.Error($"Socket tried connecting {this._connectionTries} times, abort.");
                this.OnSocketError?.Invoke(this, SocketError.ConnectionReset);
                return;
            }
            // The next line returns true when the operation is pending; false when it completed without delay
            if (!socketEventArgs.AcceptSocket.ConnectAsync(socketEventArgs)) {
                // Connected immediately
                ProcessConnect(socketEventArgs);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e) {
            if (e.SocketError == SocketError.Success) {
                this.Running = true;
                this._connectionTries = 0;
                Logger.Debug("Connected.");
            } else if (e.SocketError == SocketError.ConnectionReset) {
                Logger.Warn("Lost connection.");
                // Connection closed, try again
                this.Connect(e);
                return;
            } else {
                this.OnSocketError?.Invoke(this, e.SocketError);
                return;
            }

            try {
                e.UserToken = new AsyncUserToken();

                // The next line returns true when the operation is pending; false when it completed without delay
                if (!e.AcceptSocket.ReceiveAsync(e)) {
                    ProcessReceive(e);
                }
            } catch {
                // ignored
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e) {
            var token = (AsyncUserToken)e.UserToken;

            do {
                if (e.SocketError == SocketError.Success) {
                    if (e.BytesTransferred > 0) {
                        ProcessReceivedData(
                                            token.DataStartOffset,
                                            token.NextReceiveOffset - token.DataStartOffset + e.BytesTransferred, 0, token, e.Buffer
                                           );

                        token.NextReceiveOffset += e.BytesTransferred;

                        if (token.NextReceiveOffset == e.Buffer.Length) {
                            token.NextReceiveOffset = 0;

                            if (token.DataStartOffset < e.Buffer.Length) {
                                int notYetProcessed = e.Buffer.Length - token.DataStartOffset;
                                Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notYetProcessed);

                                token.NextReceiveOffset = notYetProcessed;
                            }

                            token.DataStartOffset = 0;
                        }

                        e.SetBuffer(token.NextReceiveOffset, e.Buffer.Length - token.NextReceiveOffset);
                    }
                } else if (e.SocketError == SocketError.ConnectionReset) {
                    Logger.Warn("Lost connection.");
                    // Try reconnect
                    this.Connect(e);
                    return;
                } else {
                    this.OnSocketError?.Invoke(this, e.SocketError);
                }

                if (_cancellationTokenSource.IsCancellationRequested) {
                    break;
                }
            } while (!e.AcceptSocket.ReceiveAsync(e)); // Returns true when the operation is pending; false when it completed without delay

        }

        private void ProcessReceivedData(
            int dataStartOffset, int totalReceivedDataSize, int alreadyProcessedDataSize,
            AsyncUserToken token, byte[] buffer
        ) {
            while (true) {
                if (alreadyProcessedDataSize >= totalReceivedDataSize) {
                    return;
                }

                if (token.MessageSize == null) {
                    if (totalReceivedDataSize - alreadyProcessedDataSize > MESSAGE_HEADER_SIZE) {
                        byte[] headerData = new byte[MESSAGE_HEADER_SIZE];
                        Buffer.BlockCopy(buffer, dataStartOffset, headerData, 0, MESSAGE_HEADER_SIZE);
                        int messageSize = BitConverter.ToInt32(headerData, 0);

                        token.MessageSize = messageSize;
                        token.DataStartOffset = dataStartOffset + MESSAGE_HEADER_SIZE;

                        dataStartOffset = token.DataStartOffset;
                        alreadyProcessedDataSize += MESSAGE_HEADER_SIZE;
                        continue;
                    }
                } else {
                    int messageSize = token.MessageSize.Value;

                    if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize) {
                        byte[] messageData = new byte[messageSize];
                        Buffer.BlockCopy(buffer, dataStartOffset, messageData, 0, messageSize);
                        ProcessMessage(messageData, token);

                        token.DataStartOffset = dataStartOffset + messageSize;
                        token.MessageSize = null;

                        dataStartOffset = token.DataStartOffset;
                        alreadyProcessedDataSize += messageSize;
                        continue;
                    }
                }

                break;
            }
        }

        private void ProcessMessage(byte[] messageData, AsyncUserToken token) {
            this.ReceivedMessage?.Invoke(this, new MessageData { Message = messageData, Token = token });
        }

    }

}