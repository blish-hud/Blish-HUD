using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Blish_HUD.ArcDps {

    public sealed class SocketListener {
        private static readonly Logger Logger = Logger.GetLogger<SocketListener>();

        private const int MESSAGE_HEADER_SIZE = 8;
        private const int RETRY_RECEIVE_COUNT = 10;

        private readonly int _bufferSize;

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Defines the socket errors on which a retry can be attempted. Max retry count is defined by <see cref="RETRY_RECEIVE_COUNT"/>.
        /// </summary>
        private static readonly SocketError[] _retryReceiveOnSocketErrors = new SocketError[] {
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

        /// <summary>
        /// Starts the <see cref="SocketListener"/> and attempts to connect to the specified <paramref name="localEndPoint"/>.
        /// </summary>
        /// <param name="localEndPoint">The <see cref="EndPoint"/> to which the <see cref="SocketListener"/> should connect to.</param>
        public void Start(IPEndPoint localEndPoint) {
            if (_cancellationTokenSource is { IsCancellationRequested: false }) {
                Logger.Warn("Start() was called more than once. Call Stop() before calling Start() again.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            Socket listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                ReceiveBufferSize = _bufferSize
            };

            _ = _cancellationTokenSource.Token.Register(() => this.ReleaseSocket(listenSocket));

            this.StartConnect(listenSocket, localEndPoint);
        }

        /// <summary>
        /// Stops the <see cref="SocketListener"/>.
        /// </summary>
        public void Stop() {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Stops the socket from receiving any further data and closes the connection.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> which should close.</param>
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

        /// <summary>
        /// Starts connecting the socket (<paramref name="client"/>) to the endpoint specified by <paramref name="endPoint"/>
        /// </summary>
        /// <param name="client">The <see cref="Socket"/> which should connect to the spcified endpoint.</param>
        /// <param name="endPoint">The <see cref="EndPoint"/> to which the socket (<paramref name="client"/>) should connect to.</param>
        private void StartConnect(Socket client, EndPoint endPoint) {
            try {
                _ = client.BeginConnect(endPoint, this.ConnectCallback, client);
                Logger.Debug("Connected.");
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to connect socket.");

                this.Stop();
            }
        }

        /// <summary>
        /// Handles the result of a connection attempt started by <see cref="StartConnect(Socket, EndPoint)"/>.
        /// </summary>
        /// <param name="ar">The AsyncResult containing the connected <see cref="Socket"/>.</param>
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

        /// <summary>
        /// Starts the receiving of the <paramref name="socket"/>.
        /// </summary>
        /// <param name="socket">The socket which should start receiving.</param>
        /// <param name="state">The prior socket state if not all data has been received or <c>null</c> if state can be (re)created for a new receive session.</param>
        /// <param name="retries">The amount of retries left after a <see cref="SocketError"/> specified by <see cref="_retryReceiveOnSocketErrors"/>.</param>
        private void StartReceive(Socket socket, SocketState state = null, int retries = RETRY_RECEIVE_COUNT) {
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

        /// <summary>
        /// Handles the result of a receive attempt started by <see cref="StartReceive(Socket, SocketState, int)"/>.
        /// </summary>
        /// <param name="ar">The AsyncResult containing the <see cref="SocketState"/>.</param>
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

        /// <summary>
        /// Processes the received bytes.
        /// </summary>
        /// <param name="state">The <see cref="SocketState"/> which contains all read data.</param>
        /// <param name="bytesRead">The amount of bytes read by the <see cref="SocketState.Socket"/>.</param>
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

        /// <summary>
        /// Processes the received bytes and processes the ArcDPS message further if it could be read completely.
        /// </summary>
        /// <param name="state">The <see cref="SocketState"/> which contains all read data.</param>
        /// <param name="bytesRead">The amount of bytes read by the <see cref="SocketState.Socket"/>.</param>
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

        /// <summary>
        /// Processes the read message from ArcDPS and forwards it via <see cref="ReceivedMessage"/>.
        /// </summary>
        /// <param name="messageData">The bytes of the ArcDPS message.</param>
        /// <param name="token">The <see cref="AsyncUserToken"/> containing additional message data.</param>
        private void ProcessMessage(byte[] messageData, AsyncUserToken token) {
            try {
                this.ReceivedMessage?.Invoke(this, new MessageData { Message = messageData, Token = token });
            } catch (Exception ex) {
                Logger.Error(ex, "Failed processing received message:");
            }
        }
    }
}