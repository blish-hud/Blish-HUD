namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    ///     Infos and data about the combat event. For more information see the arcdps plugin documentation.
    /// </summary>
    public class Ev {

        /// <summary>
        /// Time when the event was registered.
        /// </summary>
        public ulong  Time            { get; }
        /// <summary>
        /// Map instance agent id that caused the event (aka. entity id in-game).
        /// </summary>
        public ulong  SrcAgent        { get; }
        /// <summary>
        /// Map instance agent id that this event happened to (aka. entity id in-game).
        /// </summary>
        public ulong  DstAgent        { get; }
        /// <summary>
        /// Event-specific.
        /// </summary>
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
        /// Skill id of relevant skill (can be zero).
        /// </summary>
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
        /// If SrcAgent has a master (eg. minion, pet), this field will be equal to the map instance agent id of the master, zero otherwise.
        /// </summary>
        public ushort SrcMasterInstId { get; }
        /// <summary>
        /// If DstAgent has a master (eg. minion, pet), this field will be equal to the map instance agent id of the master, zero otherwise.
        /// </summary>
        public ushort DstMasterInstId { get; }
        /// <summary>
        /// Current affinity of SrcAgent and DstAgent (friend = 0, foe = 1 or unknown = 2).
        /// </summary>
        public byte   Iff             { get; }
        /// <summary>
        /// Buff application, removal, or damage event.
        /// </summary>
        public bool   Buff            { get; }
        /// <summary>
        /// If the physical hit was of special kind (critial, glance, interupted target, downing blow, killing blow, was absorbed, blocked or evaded, etc.). A good normal hit will be false.
        /// </summary>
        public byte   Result          { get; }
        /// <summary>
        /// If the event is bound to the usage or cancellation of a skill (cast start to cast finish or cast cancel).
        /// </summary>
        public bool   IsActivation    { get; }
        /// <summary>
        /// Buff removed. SrcAgent = relevant, DstAgent = caused it (for strips/cleanses).
        /// </summary>
        public bool   IsBuffRemove    { get; }
        /// <summary>
        /// If SrcAgent is above 90% health.
        /// </summary>
        public bool   IsNinety        { get; }
        /// <summary>
        /// If DstAgent is below 50% health.
        /// </summary>
        public bool   IsFifty         { get; }
        /// <summary>
        /// If SrcAgent is moving at time of event.
        /// </summary>
        public bool   IsMoving        { get; }
        /// <summary>
        /// If a state change occured (SrcAgent is now alive, dead, downed and other ambigious stuff ex. when SrcAgent is self, DstAgent is a reward id and Value is a reward type such as a wiggly box.)
        /// </summary>
        public bool   IsStateChange   { get; }
        /// <summary>
        /// If SrcAgent is flanking DstAgent at time of event.
        /// </summary>
        public bool   IsFlanking      { get; }
        /// <summary>
        /// True if all or part of damage was VS. barrier/shield, otherwise False.
        /// </summary>
        public bool   IsShields       { get; }
        /// <summary>
        /// False if buff dmg happened during tick, otherwise True.
        /// </summary>
        public bool   IsOffCycle      { get; }
        /// <summary>
        /// Buff instance id of buff applied. Zero if buff damage happened during tick, non-zero otherwise.
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
        /// Buff instance id of buff applied. Used for internal tracking (garbage).
        /// </summary>
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