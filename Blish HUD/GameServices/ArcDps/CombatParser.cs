using System;
using System.Text;
using Blish_HUD.ArcDps.Models;

namespace Blish_HUD.ArcDps {

    internal static class CombatParser {

        public static CombatEvent ProcessCombat(byte[] data) {
            Ev     ev        = null;
            Ag     src       = null;
            Ag     dst       = null;
            string skillName = null;
            int    offset    = 2;

            if ((byte) (data[1] & (byte) CombatMessageFlags.Ev) == (byte) CombatMessageFlags.Ev) (ev, offset) = ParseEv(data, offset);

            if ((byte) (data[1] & (byte) CombatMessageFlags.Src) == (byte) CombatMessageFlags.Src) (src, offset) = ParseAg(data, offset);

            if ((byte) (data[1] & (byte) CombatMessageFlags.Dst) == (byte) CombatMessageFlags.Dst) (dst, offset) = ParseAg(data, offset);

            if ((byte) (data[1] & (byte) CombatMessageFlags.SkillName) == (byte) CombatMessageFlags.SkillName) (skillName, offset) = ParseString(data, offset);

            ulong id       = BitConverter.ToUInt64(data, offset);
            ulong revision = BitConverter.ToUInt64(data, offset + 8);

            return new CombatEvent(
                                   ev, src, dst, skillName, id,
                                   revision
                                  );
        }

        private static (Ev, int) ParseEv(byte[] data, int offset) {
            ulong  time;
            ulong  srcAgent;
            ulong  dstAgent;
            int    value;
            int    buffDmg;
            uint   overStackValue;
            uint   skillId;
            ushort srcInstId;
            ushort dstInstId;
            ushort srcMasterInstId;
            ushort dstMasterInstId;
            byte   iff;
            bool   buff;
            byte   result;
            bool   isActivation;
            bool   isBuffRemove;
            bool   isNinety;
            bool   isFifty;
            bool   isMoving;
            byte   isStateChange;
            bool   isFlanking;
            bool   isShields;
            bool   isOffCycle;
            byte   pad61;
            byte   pad62;
            byte   pad63;
            byte   pad64;
            (time, offset)            = U64(data, offset);
            (srcAgent, offset)        = U64(data, offset);
            (dstAgent, offset)        = U64(data, offset);
            (value, offset)           = I32(data, offset);
            (buffDmg, offset)         = I32(data, offset);
            (overStackValue, offset)  = U32(data, offset);
            (skillId, offset)         = U32(data, offset);
            (srcInstId, offset)       = U16(data, offset);
            (dstInstId, offset)       = U16(data, offset);
            (srcMasterInstId, offset) = U16(data, offset);
            (dstMasterInstId, offset) = U16(data, offset);
            (iff, offset)             = U8(data, offset);
            (buff, offset)            = B(data, offset);
            (result, offset)          = U8(data, offset);
            (isActivation, offset)    = B(data, offset);
            (isBuffRemove, offset)    = B(data, offset);
            (isNinety, offset)        = B(data, offset);
            (isFifty, offset)         = B(data, offset);
            (isMoving, offset)        = B(data, offset);
            (isStateChange, offset)   = U8(data, offset);
            (isFlanking, offset)      = B(data, offset);
            (isShields, offset)       = B(data, offset);
            (isOffCycle, offset)      = B(data, offset);
            (pad61, offset)           = U8(data, offset);
            (pad62, offset)           = U8(data, offset);
            (pad63, offset)           = U8(data, offset);
            (pad64, offset)           = U8(data, offset);

            var ev = new Ev(
                            time, srcAgent, dstAgent, value, buffDmg,
                            overStackValue, skillId, srcInstId, dstInstId,
                            srcMasterInstId, dstMasterInstId, iff, buff, result,
                            isActivation, isBuffRemove, isNinety, isFifty,
                            isMoving, (StateChange)isStateChange,
                            isFlanking, isShields, isOffCycle, pad61, pad62,
                            pad63, pad64
                           );

            return (ev, offset);
        }

        private static (Ag, int) ParseAg(byte[] data, int offset) {
            string name;
            ulong  id;
            uint   profession;
            uint   elite;
            uint   self;
            ushort team;
            (name, offset)       = ParseString(data, offset);
            (id, offset)         = U64(data, offset);
            (profession, offset) = U32(data, offset);
            (elite, offset)      = U32(data, offset);
            (self, offset)       = U32(data, offset);
            (team, offset)       = U16(data, offset);

            var ag = new Ag(
                            name, id, profession, elite, self,
                            team
                           );

            return (ag, offset);
        }

        private static (string, int) ParseString(byte[] data, int offset) {
            ulong length;
            (length, offset) = U64(data, offset);
            string str = Encoding.UTF8.GetString(data, offset, (int) length);
            return (str, offset + (int) length);
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

            Ev        = 0x01,
            Src       = 0x02,
            Dst       = 0x04,
            SkillName = 0x08

        }

    }

    #region _Enums

