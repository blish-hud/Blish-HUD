using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    internal static class PInvoke
    {
        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SendMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam); // sends a message asynchronously.

        [DllImport("user32.dll")]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        internal static extern short VkKeyScan(char ch);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SendMessageCallbackA(IntPtr hWnd, uint Msg, uint wParam, int lParam, SendAsyncProc lpResultCallBack, uint dwData);
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
    }
}