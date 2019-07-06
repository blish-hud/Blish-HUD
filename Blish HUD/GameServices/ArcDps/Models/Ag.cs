namespace Blish_HUD.ArcDps.Models
{
    /// <summary>
    ///     An Agent. Could be anything that has behaviour in-game, for example a player or an NPC
    /// </summary>
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