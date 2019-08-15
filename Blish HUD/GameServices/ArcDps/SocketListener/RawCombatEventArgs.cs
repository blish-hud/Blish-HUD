using System;
using Blish_HUD.ArcDps.Models;

namespace Blish_HUD.ArcDps
{
    public class RawCombatEventArgs : EventArgs
    {
        public RawCombatEventArgs(CombatEvent combatEvent, CombatEventType eventType)
        {
            CombatEvent = combatEvent;
            EventType = eventType;
        }

        public CombatEventType EventType { get; }

        public CombatEvent CombatEvent { get; }

        public enum CombatEventType
        {
            Area,
            Local
        }
    }
}
