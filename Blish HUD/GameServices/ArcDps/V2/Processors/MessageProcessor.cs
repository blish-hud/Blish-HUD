using System.Threading;

namespace Blish_HUD.GameServices.ArcDps.V2.Processors {
    internal abstract class MessageProcessor {

        public abstract void Process(byte[] message, CancellationToken ct);
    }
}
