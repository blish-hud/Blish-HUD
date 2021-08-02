using System.Text;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input {
    internal static class TypedInputUtil {

        // Note: Sometimes single VKCode represents multiple chars, thus string. 
        // E.g. typing "^1" (notice that when pressing 1 the both characters appear, 
        // because of this behavior, "^" is called dead key)

        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint vkCode, uint scanCode, byte[] keyboardState, [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] System.Text.StringBuilder receivingBuffer, int bufferSize, uint flags);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint vkCode, uint mapType);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] keyboardState);

        [DllImport("user32.dll")]
        private static extern byte GetKeyState(int keyState);

        private static uint _lastVirtKeyCode = 0;
        private static uint _lastScanCode = 0;
        private static byte[] _lastKeyState = new byte[256];
        private static bool _lastIsDead = false;
        private static byte VK_SHIFT = 0x10;
        private static byte VK_MENU = 0x12;

        /// <summary>
        /// Convert virtual-key code and keyboard state to unicode string
        /// </summary>
        /// <param name="vkCode"></param>
        /// <param name="isKeyDown"></param>
        /// <returns></returns>
        public static string VirtKeyCodeToString(uint vkCode, bool isKeyDown) {
            // ToUnicodeEx needs StringBuilder, it populates that during execution.
            var output = new StringBuilder(5);

            byte[] keyState = new byte[256];
            uint scanCode = MapVirtualKey(vkCode, 0);

            // aparrently GetKeyboardState() has a problem returning the correct states
            // if called just on its own - calling GetKeyState() before seems to fix
            // this as stated in the following post:
            // https://stackoverflow.com/a/53713024
            GetKeyState(VK_SHIFT);
            GetKeyState(VK_MENU);

            if (!GetKeyboardState(keyState)) {
                return "";
            }

            int result = ToUnicode(vkCode, scanCode, keyState, output, (int)5, (uint)0);

            switch (result) {
                case -1:
                    // dead-key character (accent or diacritic)

                    // clear buffer because it will otherwise crash `public Rectangle AbsoluteBounds` in Control.cs
                    // this will probably also cause case:2 to never trigger
                    while (ToUnicode(vkCode, scanCode, keyState, output, (int)5, (uint)0) < 0) { }

                    // reinject last key becase apparently when calling functions related to keyboard inputs
                    // messes up their internal states everywhere. :rolleyes:
                    // for reference see https://gist.github.com/Ciantic/471698
                    if (_lastVirtKeyCode != 0 && _lastIsDead) {
                        var temp = new StringBuilder(5);
                        ToUnicode(_lastVirtKeyCode, _lastScanCode, _lastKeyState, temp, (int)5, (uint)0);
                    }

                    _lastIsDead = true;
                    _lastVirtKeyCode = vkCode;
                    _lastScanCode = scanCode;
                    _lastKeyState = (byte[])keyState.Clone();

                    if (isKeyDown) return "";
                    break;
                case 0:
                    // no translation for the current state of the keyboard
                case 2:
                    // two or more characters were written to the buffer
                    // this is most likely a dead-key that could not be combined with the current one
                case 1:
                default:
                    // single character was written to the buffer
                    break;

            }

            _lastIsDead = false;

            return isKeyDown ? output.ToString() : "";
        }
    }
}
