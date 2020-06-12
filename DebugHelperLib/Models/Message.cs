using ProtoBuf;

namespace Blish_HUD.DebugHelperLib.Models {

    [ProtoContract]
    [ProtoInclude(1, typeof(PingMessage))]
    [ProtoInclude(2, typeof(MouseEventMessage))]
    [ProtoInclude(3, typeof(MouseResponseMessage))]
    [ProtoInclude(4, typeof(KeyboardEventMessage))]
    [ProtoInclude(5, typeof(KeyboardResponseMessage))]
    public abstract class Message {

        [ProtoMember(11)] public ulong Id { get; set; }

    }

}
