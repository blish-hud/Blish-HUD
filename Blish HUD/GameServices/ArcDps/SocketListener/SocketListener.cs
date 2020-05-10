using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Blish_HUD.ArcDps
{
    public sealed class SocketListener
    {
        public delegate void Message(MessageData data);

        private const int MESSAGE_HEADER_SIZE = 8;

        private static readonly Mutex Mutex = new Mutex();
        private readonly SemaphoreSlim _acceptedClientsSemaphore;
        private readonly int _bufferSize;
        private readonly int _maxConnectionCount;

        private readonly SocketAsyncEventArgsPool _socketAsyncReceiveEventArgsPool;
        private Socket _listenSocket;

        public SocketListener(int maxConnectionCount, int bufferSize)
        {
            _maxConnectionCount = maxConnectionCount;
            _bufferSize = bufferSize;
            _socketAsyncReceiveEventArgsPool = new SocketAsyncEventArgsPool();
            _acceptedClientsSemaphore = new SemaphoreSlim(maxConnectionCount, maxConnectionCount);

            for (var i = 0; i < maxConnectionCount; i++)
            {
                var socketAsyncEventArgs = new SocketAsyncEventArgs();
                socketAsyncEventArgs.Completed += OnIoCompleted;
                socketAsyncEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
                _socketAsyncReceiveEventArgsPool.Push(socketAsyncEventArgs);
            }
        }

        public event Message ReceivedMessage;

        public void Start(IPEndPoint localEndPoint)
        {
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveBufferSize = _bufferSize
            };
            _listenSocket.Bind(localEndPoint);
            _listenSocket.Listen(_maxConnectionCount);
            StartAccept(null);
            Mutex.WaitOne();
        }

        public void Stop()
        {
            try
            {
                _listenSocket.Close();
            }
            catch
            {
                // ignored
            }

            Mutex.ReleaseMutex();
        }

        private void OnIoCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive");
            }
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += (sender, e) => ProcessAccept(e);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            _acceptedClientsSemaphore.Wait();

            try
            {
                if (!_listenSocket.AcceptAsync(acceptEventArg)) ProcessAccept(acceptEventArg);
            }
            catch
            {
                // ignored
                // will fail when closing bhud
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                var readEventArgs = _socketAsyncReceiveEventArgsPool.Pop();
                if (readEventArgs != null)
                {
                    readEventArgs.UserToken = new AsyncUserToken(e.AcceptSocket);
                    if (!e.AcceptSocket.ReceiveAsync(readEventArgs)) ProcessReceive(readEventArgs);
                }
            }
            catch
            {
                // ignored
            }

            StartAccept(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            while (true)
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    if (!(e.UserToken is AsyncUserToken token)) return;
                    ProcessReceivedData(token.DataStartOffset,
                        token.NextReceiveOffset - token.DataStartOffset + e.BytesTransferred, 0, token, e);

                    token.NextReceiveOffset += e.BytesTransferred;

                    if (token.NextReceiveOffset == e.Buffer.Length)
                    {
                        token.NextReceiveOffset = 0;

                        if (token.DataStartOffset < e.Buffer.Length)
                        {
                            var notYesProcessDataSize = e.Buffer.Length - token.DataStartOffset;
                            Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notYesProcessDataSize);

                            token.NextReceiveOffset = notYesProcessDataSize;
                        }

                        token.DataStartOffset = 0;
                    }

                    e.SetBuffer(token.NextReceiveOffset, e.Buffer.Length - token.NextReceiveOffset);

                    if (!token.Socket.ReceiveAsync(e)) continue;
                }
                else
                {
                    CloseClientSocket(e);
                }

                break;
            }
        }

        private void ProcessReceivedData(int dataStartOffset, int totalReceivedDataSize, int alreadyProcessedDataSize,
            AsyncUserToken token, SocketAsyncEventArgs e)
        {
            while (true)
            {
                if (alreadyProcessedDataSize >= totalReceivedDataSize) return;

                if (token.MessageSize == null)
                {
                    if (totalReceivedDataSize - alreadyProcessedDataSize > MESSAGE_HEADER_SIZE)
                    {
                        var headerData = new byte[MESSAGE_HEADER_SIZE];
                        Buffer.BlockCopy(e.Buffer, dataStartOffset, headerData, 0, MESSAGE_HEADER_SIZE);
                        var messageSize = BitConverter.ToInt32(headerData, 0);

                        token.MessageSize = messageSize;
                        token.DataStartOffset = dataStartOffset + MESSAGE_HEADER_SIZE;

                        dataStartOffset = token.DataStartOffset;
                        alreadyProcessedDataSize += MESSAGE_HEADER_SIZE;
                        continue;
                    }
                }
                else
                {
                    var messageSize = token.MessageSize.Value;
                    if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize)
                    {
                        var messageData = new byte[messageSize];
                        Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, messageSize);
                        ProcessMessage(messageData, token, e);

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

        private void ProcessMessage(byte[] messageData, AsyncUserToken token, SocketAsyncEventArgs e)
        {
            ReceivedMessage?.Invoke(new MessageData {Message = messageData, Token = token});
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            var token = e.UserToken as AsyncUserToken;
            token?.Dispose();
            _acceptedClientsSemaphore.Release();
            _socketAsyncReceiveEventArgsPool.Push(e);
        }
    }
}