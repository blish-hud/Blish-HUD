using Blish_HUD.GameServices.ArcDps.Extensions;
using Blish_HUD.GameServices.ArcDps.Models;
using System.IO;

namespace Blish_HUD.GameServices.ArcDps {
    internal class CombatEventProcessor : MessageProcessor<CombatCallback> {
        internal override CombatCallback InternalProcess(byte[] message) {
            using var memoryStream = new MemoryStream(message);
            using var binaryReader = new BincodeBinaryReader(memoryStream);
            return binaryReader.ParseCombatCallback();
        }
    }
}
