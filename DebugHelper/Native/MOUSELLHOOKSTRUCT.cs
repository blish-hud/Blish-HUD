using System.Runtime.InteropServices;

namespace Blish_HUD.DebugHelper.Native {

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSELLHOOKSTRUCT {

        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public int extraInfo;
    }
}
