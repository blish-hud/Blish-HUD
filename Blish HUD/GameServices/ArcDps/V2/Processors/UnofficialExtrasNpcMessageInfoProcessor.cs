using Blish_HUD.GameServices.ArcDps.V2.Models.UnofficialExtras;
using Blish_HUD.GameServices.ArcDps.V2.Extensions;
using Blish_HUD.GameServices.ArcDps.V2.Processors;
using System;
using System.IO;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    internal class UnofficialExtrasNpcMessageInfoProcessor : MessageProcessor<NpcMessageInfo> {
        internal override bool TryInternalProcess(byte[] message, out NpcMessageInfo result) {
            try {
                using var memoryStream = new MemoryStream(message);
                using var binaryReader = new BincodeBinaryReader(memoryStream);
                result = binaryReader.ParseNpcMessageInfo();
                return true;
            } catch (Exception) {
                result = default;
                return false;
            }
        }
    }
}