using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input {
    internal static class TypedInputUtil {

        // Note: Sometimes single VKCode represents multiple chars, thus string. 
        // E.g. typing "^1" (notice that when pressing 1 the both characters appear, 
        // because of this behavior, "^" is called dead key)

        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint virtualKeyCode, uint scanCode, byte[] keyboardState, [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] System.Text.StringBuilder receivingBuffer, int bufferSize, uint flags);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint virtualKeyCode, uint mapType);

        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern int GetKeyState(int lpKeyState);

        private static uint   _lastVkCode   = 0;
        private static uint   _lastScanCode = 0;
        private static byte[] _lastKeyState = new byte[255];
        private static bool   _lastIsDead   = false;
        private static bool   _isShiftDown = false;
        private static byte VK_SHIFT = 0x10;
        private static byte VK_MENU = 0x12;

        /// <summary>
        /// Convert virtual-key code and keyboard state to unicode string
        /// </summary>
        /// <param name="virtKeyCode"></param>
        /// <param name="isKeyDown"></param>
        /// <returns></returns>
        public static string VirtKeyCodeToString(uint virtKeyCode, bool isKeyDown) {
            // ToUnicodeEx needs StringBuilder, it populates that during execution.
            System.Text.StringBuilder output = new System.Text.StringBuilder(5);

            byte[] keyState = new byte[256];
            uint scanCode = MapVirtualKey(virtKeyCode, 0);

            // TODO: this would be the best option to determine which modifiers are
            //       pressed, but somehow it returns only the low order bit whereas
            //       ToUnicode expects the high order bit set for modifiers
            //       I've found this, but it seems to have no effect:
            //       https://stackoverflow.com/a/53713024
            //int shift = GetKeyState(VK_SHIFT);
            //int alt = GetKeyState(VK_MENU);
            //GetKeyboardState(keyState);

            switch (scanCode) {
                case 42:
                case 54:
                    // left or right shift is pressed
                    _isShiftDown = isKeyDown;
                    return "";
                default:
                    break;
            }

            // set high-order bit of shift in keyState to represent a pressed shift key
            if(_isShiftDown || Console.CapsLock) keyState[VK_SHIFT] = 0x80;

            int result = ToUnicode(virtKeyCode, scanCode, keyState, output, (int)5, (uint)0);

            switch (result) {
                case -1:
                    // dead-key character (accent or diacritic)
                    _lastIsDead = true;

                    // clear buffer because it will otherwise crash `public Rectangle AbsoluteBounds` in Control.cs
                    // this will probably also cause case:2 to never trigger
                    while (ToUnicode(virtKeyCode, scanCode, keyState, output, (int)5, (uint)0) < 0) { }

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
