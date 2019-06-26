namespace Blish_HUD.ArcDps.Models
{
    public class Ag
    {
        public Ag(string name, ulong id, uint profession, uint elite, uint self, ushort team)
        {
            Name = name;
            Id = id;
            Profession = profession;
            Elite = elite;
            Self = self;
            Team = team;
        }

        public string Name { get; }
        public ulong Id { get; }
        public uint Profession { get; }
        public uint Elite { get; }
        public uint Self { get; }
        public ushort Team { get; }
    }
}
