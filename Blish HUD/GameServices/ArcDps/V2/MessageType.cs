using System;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    public enum MessageType {
        // ArcDPS
        ImGui            = 1,
        CombatEventArea  = 2,
        CombatEventLocal = 3,
        // Unofficial Extras
        UserInfo         = 4,
        SquadMessage     = 5,
        NpcMessage       = 6
    }
}
