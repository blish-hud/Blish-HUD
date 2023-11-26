using System.Threading;

namespace Blish_HUD.GameServices.ArcDps {
    internal abstract class MessageProcessor {

        public abstract void Process(byte[] message, CancellationToken ct);
    }
}
