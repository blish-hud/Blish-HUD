namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    /// Infos and data about the combat event.
    /// </summary>
    /// <remarks>
    /// For more information see the <see cref="https://deltaconnected.com/arcdps/api/">arcdps plugin documentation</see>.
    /// </remarks>
    public class Ev {

        /// <summary>
        /// Time when the event was registered.
        /// </summary>
        /// <remarks>
        /// System specific time since boot, not the actual time in a known format.
        /// </remarks>
        public ulong  Time            { get; }
        /// <summary>
        /// Map instance <seealso cref="Ag.Id">agent id</seealso> that caused the event.
        /// </summary>
        /// <remarks>
        /// Aka. entity id in-game.
        /// </remarks>
        public ulong  SrcAgent        { get; }
        /// <summary>
        /// Map instance <seealso cref="Ag.Id">agent id</seealso> that this event happened to.
        /// </summary>
        /// <remarks>
        /// Aka. entity id in-game.
        /// </remarks>
        public ulong  DstAgent        { get; }
        /// <summary>
        /// An event-specific value.
        /// </summary>
        /// <remarks>
        /// Meaning differs per event-type. Eg. estimated physical hit damage. See <see cref="https://www.deltaconnected.com/arcdps/evtc/">evtc notes</see> for details.
        /// </remarks>
        public int    Value           { get; }
        /// <summary>
        /// Estimated buff damage. Zero on application event.
        /// </summary>
        public int    BuffDmg         { get; }
        /// <summary>
        /// Estimated overwritten stack duration for buff application.
        /// </summary>
        public uint   OverStackValue  { get; }
        /// <summary>
        /// Skill id of relevant skill.
        /// </summary>
        /// <remarks>
        /// Can be zero.
        /// </remarks>
        public uint   SkillId         { get; }
        /// <summary>
        /// Map instance agent id as it appears in-game at time of event.
        /// </summary>
        public ushort SrcInstId       { get; }
        /// <summary>
        /// Map instance agent id as it appears in-game at time of event.
        /// </summary>
        public ushort DstInstId       { get; }
        /// <summary>
        /// If <seealso cref="SrcAgent">SrcAgent</seealso> has a master (eg. minion, pet), this field will be equal to the <seealso cref="Ag.Id">agent id</seealso> of the master. Otherwise zero.
        /// </summary>
        public ushort SrcMasterInstId { get; }
        /// <summary>
        /// If <seealso cref="DstAgent">DstAgent</seealso> has a master (eg. minion, pet), this field will be equal to the <seealso cref="Ag.Id">agent id</seealso> of the master. Otherwise zero.
        /// </summary>
        public ushort DstMasterInstId { get; }
        /// <summary>
        /// Current affinity of <seealso cref="SrcAgent">SrcAgent</seealso> and <seealso cref="DstAgent">DstAgent</seealso>.
        /// </summary>
        /// <remarks>
        /// Friend = 0, foe = 1, unknown = 2.
        /// </remarks>
        public byte   Iff             { get; }
        /// <summary>
        /// <see langword="True"/> if buff was applied, removed or damaging. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   Buff            { get; }
        /// <summary>
        /// Physical Hit Result.
        /// </summary>
        /// <remarks>
        /// Normal hit = 0, was critical = 1, was glance =  2, was blocked = 3, was evaded = 4, interrupted the target = 5, was absorbed = 6, missed = 7, killed the target = 8, downed the target = 9.
        /// </remarks>
        public byte   Result          { get; }
        /// <summary>
        /// <see langword="True"/> if the event is bound to the usage or cancellation of a skill. Otherwise <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// <see langword="True"/> from cast start to cast finish or cast cancel.
        /// </remarks>
        public bool   IsActivation    { get; }
        /// <summary>
        /// <see langword="True"/> if buff was removed. Otherwise <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// For strips and cleanses: <seealso cref="SrcAgent">SrcAgent</seealso> = relevant, <seealso cref="DstAgent">DstAgent</seealso> = caused it.
        /// </remarks>
        public bool   IsBuffRemove    { get; }
        /// <summary>
        /// <see langword="True"/> if <seealso cref="SrcAgent">SrcAgent</seealso> is above 90% health. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   IsNinety        { get; }
        /// <summary>
        /// <see langword="True"/> if <seealso cref="DstAgent">DstAgent</seealso> is below 50% health. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   IsFifty         { get; }
        /// <summary>
        /// <see langword="True"/> if <seealso cref="SrcAgent">SrcAgent</seealso> is moving at time of event. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   IsMoving        { get; }
        /// <summary>
        /// <see langword="True"/> if a state change occured. Otherwise <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// <seealso cref="SrcAgent">SrcAgent</seealso> is now alive, dead, downed and other ambiguous stuff eg. when <seealso cref="SrcAgent">SrcAgent</seealso> is <seealso cref="Ag.Self">Self</seealso>, <seealso cref="DstAgent">DstAgent</seealso> is a reward id and <seealso cref="Value">Value</seealso> is a reward type such as a wiggly box.
        /// </remarks>
        public bool   IsStateChange   { get; }
        /// <summary>
        /// <see langword="True"/> if <seealso cref="SrcAgent">SrcAgent</seealso> is flanking <seealso cref="DstAgent">DstAgent</seealso> at time of event. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   IsFlanking      { get; }
        /// <summary>
        /// <see langword="True"/> if all or part of damage was VS. barrier or shield. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   IsShields       { get; }
        /// <summary>
        /// <see langword="True"/> if no buff damage happened during tick. Otherwise <see langword="false"/>.
        /// </summary>
        public bool   IsOffCycle      { get; }
        /// <summary>
        /// Buff instance id of buff applied. Non-zero if no buff damage happened during tick. Otherwise zero.
        /// </summary>
        public byte   Pad61           { get; }
        /// <summary>
        /// Buff instance id of buff applied.
        /// </summary>
        public byte   Pad62           { get; }
        /// <summary>
        /// Buff instance id of buff applied.
        /// </summary>
        public byte   Pad63           { get; }
        /// <summary>
        /// Buff instance id of buff applied.
        /// </summary>
        /// <remarks>
        /// Used for internal tracking (garbage).
        /// </remarks>
        public byte   Pad64           { get; }

        public Ev(
            ulong  time,          ulong  srcAgent,     ulong  dstAgent,        int    value,           int  buffDmg, uint overStackValue, uint skillId,
            ushort srcInstId,     ushort dstInstId,    ushort srcMasterInstId, ushort dstMasterInstId, byte iff,     bool buff,
            byte   result,        bool   isActivation, bool   isBuffRemove,    bool   isNinety,        bool isFifty, bool isMoving,
            bool   isStateChange, bool   isFlanking,   bool   isShields,       bool   isOffCycle,      byte pad61,   byte pad62, byte pad63,
            byte   pad64
        ) {
            this.Time            = time;
            this.SrcAgent        = srcAgent;
            this.DstAgent        = dstAgent;
            this.Value           = value;
            this.BuffDmg         = buffDmg;
            this.OverStackValue  = overStackValue;
            this.SkillId         = skillId;
            this.SrcInstId       = srcInstId;
            this.DstInstId       = dstInstId;
            this.SrcMasterInstId = srcMasterInstId;
            this.DstMasterInstId = dstMasterInstId;
            this.Iff             = iff;
            this.Buff            = buff;
            this.Result          = result;
            this.IsActivation    = isActivation;
            this.IsBuffRemove    = isBuffRemove;
            this.IsNinety        = isNinety;
            this.IsFifty         = isFifty;
            this.IsMoving        = isMoving;
            this.IsStateChange   = isStateChange;
            this.IsFlanking      = isFlanking;
            this.IsShields       = isShields;
            this.IsOffCycle      = isOffCycle;
            this.Pad61           = pad61;
            this.Pad62           = pad62;
            this.Pad63           = pad63;
            this.Pad64           = pad64;
        }

    }

}