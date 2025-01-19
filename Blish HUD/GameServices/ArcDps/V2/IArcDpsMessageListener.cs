using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    public interface IArcDpsMessageListener<T> : IDisposable
        where T : struct {
        event Action<IArcDpsMessageListener<T>> Disposing;

        MessageType MessageType { get; }

        Task HandleAsync(T message, CancellationToken ct);
    }

    public class ArcDpsMessageListener<T> : IArcDpsMessageListener<T>
        where T : struct {
        private readonly Func<T, CancellationToken, Task> listener;

        public MessageType MessageType { get; }

        public event Action<IArcDpsMessageListener<T>> Disposing;

        public ArcDpsMessageListener(MessageType type, Func<T, CancellationToken, Task> listener) {
            this.MessageType = type;
            this.listener = listener;
        }

        public async Task HandleAsync(T message, CancellationToken ct)
            => await this.listener(message, ct);

        public void Dispose() {
            this.Disposing?.Invoke(this);
        }
    }
}
