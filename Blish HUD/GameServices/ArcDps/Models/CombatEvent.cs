namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    ///     A combat event, like arcdps exposes it to its plugins
    /// </summary>
    public class CombatEvent {

        public Ev     Ev        { get; }
        public Ag     Src       { get; }
        public Ag     Dst       { get; }
        public string SkillName { get; }
        public ulong  Id        { get; }
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