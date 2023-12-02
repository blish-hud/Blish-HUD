namespace Blish_HUD.GameServices.ArcDps.V2.Models {
    /// <summary>
    /// A combat event, like arcdps exposes it to its plugins
    /// </summary>
    public struct CombatCallback {
        /// <summary>
        /// The event data.
        /// </summary>
        public CombatEvent Event { get; set; }

        /// <summary>
        /// The agent or entity that caused this event.
        /// </summary>
        public Agent Source { get; set; }

        /// <summary>
        /// The agent or entity that this event is happening to.
        /// </summary>
        public Agent Destination { get; set; }

        /// <summary>
        /// The relevant skill name.
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// Unique identifier of this event.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Format of the data structure. Static unless ArcDps changes the format.
        /// </summary>
        /// <remarks>
        /// Used to distinguish different versions of the format.
        /// </remarks>
        public ulong Revision { get; set; }
    }
}