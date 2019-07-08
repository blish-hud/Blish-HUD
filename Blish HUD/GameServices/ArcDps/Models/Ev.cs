namespace Blish_HUD.ArcDps.Models
{
    /// <summary>
    ///     Infos and data about the combat event. Have a look at the arcdps plugin documentation for how to use it
    /// </summary>
    public class Ev
    {
        public Ev(ulong time, ulong srcAgent, ulong dstAgent, int value, int buffDmg, uint overStackValue, uint skillId,
            ushort srcInstId, ushort dstInstId, ushort srcMasterInstId, ushort dstMasterInstId, byte iff, byte buff,
            byte result, bool isActivation, bool isBuffRemove, bool isNinety, bool isFifty, bool isMoving,
            bool isStateChange, bool isFlanking, bool isShields, bool isOffCycle, byte pad61, byte pad62, byte pad63,
            byte pad64)
        {
            Time = time;
            SrcAgent = srcAgent;
            DstAgent = dstAgent;
            Value = value;
            BuffDmg = buffDmg;
            OverStackValue = overStackValue;
            SkillId = skillId;
            SrcInstId = srcInstId;
            DstInstId = dstInstId;
            SrcMasterInstId = srcMasterInstId;
            DstMasterInstId = dstMasterInstId;
            Iff = iff;
            Buff = buff;
            Result = result;
            IsActivation = isActivation;
            IsBuffRemove = isBuffRemove;
            IsNinety = isNinety;
            IsFifty = isFifty;
            IsMoving = isMoving;
            IsStateChange = isStateChange;
            IsFlanking = isFlanking;
            IsShields = isShields;
            IsOffCycle = isOffCycle;
            Pad61 = pad61;
            Pad62 = pad62;
            Pad63 = pad63;
            Pad64 = pad64;
        }

        public ulong Time { get; }
        public ulong SrcAgent { get; }
        public ulong DstAgent { get; }
        public int Value { get; }
        public int BuffDmg { get; }
        public uint OverStackValue { get; }
        public uint SkillId { get; }
        public ushort SrcInstId { get; }
        public ushort DstInstId { get; }
        public ushort SrcMasterInstId { get; }
        public ushort DstMasterInstId { get; }
        public byte Iff { get; }
        public byte Buff { get; }
        public byte Result { get; }
        public bool IsActivation { get; }
        public bool IsBuffRemove { get; }
        public bool IsNinety { get; }
        public bool IsFifty { get; }
        public bool IsMoving { get; }
        public bool IsStateChange { get; }
        public bool IsFlanking { get; }
        public bool IsShields { get; }
        public bool IsOffCycle { get; }
        public byte Pad61 { get; }
        public byte Pad62 { get; }
        public byte Pad63 { get; }
        public byte Pad64 { get; }
    }
}