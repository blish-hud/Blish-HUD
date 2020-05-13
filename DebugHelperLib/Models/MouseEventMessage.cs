using ProtoBuf;

namespace Blish_HUD.DebugHelperLib.Models {

    [ProtoContract]
    public class MouseEventMessage : Message {

        [ProtoMember(101)] public int EventType { get; set; }

        [ProtoMember(102)] public int PointX { get; set; }

        [ProtoMember(103)] public int PointY { get; set; }

        [ProtoMember(104)] public int MouseData { get; set; }

        [ProtoMember(105)] public int Flags { get; set; }

        [ProtoMember(106)] public int Time { get; set; }

        [ProtoMember(107)] public int ExtraInfo { get; set; }

    }

}
