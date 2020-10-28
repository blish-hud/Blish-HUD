namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    /// A combat event, like arcdps exposes it to its plugins
    /// </summary>
    public class CombatEvent {

        /// <summary>
        /// The event data.
        /// </summary>
        public Ev     Ev        { get; }
        /// <summary>
        /// The agent or entity that caused this event.
        /// </summary>
        public Ag     Src       { get; }
        /// <summary>
        /// The agent or entity that this event is happening to.
        /// </summary>
        public Ag     Dst       { get; }
        /// <summary>
        /// The relevant skill name.
        /// </summary>
        public string SkillName { get; }
        /// <summary>
        /// Unique identifier of this event.
        /// </summary>
        public ulong  Id        { get; }
        /// <summary>
        /// Format of the data structure. Static unless ArcDps changes the format.
        /// </summary>
        /// <remarks>
        /// Used to distinguish different versions of the format.
        /// </remarks>
        public ulong  Revision  { get; }

        public CombatEvent(Ev ev, Ag src, Ag dst, string skillName, ulong id, ulong revision) {
            this.Ev        = ev;
            this.Src       = src;
            this.Dst       = dst;
            this.SkillName = skillName;
            this.Id        = id;
            this.Revision  = revision;
        }

    }

}