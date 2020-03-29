using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    internal static class TypedInputUtil {

        // Note: Sometimes single VKCode represents multiple chars, thus string. 
        // E.g. typing "^1" (notice that when pressing 1 the both characters appear, 
        // because of this behavior, "^" is called dead key)

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetKeyboardLayout(uint dwLayout);

        [DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private static uint   lastVKCode   = 0;
        private static uint   lastScanCode = 0;
        private static byte[] lastKeyState = new byte[255];
        private static bool   lastIsDead   = false;

        /// <summary>
        /// Convert VKCode to Unicode.
        /// <remarks>isKeyDown is required for because of keyboard state inconsistencies!</remarks>
        /// </summary>
        /// <param name="VKCode">VKCode</param>
        /// <param name="isKeyDown">Is the key down event?</param>
        /// <returns>String representing single unicode character.</returns>
        public static string VKCodeToString(uint VKCode, bool isKeyDown) {
            // ToUnicodeEx needs StringBuilder, it populates that during execution.
            System.Text.StringBuilder sbString = new System.Text.StringBuilder(5);

            byte[] bKeyState = new byte[255];
            bool bKeyStateStatus;
            bool isDead = false;

            // Gets the current windows window handle, threadID, processID
            IntPtr currentHWnd = GetForegroundWindow();
            uint currentProcessID;
            uint currentWindowThreadID = GetWindowThreadProcessId(currentHWnd, out currentProcessID);

            // This programs Thread ID
            uint thisProgramThreadId = GetCurrentThreadId();

            // Attach to active thread so we can get that keyboard state
            if (AttachThreadInput(thisProgramThreadId, currentWindowThreadID, true)) {
                // Current state of the modifiers in keyboard
                bKeyStateStatus = GetKeyboardState(bKeyState);

                // Detach
                AttachThreadInput(thisProgramThreadId, currentWindowThreadID, false);
            } else {
                // Could not attach, perhaps it is this process?
                bKeyStateStatus = GetKeyboardState(bKeyState);
            }

            // On failure we return empty string.
            if (!bKeyStateStatus)
                return "";

            // Gets the layout of keyboard
            IntPtr HKL = GetKeyboardLayout(currentWindowThreadID);

            // Maps the virtual keycode
            uint lScanCode = MapVirtualKeyEx(VKCode, 0, HKL);

            // Keyboard state goes inconsistent if this is not in place. In other words, we need to call above commands in UP events also.
            if (!isKeyDown)
                return "";

            // Converts the VKCode to unicode
            int relevantKeyCountInBuffer = ToUnicodeEx(VKCode, lScanCode, bKeyState, sbString, sbString.Capacity, (uint)0, HKL);

            string ret = "";

            switch (relevantKeyCountInBuffer) {
                // Dead keys (^,`...)
                case -1:
                    isDead = true;

                    // We must clear the buffer because ToUnicodeEx messed it up, see below.
                    ClearKeyboardBuffer(VKCode, lScanCode, HKL);
                    break;

                case 0:
                    break;

                // Single character in buffer
                case 1:
                    ret = sbString[0].ToString();

                    if (GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift)) {
                        ret = ret.ToUpper();
                    }

                    break;

                // Two or more (only two of them is relevant)
                case 2:
                default:
                    ret = sbString.ToString().Substring(0, 2);
                    break;
            }

            // We inject the last dead key back, since ToUnicodeEx removed it.
            // More about this peculiar behavior see e.g: 
            //   http://www.experts-exchange.com/Programming/System/Windows__Programming/Q_23453780.html
            //   http://blogs.msdn.com/michkap/archive/2005/01/19/355870.aspx
            //   http://blogs.msdn.com/michkap/archive/2007/10/27/5717859.aspx
            if (lastVKCode != 0 && lastIsDead) {
                System.Text.StringBuilder sbTemp = new System.Text.StringBuilder(5);
                ToUnicodeEx(lastVKCode, lastScanCode, lastKeyState, sbTemp, sbTemp.Capacity, (uint)0, HKL);

                if (!string.IsNullOrEmpty(ret)) {
                    lastVKCode = 0;
                    lastIsDead = false;
                }

                return ret;
            }

            // Save these
            lastScanCode = lScanCode;
            lastVKCode = VKCode;
            lastIsDead = isDead;
            lastKeyState = (byte[])bKeyState.Clone();

            return ret;
        }

        private static void ClearKeyboardBuffer(uint vk, uint sc, IntPtr hkl) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(10);

            int rc;
            do {
                byte[] lpKeyStateNull = new byte[255];
                rc = ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }

    }
}
