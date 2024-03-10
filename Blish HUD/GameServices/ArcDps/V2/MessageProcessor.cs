using System.Threading;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    internal abstract class MessageProcessor {

        public abstract void Process(byte[] message, CancellationToken ct);
    }
}
