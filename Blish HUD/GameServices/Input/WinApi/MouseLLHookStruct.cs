using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input.WinApi {

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseLLHookStruct {

        public Point  Point     { get; }
        public int    MouseData { get; }
        public int    Flags     { get; }
        public int    Time      { get; }
        public IntPtr Extra     { get; }

    }

}
