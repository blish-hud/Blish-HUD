using Blish_HUD.GameServices.ArcDps.V2.Models;
using System;
using System.Text;

namespace Blish_HUD.GameServices.ArcDps.V2.Processors {
    internal class LegacyCombatProcessor : MessageProcessor<CombatCallback> {

        internal override bool TryInternalProcess(byte[] message, out CombatCallback result) {
            try {
                result = ProcessCombat(message);
                return true;

            } catch (Exception) {
                result = default;
                return false;
            }
        }

        public static CombatCallback ProcessCombat(byte[] data) {
            CombatEvent ev = default;
            Agent src = default;
            Agent dst = default;
            string skillName = null;
            int offset = 1;

            if ((byte)(data[0] & (byte)CombatMessageFlags.Ev) == (byte)CombatMessageFlags.Ev) (ev, offset) = ParseEv(data, offset);

            if ((byte)(data[0] & (byte)CombatMessageFlags.Src) == (byte)CombatMessageFlags.Src) (src, offset) = ParseAg(data, offset);

            if ((byte)(data[0] & (byte)CombatMessageFlags.Dst) == (byte)CombatMessageFlags.Dst) (dst, offset) = ParseAg(data, offset);

            if ((byte)(data[0] & (byte)CombatMessageFlags.SkillName) == (byte)CombatMessageFlags.SkillName) (skillName, offset) = ParseString(data, offset);

            ulong id = BitConverter.ToUInt64(data, offset);
            ulong revision = BitConverter.ToUInt64(data, offset + 8);

            return new CombatCallback() {
                Event = ev,
                Source = src,
                Destination = dst,
                SkillName = skillName,
                Id = id,
                Revision = revision,
            };
        }

        private static (CombatEvent, int) ParseEv(byte[] data, int offset) {
            ulong time;
            ulong srcAgent;
            ulong dstAgent;
            int value;
            int buffDmg;
            uint overStackValue;
            uint skillId;
            ushort srcInstId;
            ushort dstInstId;
            ushort srcMasterInstId;
            ushort dstMasterInstId;
            byte iff;
            bool buff;
            byte result;
            byte isActivation;
            byte isBuffRemove;
            bool isNinety;
            bool isFifty;
            bool isMoving;
            byte isStateChange;
            bool isFlanking;
            bool isShields;
            bool isOffCycle;
            byte pad61;
            byte pad62;
            byte pad63;
            byte pad64;
            (time, offset) = U64(data, offset);
            (srcAgent, offset) = U64(data, offset);
            (dstAgent, offset) = U64(data, offset);
            (value, offset) = I32(data, offset);
            (buffDmg, offset) = I32(data, offset);
            (overStackValue, offset) = U32(data, offset);
            (skillId, offset) = U32(data, offset);
            (srcInstId, offset) = U16(data, offset);
            (dstInstId, offset) = U16(data, offset);
            (srcMasterInstId, offset) = U16(data, offset);
            (dstMasterInstId, offset) = U16(data, offset);
            (iff, offset) = U8(data, offset);
            (buff, offset) = B(data, offset);
            (result, offset) = U8(data, offset);
            (isActivation, offset) = U8(data, offset);
            (isBuffRemove, offset) = U8(data, offset);
            (isNinety, offset) = B(data, offset);
            (isFifty, offset) = B(data, offset);
            (isMoving, offset) = B(data, offset);
            (isStateChange, offset) = U8(data, offset);
            (isFlanking, offset) = B(data, offset);
            (isShields, offset) = B(data, offset);
            (isOffCycle, offset) = B(data, offset);
            (pad61, offset) = U8(data, offset);
            (pad62, offset) = U8(data, offset);
            (pad63, offset) = U8(data, offset);
            (pad64, offset) = U8(data, offset);

            var ev = new CombatEvent() {
                Time = time,
                SourceAgent = srcAgent,
                DestinationAgent = dstAgent,
                Value = value,
                BuffDamage = buffDmg,
                OverstackValue = overStackValue,
                SkillId = skillId,
                SourceInstanceId = srcInstId,
                DestinationInstanceId = dstInstId,
                SourceMasterInstanceId = srcMasterInstId,
                DestinationMasterInstanceId = dstMasterInstId,
                Iff = (Affinity)iff,
                Buff = buff,
                Result = result,
                IsActivation = (Activation)isActivation,
                IsBuffRemoved = (BuffRemove)isBuffRemove,
                IsNinety = isNinety,
                IsFifty = isFifty,
                IsMoving = isMoving,
                IsStateChanged = (StateChange)isStateChange,
                IsFlanking = isFlanking,
                IsShiels = isShields,
                IsOffCycle = isOffCycle,
                Pad61 = pad61,
                Pad62 = pad62,
                Pad63 = pad63,
                Pad64 = pad64,
            };

            return (ev, offset);
        }

        private static (Agent, int) ParseAg(byte[] data, int offset) {
            string name;
            ulong id;
            uint profession;
            uint elite;
            uint self;
            ushort team;
            (name, offset) = ParseString(data, offset);
            (id, offset) = U64(data, offset);
            (profession, offset) = U32(data, offset);
            (elite, offset) = U32(data, offset);
            (self, offset) = U32(data, offset);
            (team, offset) = U16(data, offset);

            var ag = new Agent() {
                Name = name,
                Id = id,
                Profession = profession,
                Elite = elite,
                Self = self,
                Team = team,
            };

            return (ag, offset);
        }

        private static (string, int) ParseString(byte[] data, int offset) {
            ulong length;
            (length, offset) = U64(data, offset);
            string str = Encoding.UTF8.GetString(data, offset, (int)length);
            return (str, offset + (int)length);
        }

        private static (ulong, int) U64(byte[] data, int offset) {
            return (BitConverter.ToUInt64(data, offset), offset + 8);
        }

        private static (uint, int) U32(byte[] data, int offset) {
            return (BitConverter.ToUInt32(data, offset), offset + 4);
        }

        private static (int, int) I32(byte[] data, int offset) {
            return (BitConverter.ToInt32(data, offset), offset + 4);
        }

        private static (ushort, int) U16(byte[] data, int offset) {
            return (BitConverter.ToUInt16(data, offset), offset + 2);
        }

        private static (byte, int) U8(byte[] data, int offset) {
            return (data[offset], offset + 1);
        }

        private static (bool, int) B(byte[] data, int offset) {
            return (data[offset] != 0, offset + 1);
        }

        private enum CombatMessageFlags {

            Ev = 0x01,
            Src = 0x02,
            Dst = 0x04,
            SkillName = 0x08

        }
    }
}
