using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blish_HUD.WinApi {
    public class KeyboardHook {

        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN     = 0x0100;
        public const int WM_KEYUP       = 0x0101;
        public LowLevelKeyboardProc _proc;
        public static IntPtr               _hookID        = IntPtr.Zero;

        public KeyboardHook() { _proc = HookCallback; }

        public IntPtr HookKeyboard() {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL,                        _proc,
                                        GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void UnHookKeyboard() {
            UnhookWindowsHookEx(_hookID);
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            // Priority is to get the event into the queue so that Windows doesn't give up waiting on us
            GameService.Input.KeyboardMessages.Enqueue(new KeyboardMessage(nCode, wParam, Marshal.ReadInt32(lParam)));

            // If we are sending input to a control, try to prevent GW2 from getting any keypresses
            if (nCode >= 0 && GameService.Input.BlockInput)
                return (IntPtr) 1;

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
