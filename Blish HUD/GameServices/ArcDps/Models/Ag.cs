namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    ///     An Agent. Could be anything that has behaviour in-game, for example a player or an NPC
    /// </summary>
    public class Ag {

        public string Name       { get; }
        public ulong  Id         { get; }
        public uint   Profession { get; }
        public uint   Elite      { get; }
        public uint   Self       { get; }
        public ushort Team       { get; }

        public Ag(string name, ulong id, uint profession, uint elite, uint self, ushort team) {
            this.Name       = name;
            this.Id         = id;
            this.Profession = profession;
            this.Elite      = elite;
            this.Self       = self;
            this.Team       = team;
        }

    }

}