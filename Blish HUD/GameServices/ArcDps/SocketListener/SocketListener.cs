using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Blish_HUD.ArcDps {

    public sealed class SocketListener {

        private const int MESSAGE_HEADER_SIZE = 8;

        private static readonly Logger Logger = Logger.GetLogger<SocketListener>();

        private readonly int _bufferSize;

        private CancellationTokenSource _cancellationTokenSource;

        public SocketListener(int bufferSize) {
            _bufferSize = bufferSize;
        }

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
            socketEventArgs.AcceptSocket   = listenSocket;
            socketEventArgs.RemoteEndPoint = localEndPoint;

            try {
                // The next line returns true when the operation is pending; false when it completed without delay
                if (!listenSocket.ConnectAsync(socketEventArgs)) {
                    ProcessConnect(socketEventArgs);
                }
            } catch (Exception e) {
                Logger.Warn(e, "Failed to connect to Arcdps-BHUD bridge.");
            }
        }

        public void Stop() {
            _cancellationTokenSource?.Cancel();
            this.Running = false;
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
            switch (e.LastOperation) {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success) {
                this.OnSocketError?.Invoke(this, e.SocketError);
                return;
            } else {
                this.Running = true;
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

                        token.MessageSize     = messageSize;
                        token.DataStartOffset = dataStartOffset + MESSAGE_HEADER_SIZE;

                        dataStartOffset          =  token.DataStartOffset;
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
                        token.MessageSize     = null;

                        dataStartOffset          =  token.DataStartOffset;
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