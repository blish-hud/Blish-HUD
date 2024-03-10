using Blish_HUD.GameServices.ArcDps.V2.Extensions;
using Blish_HUD.GameServices.ArcDps.V2.Models;
using SharpDX;
using System;
using System.IO;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    internal class CombatEventProcessor : MessageProcessor<CombatCallback> {
        internal override bool TryInternalProcess(byte[] message, out CombatCallback result) {
            try {
                using var memoryStream = new MemoryStream(message);
                using var binaryReader = new BincodeBinaryReader(memoryStream);
                result = binaryReader.ParseCombatCallback();
                return true;

            } catch (Exception) {
                result = default;
                return false;
            }

        }
    }
}
