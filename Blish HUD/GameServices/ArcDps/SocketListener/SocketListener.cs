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

        private static readonly int _retryReceiveCount = 10;

        private readonly SocketError[] _retryReceiveOnSocketErrors = new SocketError[] {
            SocketError.ConnectionReset
        };

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

            _cancellationTokenSource.Token.Register(() => ReleaseSocket(listenSocket));

            this.StartConnect(listenSocket, localEndPoint);
        }

        public void Stop() {
            _cancellationTokenSource?.Cancel();
        }

        private void ReleaseSocket(Socket socket) {
            try {
                if (socket.Connected) {
                    socket.Shutdown(SocketShutdown.Receive);
                    socket.Close();

                    Logger.Debug("Disconnected.");
                } else {
                    Logger.Warn("Socket already disconnected.");
                }

                this.Running = false;
            } catch (Exception reason) {
                Logger.Error(reason, "Failed to disconnect socket:");
            }
        }

        private void StartConnect(Socket client, EndPoint endPoint) {
            try {
                _ = client.BeginConnect(endPoint, this.ConnectCallback, client);
                Logger.Debug("Connected.");
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to connect socket.");

                this.Stop();
            }
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.
                Socket socket = (Socket)ar.AsyncState;

                // Complete the connection.
                socket.EndConnect(ar);

                Logger.Debug("Socket connected to {0}",
                    socket.RemoteEndPoint.ToString());

                this.Running = true;

                this.StartReceive(socket);
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to connect socket:");

                this.Stop();
            }
        }

        private void StartReceive(Socket socket, SocketState state = null, int? retries = null) {
            retries ??= _retryReceiveCount;

            try {
                // Create the state object.
                state ??= new SocketState() {
                    Socket = socket,
                    Token = new AsyncUserToken()
                };

                // Begin receiving the data from the remote device.
                _ = socket.BeginReceive(state.Buffer, state.Token.NextReceiveOffset, SocketState.BUFFER_SIZE - state.Token.NextReceiveOffset, 0, this.ReceiveCallback, state);
            } catch (SocketException socketEx) {
                Logger.Error(socketEx, "Failed to start receiving from socket: ");

                if (Array.Exists(_retryReceiveOnSocketErrors, socketError => socketError == socketEx.SocketErrorCode)) {
                    if (retries >= 1) {
                        // Retry again
                        Logger.Error("Retry starting receive");

                        this.StartReceive(socket, state, retries--);
                    } else {
                        Logger.Error("Failed too many times. No repeat attempted.");
                        this.OnSocketError?.Invoke(this, socketEx.SocketErrorCode);

                        this.Stop();
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to start receiving from socket:");

                this.OnSocketError?.Invoke(this, SocketError.SocketError);

                this.Stop();
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                SocketState state = (SocketState)ar.AsyncState;
                Socket socket = state.Socket;

                // Read data from the remote device.
                int bytesRead = 0;
                try {
                    bytesRead = socket.EndReceive(ar);
                } catch (SocketException socketEx) {
                    if (!Array.Exists(_retryReceiveOnSocketErrors, socketError => socketError == socketEx.SocketErrorCode) && !_cancellationTokenSource.IsCancellationRequested) {
                        // Errors can be expected when socket already closed
                        Logger.Error(socketEx, "Failed to receive from socket: ");

                        this.OnSocketError?.Invoke(this, socketEx.SocketErrorCode);

                        this.Stop();

                        return;
                    }
                }

                try {
                    this.ProcessReceive(state, bytesRead);
                } catch (Exception ex) {
                    Logger.Error(ex, "Failed to receive from socket:");
                    bytesRead = 0;
                }

                if (_cancellationTokenSource.IsCancellationRequested) {
                    // Stop processing
                    return;
                }

                if (bytesRead > 0) {

                    // Get the rest of the data.
                    this.StartReceive(socket, state);
                } else {
                    // All the data has arrived.
                    this.StartReceive(socket);
                }
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to receive from socket:");
                this.OnSocketError?.Invoke(this, SocketError.SocketError);

                this.Stop();
            }
        }

        private void ProcessReceive(SocketState state, int bytesRead) {
            var token = state.Token;

            if (bytesRead > 0) {
                this.ProcessReceivedData(state, bytesRead);

                token.NextReceiveOffset += bytesRead;

                if (token.NextReceiveOffset == state.Buffer.Length) {
                    token.NextReceiveOffset = 0;

                    if (token.DataStartOffset < state.Buffer.Length) {
                        int notYetProcessed = state.Buffer.Length - token.DataStartOffset;
                        Buffer.BlockCopy(state.Buffer, token.DataStartOffset, state.Buffer, 0, notYetProcessed);

                        token.NextReceiveOffset = notYetProcessed;
                    }

                    token.DataStartOffset = 0;
                }
            }
        }

        private void ProcessReceivedData(SocketState state, int bytesRead) {
            int dataStartOffset = state.Token.DataStartOffset;
            int totalReceivedDataSize = state.Token.NextReceiveOffset - state.Token.DataStartOffset + bytesRead;
            byte[] buffer = state.Buffer;
            int alreadyProcessedDataSize = 0;

            while (true) {
                if (alreadyProcessedDataSize >= totalReceivedDataSize) {
                    return;
                }

                // Check if message size has to be read first.
                if (state.Token.MessageSize == null) {
                    // Check if buffer contains first or additional messages
                    if (totalReceivedDataSize - alreadyProcessedDataSize > MESSAGE_HEADER_SIZE) {
                        byte[] headerData = new byte[MESSAGE_HEADER_SIZE];
                        Buffer.BlockCopy(buffer, dataStartOffset, headerData, 0, MESSAGE_HEADER_SIZE);
                        int messageSize = BitConverter.ToInt32(headerData, 0);

                        state.Token.MessageSize = messageSize;
                        state.Token.DataStartOffset = dataStartOffset + MESSAGE_HEADER_SIZE;

                        dataStartOffset = state.Token.DataStartOffset;
                        alreadyProcessedDataSize += MESSAGE_HEADER_SIZE;
                        continue;
                    }
                } else {
                    // Read message
                    int messageSize = state.Token.MessageSize.Value;

                    if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize) {
                        byte[] messageData = new byte[messageSize];
                        Buffer.BlockCopy(buffer, dataStartOffset, messageData, 0, messageSize);

                        this.ProcessMessage(messageData, state.Token);

                        state.Token.DataStartOffset = dataStartOffset + messageSize;
                        state.Token.MessageSize = null;

                        dataStartOffset = state.Token.DataStartOffset;
                        alreadyProcessedDataSize += messageSize;
                        continue;
                    }
                }

                break;
            }
        }

        private void ProcessMessage(byte[] messageData, AsyncUserToken token) {
            try {
                this.ReceivedMessage?.Invoke(this, new MessageData { Message = messageData, Token = token });
            } catch (Exception ex) {
                Logger.Error(ex, "Failed processing received message:");
            }
        }
    }
}