namespace Blish_HUD.ArcDps.Models {

    /// <summary>
    ///     Infos and data about the combat event. Have a look at the arcdps plugin documentation for how to use it
    /// </summary>
    public class Ev {

        public ulong  Time            { get; }
        public ulong  SrcAgent        { get; }
        public ulong  DstAgent        { get; }
        public int    Value           { get; }
        public int    BuffDmg         { get; }
        public uint   OverStackValue  { get; }
        public uint   SkillId         { get; }
        public ushort SrcInstId       { get; }
        public ushort DstInstId       { get; }
        public ushort SrcMasterInstId { get; }
        public ushort DstMasterInstId { get; }
        public byte   Iff             { get; }
        public bool   Buff            { get; }
        public byte   Result          { get; }
        public bool   IsActivation    { get; }
        public bool   IsBuffRemove    { get; }
        public bool   IsNinety        { get; }
        public bool   IsFifty         { get; }
        public bool   IsMoving        { get; }
        public bool   IsStateChange   { get; }
        public bool   IsFlanking      { get; }
        public bool   IsShields       { get; }
        public bool   IsOffCycle      { get; }
        public byte   Pad61           { get; }
        public byte   Pad62           { get; }
        public byte   Pad63           { get; }
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