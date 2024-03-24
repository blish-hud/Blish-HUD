namespace Blish_HUD.GameServices.ArcDps.V2.Models {
    /// <summary>
    /// An Agent. Could be anything that has behaviour in-game, for example a player or an NPC
    /// </summary>
    public struct Agent {

        /// <summary>
        /// The name of the agent.
        /// </summary>
        /// <remarks>
        /// Can be null.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Agent unique identifier.
        /// </summary>
        /// <remarks>
        /// Not unique between sessions.
        /// </remarks>
        public ulong Id { get; set; }

        /// <summary>
        /// Profession id at time of event.
        /// </summary>
        /// <remarks>
        /// Meaning differs per event type. Eg. "Species ID" for non-gadgets. See <see cref="https://www.deltaconnected.com/arcdps/evtc/">evtc notes</see> for details.
        /// </remarks>
        public uint Profession { get; set; }

        /// <summary>
        /// Elite specialization id at time of event. 
        /// </summary>
        /// <remarks>
        /// Meaning differs per event type. See <see cref="https://www.deltaconnected.com/arcdps/evtc/">evtc notes</see> for details.
        /// </remarks>
        public uint Elite { get; set; }

        /// <summary>
        /// One if this agent belongs to the account currently logged in on the local Guild Wars 2 instance. Zero otherwise.
        /// </summary>
        public uint Self { get; set; }

        /// <summary>
        /// Team unique identifier.
        /// </summary>
        public ushort Team { get; set; }
    }
}