using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input.WinApi {

    internal static class HookExtern {

        public delegate int HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType idHook, HookCallbackDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(HookType idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandleW(IntPtr fakezero);

    }
}
