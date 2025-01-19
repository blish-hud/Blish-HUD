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
}
