using System;
using System.Collections.Generic;
using Blish_HUD.Controls.Extern;
namespace Blish_HUD.Controls.Intern
{
    public static class Keyboard
    {
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

        private static List<VirtualKeyShort> ExtendedKeys = new List<VirtualKeyShort> {
            VirtualKeyShort.INSERT,  VirtualKeyShort.HOME,   VirtualKeyShort.NEXT, 
            VirtualKeyShort.DELETE,  VirtualKeyShort.END,    VirtualKeyShort.PRIOR,
            VirtualKeyShort.RMENU,   VirtualKeyShort.RSHIFT, VirtualKeyShort.RCONTROL,
            VirtualKeyShort.UP,      VirtualKeyShort.DOWN,   VirtualKeyShort.LEFT,     VirtualKeyShort.RIGHT,
            VirtualKeyShort.NUMLOCK, VirtualKeyShort.PRINT
        };

        /// <summary>
        /// Presses a key.
        /// </summary>
        /// <param name="key">Virtual Key Short</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Press(VirtualKeyShort key, bool sendToSystem = false)
        {
            if (!GameService.GameIntegration.Gw2Proc.Gw2IsRunning || sendToSystem)
            {
                Extern.Input[] nInputs;
                if (ExtendedKeys.Contains(key)) {
                    nInputs = new[]
                    {
                        new Extern.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = ScanCodeShort.EXTENDEDKEY,
                                    wVk = 0,
                                    dwFlags = 0
                                }
                            }
                        },
                        new Extern.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = (ScanCodeShort)PInvoke.MapVirtualKey((uint)key, MAPVK_VK_TO_VSC),
                                    wVk = key,
                                    dwFlags = KeyEventF.EXTENDEDKEY
                                }
                            }
                        }
                    };
                } else {
                    nInputs = new[]
                    {
                        new Extern.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = (ScanCodeShort)PInvoke.MapVirtualKey((uint)key, MAPVK_VK_TO_VSC),
                                    wVk = key
                                }
                            }
                        }
                    };
                }
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
            }
            else
            {
                uint vkCode = (uint)key;
                ExtraKeyInfo lParam = new ExtraKeyInfo() {
                    scanCode = (char)PInvoke.MapVirtualKey(vkCode, MAPVK_VK_TO_VSC)
                };
                if (ExtendedKeys.Contains(key))
                    lParam.extendedKey = 1;
                PInvoke.PostMessage(GameService.GameIntegration.Gw2Proc.Gw2WindowHandle, WM_KEYDOWN, vkCode, lParam.GetInt());
            }
        }
        /// <summary>
        /// Releases a key.
        /// </summary>
        /// <param name="key">Virtual Key Short</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Release(VirtualKeyShort key, bool sendToSystem = false)
        {
            if (!GameService.GameIntegration.Gw2Proc.Gw2IsRunning || sendToSystem)
            {
                Extern.Input[] nInputs;
                if (ExtendedKeys.Contains(key)) {
                    nInputs = new[]
                    {
                        new Extern.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = ScanCodeShort.EXTENDEDKEY,
                                    wVk = 0,
                                    dwFlags = 0
                                }
                            }
                        },
                        new Extern.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = (ScanCodeShort)PInvoke.MapVirtualKey((uint)key, MAPVK_VK_TO_VSC),
                                    wVk = key,
                                    dwFlags = KeyEventF.EXTENDEDKEY | KeyEventF.KEYUP
                                }
                            }
                        }
                    };
                } else {
                    nInputs = new[]
                    {
                        new Extern.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = (ScanCodeShort)PInvoke.MapVirtualKey((uint)key, MAPVK_VK_TO_VSC),
                                    wVk = key,
                                    dwFlags = KeyEventF.KEYUP 
                                }
                            }
                        }
                    };
                }
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
            }
            else
            {
                uint vkCode = (uint)key;
                ExtraKeyInfo lParam = new ExtraKeyInfo() {
                    scanCode = (char)PInvoke.MapVirtualKey(vkCode, MAPVK_VK_TO_VSC),
                    repeatCount = 1,
                    prevKeyState = 1,
                    transitionState = 1
                };
                if (ExtendedKeys.Contains(key))
                    lParam.extendedKey = 1;
                PInvoke.PostMessage(GameService.GameIntegration.Gw2Proc.Gw2WindowHandle, WM_KEYUP, vkCode, lParam.GetInt());
            }
        }
        /// <summary>
        /// Performs a keystroke inwhich a key is pressed and immediately released once.
        /// </summary>
        /// <param name="key">Virtual Key Short</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Stroke(VirtualKeyShort key, bool sendToSystem = false)
        {
            Press(key, sendToSystem);
            Release(key, sendToSystem);
        }
    }
    class ExtraKeyInfo
    {
        public ushort repeatCount;
        public char scanCode;
        public ushort extendedKey, prevKeyState, transitionState;

        public int GetInt()
        {
            return repeatCount | (scanCode << 16) | (extendedKey << 24) |
                (prevKeyState << 30) | (transitionState << 31);
        }
    };
}