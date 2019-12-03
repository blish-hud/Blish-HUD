using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Blish_HUD.Input.WinApi {

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseLLHookStruct {

        public Point  Point     { get; }
        public int    MouseData { get; }
        public int    Flags     { get; }
        public int    Time      { get; }
        public IntPtr Extra     { get; }

        public Int32 WheelDelta {
            get {
                int v                                              = Convert.ToInt32((MouseData & 0xFFFF0000) >> 16);
                if (v > SystemInformation.MouseWheelScrollDelta) v -= (ushort.MaxValue + 1);
                return v;
            }
        }

    }

}
