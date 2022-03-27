using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using InputType = Blish_HUD.Input.WinApi.InputType;
using InputUnion = Blish_HUD.Input.WinApi.InputUnion;
namespace Blish_HUD {
    public static class KeyboardUtil {
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

        [Flags]
        internal enum KeyEventF : uint {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeybdInput {
            internal short wVk;
            internal short wScan;
            internal KeyEventF dwFlags;
            internal int time;
            internal UIntPtr dwExtraInfo;
        }

        private static List<int> ExtendedKeys = new List<int> {
            0x2D, 0x24, 0x22,
            0x2E, 0x23, 0x21,
            0xA5, 0xA1, 0xA3,
            0x26, 0x28, 0x25, 
            0x27, 0x90, 0x2A
        };

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input.WinApi.Input[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, int lParam); // sends a message asynchronously.
        /// <summary>
        /// Presses a key.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to press.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Press(int keyCode, bool sendToSystem = false) {
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem) {
                Input.WinApi.Input[] nInputs;
                if (ExtendedKeys.Contains(keyCode)) {
                    nInputs = new[]
                    {
                        new Input.WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = 224,
                                    wVk = 0,
                                    dwFlags = 0
                                }
                            }
                        },
                        new Input.WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk     = (short)keyCode,
                                    dwFlags = KeyEventF.EXTENDEDKEY
                                }
                            }
                        }
                    };
                } else {
                    nInputs = new[]
                    {
                        new Input.WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk   = (short)keyCode
                                }
                            }
                        }
                    };
                }
                SendInput((uint)nInputs.Length, nInputs, Input.WinApi.Input.Size);
            } else {
                uint vkCode = (uint)keyCode;
                ExtraKeyInfo lParam = new ExtraKeyInfo {
                    scanCode = (char)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC)
                };

                if (ExtendedKeys.Contains(keyCode))
                    lParam.extendedKey = 1;
                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_KEYDOWN, vkCode, lParam.GetInt());
            }
        }

        /// <summary>
        /// Releases a key.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to release.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Release(int keyCode, bool sendToSystem = false) {
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem) {
                Input.WinApi.Input[] nInputs;
                if (ExtendedKeys.Contains(keyCode)) {
                    nInputs = new[]
                    {
                        new Input.WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = 224,
                                    wVk = 0,
                                    dwFlags = 0
                                }
                            }
                        },
                        new Input.WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk     = (short)keyCode,
                                    dwFlags = KeyEventF.EXTENDEDKEY | KeyEventF.KEYUP
                                }
                            }
                        }
                    };
                } else {
                    nInputs = new[]
                    {
                        new Input.WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk     = (short)keyCode,
                                    dwFlags = KeyEventF.KEYUP
                                }
                            }
                        }
                    };
                }
                SendInput((uint)nInputs.Length, nInputs, Input.WinApi.Input.Size);
            } else {
                uint vkCode = (uint)keyCode;
                ExtraKeyInfo lParam = new ExtraKeyInfo {
                    scanCode = (char)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC),
                    repeatCount = 1,
                    prevKeyState = 1,
                    transitionState = 1
                };

                if (ExtendedKeys.Contains(keyCode))
                    lParam.extendedKey = 1;
                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_KEYUP, vkCode, lParam.GetInt());
            }
        }

        /// <summary>
        /// Performs a keystroke inwhich a key is pressed and immediately released once.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to stroke.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Stroke(int keyCode, bool sendToSystem = false) {
            Press(keyCode, sendToSystem);
            Release(keyCode, sendToSystem);
        }

        private class ExtraKeyInfo {
            public ushort repeatCount;
            public char scanCode;
            public ushort extendedKey, prevKeyState, transitionState;

            public int GetInt() {
                return repeatCount | (scanCode << 16) | (extendedKey << 24) |
                       (prevKeyState << 30) | (transitionState << 31);
            }
        }
    }
}