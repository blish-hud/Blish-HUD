using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input.WinApi {
    [StructLayout(LayoutKind.Sequential)]
    internal struct CursorInfo {
        /// <summary>
        /// The caller must set this to Marshal.SizeOf(typeof(CURSORINFO))
        /// </summary>
        public int         CbSize;
        public CursorFlags Flags;
        public IntPtr      HCursor;
        public Point       ScreenPosition;
    }
}
