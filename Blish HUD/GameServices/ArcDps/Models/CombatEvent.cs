namespace Blish_HUD.ArcDps.Models
{
    public class CombatEvent
    {
        public CombatEvent(Ev ev, Ag src, Ag dst, string skillName, ulong id, ulong revision)
        {
            Ev = ev;
            Src = src;
            Dst = dst;
            SkillName = skillName;
            Id = id;
            Revision = revision;
        }

        public Ev Ev { get; }
        public Ag Src { get; }
        public Ag Dst { get; }
        public string SkillName { get; }
        public ulong Id { get; }
        public ulong Revision { get; }

    }
}
