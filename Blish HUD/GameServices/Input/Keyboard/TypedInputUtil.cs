/*
 * This code has been adapted from Ciantic's `keyboardlistener.cs` (https://gist.github.com/Ciantic/471698#file-keyboardlistener-cs-L224-L427)
 * We include adaptions for dead-key handling from Urutar (https://gist.github.com/Ciantic/471698#gistcomment-1448512)
 */

using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input {
    internal static class TypedInputUtil {

        // Note: Sometimes single VKCode represents multiple chars, thus string. 
        // E.g. typing "^1" (notice that when pressing 1 the both characters appear, 
        // because of this behavior, "^" is called dead key)

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetKeyboardLayout(uint dwLayout);

        [DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static uint   _lastVkCode   = 0;
        private static uint   _lastScanCode = 0;
        private static byte[] _lastKeyState = new byte[255];
        private static bool   _lastIsDead   = false;
        private static bool   _isShiftDown = false;

        /// <summary>
        /// Convert virtual-key code and keyboard state to unicode string
        /// </summary>
        /// <param name="virtKeyCode"></param>
        /// <param name="isKeyDown"></param>
        /// <returns></returns>
        public static string VirtKeyCodeToString(uint virtKeyCode, bool isKeyDown) {
            // ToUnicodeEx needs StringBuilder, it populates that during execution.
            System.Text.StringBuilder output = new System.Text.StringBuilder(5);

            // Get the current keyboard layout
            IntPtr HKL = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out _));

            byte[] keyState = new byte[256];
            uint scanCode = MapVirtualKeyEx(virtKeyCode, 0, HKL);

            switch (scanCode) {
                case 42:
                case 54:
                    // left or right shift is pressed
                    _isShiftDown = isKeyDown;
                    return "";
                default:
                    break;
            }

            if(_isShiftDown || Console.CapsLock) keyState[0x10] = 0x80;

            int result = ToUnicodeEx(virtKeyCode, scanCode, keyState, output, (int)5, (uint)0, HKL);

            switch (result) {
                case -1:
                    // dead-key character (accent or diacritic)
                    _lastIsDead = true;

                    // clear buffer because it will otherwise crash `public Rectangle AbsoluteBounds` in Control.cs
                    // this will probably also cause case:2 to never trigger
                    while (ToUnicodeEx(virtKeyCode, scanCode, keyState, output, (int)5, (uint)0, HKL) < 0);

                    // TODO: handle dead keys

                    return "";
                case 0:
                    // no translation for the current state of the keyboard
                    break;
                case 2:
                    // two or more characters were written to the buffer
                    // this is most likely a dead-key that could not be combined with the current one
                    break;
                case 1:
                default:
                    // single character was written to the buffer
                    break;

            }

            _lastIsDead = false;

            if (isKeyDown) return output.ToString();
            else return "";
        }
    }
}
