using ProtoBuf;

namespace Blish_HUD.DebugHelper.Models {

    [ProtoContract]
    public class MouseResponseMessage : Message {

        [ProtoMember(101)] public bool IsHandled { get; set; }

    }

}
