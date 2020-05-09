using ProtoBuf;

namespace Blish_HUD.DebugHelperLib.Models {

    [ProtoContract]
    public class KeyboardResponseMessage : Message {

        [ProtoMember(101)]
        public bool IsHandled { get; set; }
    }
}
