using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Blish_HUD.WinAPI {
    internal class KeyboardHook {

        private static readonly Logger Logger = Logger.GetLogger(typeof(KeyboardHook));

        private const  int                  WH_KEYBOARD_LL = 13;
        private const  int                  WM_KEYDOWN     = 0x0100;
        private const  int                  WM_KEYUP       = 0x0101;

        private          IntPtr               _keyboardHook = IntPtr.Zero;
        private readonly LowLevelKeyboardProc _proc;

        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        public KeyboardHook() => _proc = HookCallback;

        public bool HookKeyboard() {
            Logger.Debug("Enabling keyboard hook.");

            if (_keyboardHook == IntPtr.Zero) {
                _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, Extern.GetModuleHandleW(IntPtr.Zero), 0);
            }

            return _keyboardHook != IntPtr.Zero;
        }

        public void UnhookKeyboard() {
            Logger.Debug("Disabling the keyboard hook.");

            if (_keyboardHook == IntPtr.Zero) return;

            Extern.UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            // Priority is to get the event into the queue so that Windows doesn't give up waiting on us
            GameService.Input.KeyboardMessages.Enqueue(new KeyboardMessage(nCode, wParam, Marshal.ReadInt32(lParam)));

            // If we are sending input to a control, try to prevent GW2 from getting any keypresses
            if (nCode >= 0 && GameService.Input.BlockInput)
                return (IntPtr) 1;

            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

    }
}
