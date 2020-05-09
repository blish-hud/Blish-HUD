using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.DebugHelper.Native {

    internal static class User32 {

        public delegate int HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType idHook, HOOKPROC lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(HookType idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hook);
    }
}
