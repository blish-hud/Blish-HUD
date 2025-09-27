using Blish_HUD.GameServices.ArcDps.Models.UnofficialExtras;
using Blish_HUD.GameServices.ArcDps.V2.Extensions;
using Blish_HUD.GameServices.ArcDps.V2.Models;
using Blish_HUD.GameServices.ArcDps.V2.Processors;
using SharpDX;
using System;
using System.IO;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    internal class UnofficialExtrasMessageInfoProcessor : MessageProcessor<ChatMessageInfo> {
        internal override bool TryInternalProcess(byte[] message, out ChatMessageInfo result) {
            try {
                using var memoryStream = new MemoryStream(message);
                using var binaryReader = new BincodeBinaryReader(memoryStream);
                result = binaryReader.ParseChatMessageInfo();
                return true;
            } catch (Exception) {
                result = default;
                return false;
            }
        }
    }
}
