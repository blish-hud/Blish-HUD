using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2.Processors {
    internal abstract class MessageProcessor<T> : MessageProcessor
        where T : struct {
        private readonly List<IArcDpsMessageListener<T>> listeners = new List<IArcDpsMessageListener<T>>();

        public override void Process(byte[] message, CancellationToken ct) {
            if (listeners.Count > 0 && TryInternalProcess(message, out var parsedMessage)) {
                Task.Run(async () => await SendToListener(parsedMessage, ct));
            }

        }

        private async Task SendToListener(T Message, CancellationToken ct) {
            foreach (var listener in listeners.ToArray()) {
                ct.ThrowIfCancellationRequested();
                await listener.HandleAsync(Message, ct);
            }
        }

        internal abstract bool TryInternalProcess(byte[] message, out T result);

        public void RegisterListener(IArcDpsMessageListener<T> listener) {
            listeners.Add(listener);
            listener.Disposing += RemoveListener;
        }

        private void RemoveListener(IArcDpsMessageListener<T> listener) {
            listener.Disposing -= RemoveListener;
            listeners.Remove(listener);
        }

    }
}
