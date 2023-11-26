using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps {
    internal abstract class MessageProcessor<T> : MessageProcessor
        where T : struct {
        private readonly List<Func<T, CancellationToken, Task>> listener;

        public override void Process(byte[] message, CancellationToken ct) {
            var parsedMessage = InternalProcess(message);
            ArrayPool<byte>.Shared.Return(message);
            Task.Run(async () => await this.SendToListener(parsedMessage, ct));

        }

        private async Task SendToListener(T Message, CancellationToken ct) {
            foreach (var listener in this.listener) {
                ct.ThrowIfCancellationRequested();
                await listener.Invoke(Message, ct);
            }
        }

        internal abstract T InternalProcess(byte[] message);

        public void RegisterListener(Func<T, CancellationToken, Task> listener) {
            this.listener.Add(listener);
        }

    }
}
