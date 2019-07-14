using System;
using System.Text;
using Blish_HUD.ArcDps.Models;

namespace Blish_HUD.ArcDps
{
    internal static class CombatParser
    {
        public static CombatEvent ProcessCombat(byte[] data)
        {
            Ev ev = null;
            Ag src = null;
            Ag dst = null;
            string skillName = null;
            var offset = 2;

            if ((byte) (data[1] & (byte) CombatMessageFlags.Ev) == (byte) CombatMessageFlags.Ev)
                (ev, offset) = ParseEv(data, offset);

            if ((byte) (data[1] & (byte) CombatMessageFlags.Src) == (byte) CombatMessageFlags.Src)
                (src, offset) = ParseAg(data, offset);

            if ((byte) (data[1] & (byte) CombatMessageFlags.Dst) == (byte) CombatMessageFlags.Dst)
                (dst, offset) = ParseAg(data, offset);

            if ((byte) (data[1] & (byte) CombatMessageFlags.SkillName) == (byte) CombatMessageFlags.SkillName)
                (skillName, offset) = ParseString(data, offset);

            var id = BitConverter.ToUInt64(data, offset);
            var revision = BitConverter.ToUInt64(data, offset + 8);
            return new CombatEvent(ev, src, dst, skillName, id, revision);
        }

        private static (Ev, int) ParseEv(byte[] data, int offset)
        {
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
            bool isActivation;
            bool isBuffRemove;
            bool isNinety;
            bool isFifty;
            bool isMoving;
            bool isStateChange;
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
            (isActivation, offset) = B(data, offset);
            (isBuffRemove, offset) = B(data, offset);
            (isNinety, offset) = B(data, offset);
            (isFifty, offset) = B(data, offset);
            (isMoving, offset) = B(data, offset);
            (isStateChange, offset) = B(data, offset);
            (isFlanking, offset) = B(data, offset);
            (isShields, offset) = B(data, offset);
            (isOffCycle, offset) = B(data, offset);
            (pad61, offset) = U8(data, offset);
            (pad62, offset) = U8(data, offset);
            (pad63, offset) = U8(data, offset);
            (pad64, offset) = U8(data, offset);
            var ev = new Ev(time, srcAgent, dstAgent, value, buffDmg, overStackValue, skillId, srcInstId, dstInstId,
                srcMasterInstId, dstMasterInstId, iff, buff, result, isActivation, isBuffRemove, isNinety, isFifty,
                isMoving, isStateChange,
                isFlanking, isShields, isOffCycle, pad61, pad62, pad63, pad64);
            return (ev, offset);
        }

        private static (Ag, int) ParseAg(byte[] data, int offset)
        {
            string name; ulong id; uint profession; uint elite; uint self;
            ushort team;
            (name, offset) = ParseString(data, offset);
            (id, offset) = U64(data, offset);
            (profession, offset) = U32(data, offset);
            (elite, offset) = U32(data, offset);
            (self, offset) = U32(data, offset);
            (team, offset) = U16(data, offset);
            var ag = new Ag(name, id, profession, elite, self, team);
            return (ag, offset);
        }

        private static (string, int) ParseString(byte[] data, int offset)
        {
            ulong length;
            (length, offset) = U64(data, offset);
            var str = Encoding.Default.GetString(data, offset, (int)length);
            return (str, offset+(int)length);
        }

        private static (ulong, int) U64(byte[] data, int offset)
        {
            return (BitConverter.ToUInt64(data, offset), offset + 8);
        }

        private static (uint, int) U32(byte[] data, int offset)
        {
            return (BitConverter.ToUInt32(data, offset), offset + 4);
        }

        private static (int, int) I32(byte[] data, int offset)
        {
            return (BitConverter.ToInt32(data, offset), offset + 4);
        }

        private static (ushort, int) U16(byte[] data, int offset)
        {
            return (BitConverter.ToUInt16(data, offset), offset + 2);
        }

        private static (byte, int) U8(byte[] data, int offset)
        {
            return (data[offset], offset + 1);
        }

        private static (bool, int) B(byte[] data, int offset)
        {
            return (data[offset] != 0, offset + 1);
        }

        private enum CombatMessageFlags
        {
            Ev = 0x01,
            Src = 0x02,
            Dst = 0x04,
            SkillName = 0x08
        }
    }
}