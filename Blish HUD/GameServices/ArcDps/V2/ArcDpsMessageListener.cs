using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    public class ArcDpsMessageListener<T> : IArcDpsMessageListener<T>
        where T : struct {
        private readonly Func<T, CancellationToken, Task> _listener;

        public MessageType MessageType { get; }

        public event Action<IArcDpsMessageListener<T>> Disposing;

        public ArcDpsMessageListener(MessageType type, Func<T, CancellationToken, Task> listener) {
            MessageType = type;
            _listener = listener;
        }

        public async Task HandleAsync(T message, CancellationToken ct)
            => await _listener(message, ct);

        public void Dispose() {
            Disposing?.Invoke(this);
        }
    }
}
