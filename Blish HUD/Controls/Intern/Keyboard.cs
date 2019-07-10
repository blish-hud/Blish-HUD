using System;
using System.Collections.Generic;
using Blish_HUD.Controls.Extern;
namespace Blish_HUD.Controls.Intern
{
    public class Keyboard : IKeyboard
    {
        /// <summary>
        /// If true, uses normal key input instead of sending key event messages to GW2's window handle.
        /// </summary>
        public bool HardwareInput = false;

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

        private IntPtr Gw2WndHandle = GameService.GameIntegration.Gw2Process.MainWindowHandle;

        private static readonly Dictionary<GuildWarsControls, ScanCodeShort> ScanCodeShorts = new Dictionary<GuildWarsControls, ScanCodeShort>
        {
            {GuildWarsControls.WeaponSkill1, ScanCodeShort.KEY_1},
            {GuildWarsControls.WeaponSkill2, ScanCodeShort.KEY_2},
            {GuildWarsControls.WeaponSkill3, ScanCodeShort.KEY_3},
            {GuildWarsControls.WeaponSkill4, ScanCodeShort.KEY_4},
            {GuildWarsControls.WeaponSkill5, ScanCodeShort.KEY_5},
            {GuildWarsControls.HealingSkill, ScanCodeShort.KEY_6},
            {GuildWarsControls.UtilitySkill1, ScanCodeShort.KEY_7},
            {GuildWarsControls.UtilitySkill2, ScanCodeShort.KEY_8},
            {GuildWarsControls.UtilitySkill3, ScanCodeShort.KEY_9},
            {GuildWarsControls.EliteSkill, ScanCodeShort.KEY_0}
        };

        private static readonly Dictionary<GuildWarsControls, VirtualKeyShort> VirtualKeyShorts = new Dictionary<GuildWarsControls, VirtualKeyShort>
        {
            {GuildWarsControls.WeaponSkill1, VirtualKeyShort.KEY_1},
            {GuildWarsControls.WeaponSkill2, VirtualKeyShort.KEY_2},
            {GuildWarsControls.WeaponSkill3, VirtualKeyShort.KEY_3},
            {GuildWarsControls.WeaponSkill4, VirtualKeyShort.KEY_4},
            {GuildWarsControls.WeaponSkill5, VirtualKeyShort.KEY_5},
            {GuildWarsControls.HealingSkill, VirtualKeyShort.KEY_6},
            {GuildWarsControls.UtilitySkill1, VirtualKeyShort.KEY_7},
            {GuildWarsControls.UtilitySkill2, VirtualKeyShort.KEY_8},
            {GuildWarsControls.UtilitySkill3, VirtualKeyShort.KEY_9},
            {GuildWarsControls.EliteSkill, VirtualKeyShort.KEY_0}
        };

        public Keyboard(){ /** NOOP **/ }

        public void Press(GuildWarsControls key)
        {
            if (HardwareInput || !GameService.GameIntegration.Gw2IsRunning)
            {
                var nInputs = new[]
                {
                    new Input
                    {
                        type = InputType.KEYBOARD,
                        U = new InputUnion
                        {
                            ki = new KeybdInput
                            {
                                wScan = ScanCodeShorts[key],
                                wVk = VirtualKeyShorts[key]
                            }
                        }
                    }
                };
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Input.Size);
            }
            else
            {
                uint vkCode = (uint)VirtualKeyShorts[key];
                ExtraKeyInfo lParam = new ExtraKeyInfo(){
                    scanCode = (char)PInvoke.MapVirtualKey(vkCode, MAPVK_VK_TO_VSC)
                };
                PInvoke.PostMessage(Gw2WndHandle, WM_KEYDOWN, vkCode, lParam.GetInt());
            }
        }

        public void Release(GuildWarsControls key)
        {
            if (HardwareInput || !GameService.GameIntegration.Gw2IsRunning)
            {
                var nInputs = new[]
                {
                    new Input
                    {
                        type = InputType.KEYBOARD,
                        U = new InputUnion
                        {
                            ki = new KeybdInput
                            {
                                wScan = ScanCodeShorts[key],
                                wVk = VirtualKeyShorts[key],
                                dwFlags = KeyEventF.KEYUP
                            }
                        }
                    }
                };
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Input.Size);
            }
            else
            {
                uint vkCode = (uint)VirtualKeyShorts[key];
                ExtraKeyInfo lParam = new ExtraKeyInfo
                {
                    scanCode = (char)PInvoke.MapVirtualKey(vkCode, MAPVK_VK_TO_VSC),
                    repeatCount = 1,
                    prevKeyState = 1,
                    transitionState = 1
                };
                PInvoke.PostMessage(Gw2WndHandle, WM_KEYUP, vkCode, lParam.GetInt());
            }
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