using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public enum CURSORFLAGS : int
    {
        CURSOR_HIDING = 0x0,
        CURSOR_SHOWING = 0x1,
        CURSOR_SUPPRESSED = 0x2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        /// <summary>
        /// The caller must set this to Marshal.SizeOf(typeof(CURSORINFO))
        /// </summary>
        public int cbSize;
        public CURSORFLAGS flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    internal static class PInvoke
    {
        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SendMessage(IntPtr hWnd, uint msg, uint wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, int lParam); // sends a message asynchronously.

        [DllImport("user32.dll")]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        internal static extern short VkKeyScan(char ch);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SendMessageCallbackA(IntPtr hWnd, uint msg, uint wParam, int lParam, SendAsyncProc lpResultCallBack, uint dwData);
        internal delegate void SendAsyncProc(IntPtr hWnd, uint uMsg, uint dwData, int lResult);

        [DllImport("user32.dll")]
        internal static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.Dll", SetLastError = true)]
        internal static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(ref CURSORINFO pci);

        internal static CURSORINFO GetCursorInfo()
        {
            var pci = new CURSORINFO {
                cbSize = Marshal.SizeOf(typeof(CURSORINFO))
            };
            GetCursorInfo(ref pci);
            return pci;
        }
    }
}