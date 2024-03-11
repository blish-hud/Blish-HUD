using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2.Processors {
    internal abstract class MessageProcessor<T> : MessageProcessor
        where T : struct {
        private readonly List<Func<T, CancellationToken, Task>> listeners = new List<Func<T, CancellationToken, Task>>();

        public override void Process(byte[] message, CancellationToken ct) {
            if (listeners.Count > 0 && TryInternalProcess(message, out var parsedMessage)) {
                ArrayPool<byte>.Shared.Return(message);
                Task.Run(async () => await SendToListener(parsedMessage, ct));
            }

        }

        private async Task SendToListener(T Message, CancellationToken ct) {
            foreach (var listener in listeners) {
                ct.ThrowIfCancellationRequested();
                await listener.Invoke(Message, ct);
            }
        }

        internal abstract bool TryInternalProcess(byte[] message, out T result);

        public void RegisterListener(Func<T, CancellationToken, Task> listener) {
            listeners.Add(listener);
        }

    }
}
