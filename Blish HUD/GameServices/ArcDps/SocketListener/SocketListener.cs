using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Blish_HUD.ArcDps {

    public sealed class SocketListener {

        private const int MESSAGE_HEADER_SIZE = 8;

        private static readonly Logger Logger = Logger.GetLogger<SocketListener>();

        private static readonly Mutex Mutex = new Mutex();

        public delegate void Message(MessageData data);

        private readonly int _bufferSize;

        private readonly SocketAsyncEventArgs _socketAsyncReceiveEventArgs;
        private          Socket               _listenSocket;

        public SocketListener(int bufferSize) {
            _bufferSize = bufferSize;

            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.Completed += OnIoCompleted;
            socketAsyncEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
            _socketAsyncReceiveEventArgs = socketAsyncEventArgs;
        }

        public event Message ReceivedMessage;

        public void Start(IPEndPoint localEndPoint) {
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                ReceiveBufferSize = _bufferSize
            };

            _socketAsyncReceiveEventArgs.AcceptSocket   = _listenSocket;
            _socketAsyncReceiveEventArgs.RemoteEndPoint = localEndPoint;

            try {
                if (!_listenSocket.ConnectAsync(_socketAsyncReceiveEventArgs)) ProcessConnect(_socketAsyncReceiveEventArgs);
            } catch (Exception e) {
                Logger.Warn(e, "Failed to connect to Arcdps-BHUD bridge.");
            }

            Mutex.WaitOne();
        }

        public void Stop() {
            try {
                _listenSocket.Close();
            } catch {
                // ignored
            }

            Mutex.ReleaseMutex();
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
            try {
                _socketAsyncReceiveEventArgs.UserToken = new AsyncUserToken(e.AcceptSocket);
                if (!e.AcceptSocket.ReceiveAsync(_socketAsyncReceiveEventArgs)) ProcessReceive(_socketAsyncReceiveEventArgs);
            } catch {
                // ignored
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e) {
            while (true) {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success) {
                    if (!(e.UserToken is AsyncUserToken token)) return;

                    ProcessReceivedData(
                                        token.DataStartOffset,
                                        token.NextReceiveOffset - token.DataStartOffset + e.BytesTransferred, 0, token, e
                                       );

                    token.NextReceiveOffset += e.BytesTransferred;

                    if (token.NextReceiveOffset == e.Buffer.Length) {
                        token.NextReceiveOffset = 0;

                        if (token.DataStartOffset < e.Buffer.Length) {
                            int notYesProcessDataSize = e.Buffer.Length - token.DataStartOffset;
                            Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notYesProcessDataSize);

                            token.NextReceiveOffset = notYesProcessDataSize;
                        }

                        token.DataStartOffset = 0;
                    }

                    e.SetBuffer(token.NextReceiveOffset, e.Buffer.Length - token.NextReceiveOffset);

                    if (!token.Socket.ReceiveAsync(e)) continue;
                } else {
                    var token = e.UserToken as AsyncUserToken;
                    token?.Dispose();
                }

                break;
            }
        }

        private void ProcessReceivedData(
            int            dataStartOffset, int                  totalReceivedDataSize, int alreadyProcessedDataSize,
            AsyncUserToken token,           SocketAsyncEventArgs e
        ) {
            while (true) {
                if (alreadyProcessedDataSize >= totalReceivedDataSize) return;

                if (token.MessageSize == null) {
                    if (totalReceivedDataSize - alreadyProcessedDataSize > MESSAGE_HEADER_SIZE) {
                        var headerData = new byte[MESSAGE_HEADER_SIZE];
                        Buffer.BlockCopy(e.Buffer, dataStartOffset, headerData, 0, MESSAGE_HEADER_SIZE);
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
                        var messageData = new byte[messageSize];
                        Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, messageSize);
                        ProcessMessage(messageData, token, e);

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

        private void ProcessMessage(byte[] messageData, AsyncUserToken token, SocketAsyncEventArgs e) {
            this.ReceivedMessage?.Invoke(new MessageData {Message = messageData, Token = token});
        }

    }

}