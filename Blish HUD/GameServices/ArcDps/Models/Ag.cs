namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    ///     An Agent. Could be anything that has behaviour in-game, for example a player or an NPC
    /// </summary>
    public class Ag {

        /// <summary>
        /// Agent name. May be null. Valid only at time of event.
        /// </summary>
        public string Name       { get; }
        /// <summary>
        /// Agent unique identifier.
        /// </summary>
        public ulong  Id         { get; }
        /// <summary>
        /// Profession at time of event. Could be anything (see evtc notes for details).
        /// </summary>
        public uint   Profession { get; }
        /// <summary>
        /// Elite specialization map instance agent id at time of event. Could be anything (see evtc notes for details).
        /// </summary>
        public uint   Elite      { get; }
        /// <summary>
        /// One if this agent belongs to the account currently logged in on this Guild Wars 2 instance. Zero otherwise.
        /// </summary>
        public uint   Self       { get; }
        /// <summary>
        /// Team unique identifier (Sep21+).
        /// </summary>
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