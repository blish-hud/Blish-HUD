using Blish_HUD.GameServices.ArcDps.Models.UnofficialExtras;
using Blish_HUD.GameServices.ArcDps.V2;
using Blish_HUD.GameServices.ArcDps.V2.Models;

namespace Blish_HUD.GameServices.ArcDps.V2.Extensions {
    public static class BincodeBinaryReaderExtensions {
        public static CombatCallback ParseCombatCallback(this BincodeBinaryReader reader) {
            var result = default(CombatCallback);
            result.Event = reader.ParseCombatEvent();
            result.Source = reader.ParseAgent();
            result.Destination = reader.ParseAgent();
            result.SkillName = reader.Convert.ParseString();
            result.Id = reader.Convert.ParseULong();
            result.Revision = reader.Convert.ParseULong();

            return result;
        }

        public static CombatEvent ParseCombatEvent(this BincodeBinaryReader reader) {
            var result = default(CombatEvent);
            result.Time = reader.Convert.ParseULong();
            result.SourceAgent = reader.Convert.ParseUSize();
            result.DestinationAgent = reader.Convert.ParseUSize();
            result.Value = reader.Convert.ParseInt();
            result.BuffDamage = reader.Convert.ParseInt();
            result.OverstackValue = reader.Convert.ParseUInt();
            result.SkillId = reader.Convert.ParseUInt();
            result.SourceInstanceId = reader.Convert.ParseUShort();
            result.DestinationInstanceId = reader.Convert.ParseUShort();
            result.SourceMasterInstanceId = reader.Convert.ParseUShort();
            result.DestinationMasterInstanceId = reader.Convert.ParseUShort();
            result.Iff = ParseEnum(reader.Convert.ParseByte(), (int)Affinity.Unknown, Affinity.Unknown);
            result.Buff = reader.Convert.ParseBool();
            result.Result = reader.Convert.ParseByte();
            result.IsActivation = ParseEnum(reader.Convert.ParseByte(), (int)Activation.Unknown, Activation.Unknown);
            result.IsBuffRemoved = ParseEnum(reader.Convert.ParseByte(), (int)BuffRemove.Unknown, BuffRemove.Unknown);
            result.IsNinety = reader.Convert.ParseBool();
            result.IsFifty = reader.Convert.ParseBool();
            result.IsMoving = reader.Convert.ParseBool();
            result.IsStateChanged = ParseEnum(reader.Convert.ParseByte(), (int)StateChange.Unknown, StateChange.Unknown);
            result.IsFlanking = reader.Convert.ParseBool();
            result.IsShiels = reader.Convert.ParseBool();
            result.IsOffCycle = reader.Convert.ParseBool();
            result.Pad61 = reader.Convert.ParseByte();
            result.Pad62 = reader.Convert.ParseByte();
            result.Pad63 = reader.Convert.ParseByte();
            result.Pad64 = reader.Convert.ParseByte();
            return result;
        }

        public static Agent ParseAgent(this BincodeBinaryReader reader) {
            var result = default(Agent);
            result.Name = reader.Convert.ParseString();
            result.Id = reader.Convert.ParseUSize();
            result.Profession = reader.Convert.ParseUInt();
            result.Elite = reader.Convert.ParseUInt();
            result.Self = reader.Convert.ParseUInt();
            result.Team = reader.Convert.ParseUShort();
            return result;
        }

        public static UserInfo ParseUserInfo(this BincodeBinaryReader reader) {
            var result = default(UserInfo);
            result.AccountName = reader.Convert.ParseString();
            result.JoinTime = reader.Convert.ParseULong();
            result.Role = ParseEnum((byte)reader.Convert.ParseUInt(), (int)UserRole.None, UserRole.None);
            result.Subgroup = reader.Convert.ParseByte();
            result.ReadyStatus = reader.Convert.ParseBool();
            result._unused1 = reader.Convert.ParseByte();
            result._unused2 = reader.Convert.ParseUInt();
            return result;
        }

        public static ChatMessageInfo ParseChatMessageInfo(BincodeBinaryReader reader) {
            var result = default(ChatMessageInfo);
            result.ChannelId = reader.Convert.ParseUInt();
            result.ChannelType = ParseEnum((byte)reader.Convert.ParseUInt(), (int)ChannelType.Invalid, ChannelType.Invalid);
            result.Subgroup = reader.Convert.ParseByte();
            result.IsBroadcast = reader.Convert.ParseBool();
            result._unused1 = reader.Convert.ParseByte();
            result.TimeStamp = reader.Convert.ParseString();
            result.AccountName = reader.Convert.ParseString();
            result.CharacterName = reader.Convert.ParseString();
            result.Text = reader.Convert.ParseString();
            return result;
        }

        // This is used to not make an expensive reflection typeof/GetType and wasting precious time
        private static T ParseEnum<T>(byte enumByteValue, int maxValue, T unknown)
            where T : System.Enum {
            if (enumByteValue > maxValue) {
                return unknown;
            }

            return (T)(object)enumByteValue;
        }
    }

}
