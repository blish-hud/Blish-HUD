using System;
using Blish_HUD.ArcDps.Models;

namespace Blish_HUD.ArcDps {

    public class RawCombatEventArgs : EventArgs {

        public enum CombatEventType {

            /// <summary>
            /// ArcDps calculations and displayed estimates.
            /// </summary>
            Area,
            /// <summary>
            /// Exact and sole recording of the player character bound to the signed-in account of this Guild Wars 2 instance.
            /// </summary>
            /// <remarks>
            /// Similar to the in-game combat log in the chat window.
            /// </remarks>
            Local

        }

        public CombatEventType EventType { get; }

        public CombatEvent CombatEvent { get; }

        public RawCombatEventArgs(CombatEvent combatEvent, CombatEventType eventType) {
            this.CombatEvent = combatEvent;
            this.EventType   = eventType;
        }

    }

}