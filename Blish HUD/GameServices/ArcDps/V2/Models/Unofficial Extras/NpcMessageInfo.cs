using System;

namespace Blish_HUD.GameServices.ArcDps.V2.Models.UnofficialExtras {
    public struct NpcMessageInfo {
        public string CharacterName { get; set; }

        public string Message { get; set; }
        
        public ulong TimeStamp { get; set; }
    }
}