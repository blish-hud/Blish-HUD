using System;
using Blish_HUD.ArcDps.Models;

namespace Blish_HUD.ArcDps
{
    public class RawCombatEventArgs : EventArgs
    {
        public RawCombatEventArgs(CombatEvent combatEvent)
        {
            CombatEvent = combatEvent;
        }

        public CombatEvent CombatEvent { get; private set; }
    }
}
