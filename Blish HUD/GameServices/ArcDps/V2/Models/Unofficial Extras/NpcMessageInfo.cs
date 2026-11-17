using System;

namespace Blish_HUD.GameServices.ArcDps.V2.Models.UnofficialExtras {
    public struct NpcMessageInfo {
        public string CharacterName { get; set; }

        public string Message { get; set; }
        
        /// <summary>
        /// Time since epoch in nanoseconds.
        /// This can be used to sort messages, when they are out of order.
        /// </summary>
        public ulong TimeStamp { get; set; }
    }
}