    /// <summary>
    /// combat state change
    /// </summary>
    public enum StateChange {
        /// <summary>
        /// not used - not this kind of event
        /// </summary>
	    None,
        /// <summary>
        /// src_agent entered combat, dst_agent is subgroup
        /// </summary>
	    EnterCombat,
        /// <summary>
        /// src_agent left combat
        /// </summary>
	    ExitCombat,
        /// <summary>
        /// src_agent is now alive
        /// </summary>
	    ChangeUp,
        /// <summary>
        /// src_agent is now dead
        /// </summary>
	    ChangeDead,
        /// <summary>
        /// src_agent is now downed
        /// </summary>
	    ChangeDown,
        /// <summary>
        /// src_agent is now in game tracking range (not in realtime api)
        /// </summary>
	    Spawn,
        /// <summary>
        /// src_agent is no longer being tracked (not in realtime api)
        /// </summary>
	    Despawn,
        /// <summary>
        /// src_agent has reached a health marker. dst_agent = percent * 10000 (eg. 99.5% will be 9950) (not in realtime api)
        /// </summary>
	    HealthUpdate,
        /// <summary>
        /// log start. value = server unix timestamp **uint32**. buff_dmg = local unix timestamp. src_agent = 0x637261 (arcdps id) if evtc, npc id if realtime
        /// </summary>
	    LogStart,
        /// <summary>
        /// log end. value = server unix timestamp **uint32**. buff_dmg = local unix timestamp. src_agent = 0x637261 (arcdps id)
        /// </summary>
	    LogEnd,
        /// <summary>
        /// src_agent swapped weapon set. dst_agent = current set id (0/1 water, 4/5 land)
        /// </summary>
	    WeaponSwap,
        /// <summary>
        /// src_agent has had it's maximum health changed. dst_agent = new max health (not in realtime api)
        /// </summary>
	    MaxHealthUpdate,
        /// <summary>
        /// src_agent is agent of "recording" player
        /// </summary>
	    PointOfView,
        /// <summary>
        /// src_agent is text language
        /// </summary>
	    TextLanguage,
        /// <summary>
        /// src_agent is game build
        /// </summary>
	    GameBuild,
        /// <summary>
        /// src_agent is sever shard id
        /// </summary>
	    ShardId,
        /// <summary>
        /// src_agent is self, dst_agent is reward id, value is reward type. these are the wiggly boxes that you get
        /// </summary>
	    Reward,
        /// <summary>
        /// combat event that will appear once per buff per agent on logging start (statechange==18, buff==18, normal cbtevent otherwise)
        /// </summary>
	    BuffInitial,
        /// <summary>
        /// src_agent changed, cast float* p = (float*)&dst_agent, access as x/y/z (float[3]) (not in realtime api)
        /// </summary>
	    Position,
        /// <summary>
        /// src_agent changed, cast float* v = (float*)&dst_agent, access as x/y/z (float[3]) (not in realtime api)
        /// </summary>
	    Velocity,
        /// <summary>
        /// src_agent changed, cast float* f = (float*)&dst_agent, access as x/y (float[2]) (not in realtime api)
        /// </summary>
	    Facing,
        /// <summary>
        /// src_agent change, dst_agent new team id
        /// </summary>
	    TeamChange,
        /// <summary>
        /// src_agent is an attacktarget, dst_agent is the parent agent (gadget type), value is the current targetable state (not in realtime api)
        /// </summary>
	    AttackTarget,
        /// <summary>
        /// dst_agent is new target-able state (0 = no, 1 = yes. default yes) (not in realtime api)
        /// </summary>
	    Targetable,
        /// <summary>
        /// src_agent is map id
        /// </summary>
	    MapId,
        /// <summary>
        /// internal use, won't see anywhere
        /// </summary>
	    ReplInfo,
        /// <summary>
        /// src_agent is agent with buff, dst_agent is the stackid marked active
        /// </summary>
	    StackActive,
        /// <summary>
        /// src_agent is agent with buff, value is the duration to reset to (also marks inactive), pad61- is the stackid
        /// </summary>
	    StackReset,
        /// <summary>
        /// src_agent is agent, dst_agent through buff_dmg is 16 byte guid (client form, needs minor rearrange for api form),
        /// </summary>
	    Guild,
        /// <summary>
        /// is_flanking = probably invuln, is_shields = probably invert, is_offcycle = category, pad61 = stacking type, pad62 = probably resistance, src_master_instid = max stacks (not in realtime)
        /// </summary>
	    BuffInfo,
        /// <summary>
        /// (float*)&time[8]: type attr1 attr2 param1 param2 param3 trait_src trait_self, is_flanking = !npc, is_shields = !player, is_offcycle = break, overstack = value of type determined by pad61 (none/number/skill) (not in realtime, one per formula)
        /// </summary>
	    BuffFormula,
        /// <summary>
        /// (float*)&time[4]: recharge range0 range1 tooltiptime (not in realtime)
        /// </summary>
	    SkillInfo,
        /// <summary>
        /// // src_agent = action, dst_agent = at millisecond (not in realtime, one per timing)
        /// </summary>
	    SkillTiming,
        /// <summary>
        /// src_agent is agent, value is u16 game enum (active, recover, immune, none) (not in realtime api)
        /// </summary>
	    BreakbarState,
        /// <summary>
        /// src_agent is agent, value is float with percent (not in realtime api)
        /// </summary>
	    BreakbarPercent,
        /// <summary>
        /// (char*)&time[32]: error string (not in realtime api)
        /// </summary>
	    Error,
        /// <summary>
        /// src_agent is agent, value is the id (volatile, game build dependent) of the tag
        /// </summary>
	    Tag,
        /// <summary>
        /// unknown or invalid, ignore
        /// </summary>
	    Unknown
    };

    #endregion
}