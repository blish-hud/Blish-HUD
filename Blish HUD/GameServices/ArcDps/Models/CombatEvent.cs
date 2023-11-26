namespace Blish_HUD.GameServices.ArcDps.Models {
    /// <summary>
    /// Infos and data about the combat event.
    /// </summary>
    /// <remarks>
    /// For more information see the <see cref="https://deltaconnected.com/arcdps/api/">arcdps plugin documentation</see>.
    /// </remarks>
    public struct CombatEvent {
        
        /// <summary>
        /// Time when the event was registered.
        /// </summary>
        /// <remarks>
        /// System specific time since boot, not the actual time in a known format.
        /// </remarks>
        public ulong Time { get; set; }

        /// <summary>
        /// Map instance <seealso cref="Agent.Id">agent id</seealso> that caused the event.
        /// </summary>
        /// <remarks>
        /// Aka. entity id in-game.
        /// </remarks>
        public ulong SourceAgent { get; set; }

        /// <summary>
        /// Map instance <seealso cref="Agent.Id">agent id</seealso> that this event happened to.
        /// </summary>
        /// <remarks>
        /// Aka. entity id in-game.
        /// </remarks>
        public ulong DestinationAgent { get; set; }

        /// <summary>
        /// An event-specific value.
        /// </summary>
        /// <remarks>
        /// Meaning differs per event-type. Eg. estimated physical hit damage. See <see cref="https://www.deltaconnected.com/arcdps/evtc/">evtc notes</see> for details.
        /// </remarks>
        public int Value { get; set; }

        /// <summary>
        /// Estimated buff damage. Zero on application event.
        /// </summary>
        public int BuffDamage { get; set; }

        /// <summary>
        /// Estimated overwritten stack duration for buff application.
        /// </summary>
        public uint OverstackValue { get; set; }

        /// <summary>
        /// Skill id of relevant skill.
        /// </summary>
        /// <remarks>
        /// Can be zero.
        /// </remarks>
        public uint SkillId { get; set; }

        /// <summary>
        /// Map instance agent id as it appears in-game at time of event.
        /// </summary>
        public ushort SourceInstanceId { get; set; }

        /// <summary>
        /// Map instance agent id as it appears in-game at time of event.
        /// </summary>
        public ushort DestinationInstanceId { get; set; }

        /// <summary>
        /// If <seealso cref="SourceAgent">SourceAgent</seealso> has a master (eg. minion, pet), this field will be equal to the <seealso cref="Agent.Id">agent id</seealso> of the master. Otherwise zero.
        /// </summary>
        public ushort SourceMasterInstanceId { get; set; }

        /// <summary>
        /// If <seealso cref="DstAgent">DstAgent</seealso> has a master (eg. minion, pet), this field will be equal to the <seealso cref="Agent.Id">agent id</seealso> of the master. Otherwise zero.
        /// </summary>
        public ushort DestinationMasterInstanceId { get; set; }

        /// <summary>
        /// Current affinity of <seealso cref="SourceAgent">SourceAgent</seealso> and <seealso cref="DestinationAgent">DestinationAgent</seealso>.
        /// </summary>
        /// <remarks>
        /// Friend = 0, foe = 1, unknown = 2.
        /// </remarks>
        public Affinity Iff { get; set; }

        /// <summary>
        /// <see langword="True"/> if buff was applied, removed or damaging. Otherwise <see langword="false"/>.
        /// </summary>
        public bool Buff { get; set; }

        /// <summary>
        /// <seealso cref="PhysicalResult"/> or <see cref="ConditionResult"/>.
        /// </summary>
        /// <remarks>
        /// See <see cref="https://www.deltaconnected.com/arcdps/evtc/">evtc notes</see> for details.
        /// </remarks>
        public byte Result { get; set; }

        /// <summary>
        /// The event is bound to the usage or cancellation of a skill. <seealso cref="Activation"/>.
        /// </summary>
        /// <remarks>
        /// The type of <see cref="Activation"/>/>
        /// </remarks>
        public Activation IsActivation { get; set; }

        /// <summary>
        /// <seealso cref="BuffRemove"/>.
        /// </summary>
        /// <remarks>
        /// For strips and cleanses: <seealso cref="SourceAgent">SourceAgent</seealso> = relevant, <seealso cref="DestinationAgent">DestinationAgent</seealso> = caused it.
        /// </remarks>
        public BuffRemove IsBuffRemoved { get; set; }

        /// <summary>
        /// <see langword="True"/> if <seealso cref="SourceAgent">SourceAgent</seealso> is above 90% health. Otherwise <see langword="false"/>.
        /// </summary>
        public bool IsNinety { get; set; }

        /// <summary>
        /// <see langword="True"/> if <seealso cref="DestinationAgent">DestinationAgent</seealso> is below 50% health. Otherwise <see langword="false"/>.
        /// </summary>
        public bool IsFifty { get; set; }

        /// <summary>
        /// <see langword="True"/> if <seealso cref="SourceAgent">SourceAgent</seealso> is moving at time of event. Otherwise <see langword="false"/>.
        /// </summary>
        public bool IsMoving { get; set; }

        /// <summary>
        /// Type of <see cref="StateChange"/> that occured.
        /// </summary>
        /// <remarks>
        /// <seealso cref="SourceAgent">SourceAgent</seealso> is now alive, dead, downed and other ambiguous stuff eg. when <seealso cref="SourceAgent">SourceAgent</seealso> is <seealso cref="Agent.Self">Self</seealso>, <seealso cref="DestinationAgent">DestinationAgent</seealso> is a reward id and <seealso cref="Value">Value</seealso> is a reward type such as a wiggly box.
        /// </remarks>
        public StateChange IsStateChanged { get; set; }

        /// <summary>
        /// <see langword="True"/> if <seealso cref="SourceAgent">SourceAgent</seealso> is flanking <seealso cref="DestinationAgent">DestinationAgent</seealso> at time of event. Otherwise <see langword="false"/>.
        /// </summary>
        public bool IsFlanking { get; set; }

        /// <summary>
        /// <see langword="True"/> if all or part of damage was VS. barrier or shield. Otherwise <see langword="false"/>.
        /// </summary>
        public bool IsShiels { get; set; }

        /// <summary>
        /// <see langword="True"/> if no buff damage happened during tick. Otherwise <see langword="false"/>.
        /// </summary>
        public bool IsOffCycle { get; set; }

        /// <summary>
        /// Buff instance id of buff applied. Non-zero if no buff damage happened during tick. Otherwise zero.
        /// </summary>
        public byte Pad61 { get; set; }

        /// <summary>
        /// Buff instance id of buff applied.
        /// </summary>
        public byte Pad62 { get; set; }

        /// <summary>
        /// Buff instance id of buff applied.
        /// </summary>
        public byte Pad63 { get; set; }

        /// <summary>
        /// Buff instance id of buff applied.
        /// </summary>
        /// <remarks>
        /// Used for internal tracking (garbage).
        /// </remarks>
        public byte Pad64 { get; set; }
    }

}