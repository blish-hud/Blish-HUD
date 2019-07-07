using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Blish_HUD.ArcDps
{
    public sealed class SocketAsyncEventArgsPool
    {
        private readonly ConcurrentQueue<SocketAsyncEventArgs> _queue;

        public SocketAsyncEventArgsPool()
        {
            _queue = new ConcurrentQueue<SocketAsyncEventArgs>();
        }

        public SocketAsyncEventArgs Pop()
        {
            return _queue.TryDequeue(out var args) ? args : null;
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            _queue.Enqueue(item);
        }
    }
}