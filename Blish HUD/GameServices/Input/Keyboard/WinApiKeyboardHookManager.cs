using System;
using System.Collections;
using System.Runtime.InteropServices;
using Blish_HUD.Input.WinApi;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    internal class WinApiKeyboardHookManager : WinApiInputHookManager<HandleKeyboardInputDelegate>, IKeyboardHookManager {

        private const int LLKHF_EXTENDED = 0x01;

        // KBDLLHOOKSTRUCT: vkCode (4 bytes) + scanCode (4 bytes) + flags (4 bytes)
        private const int FLAGS_OFFSET = 8;

        protected override HookType HookType { get; } = HookType.WH_KEYBOARD_LL;

        protected override int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0) return HookExtern.CallNextHookEx(this.HookType, nCode, wParam, lParam);

            KeyboardEventType eventType = (KeyboardEventType)(((uint)wParam % 2) + 256); // filter out SysKeyDown & SysKeyUp
            Keys              key       = (Keys)Marshal.ReadInt32(lParam);

            // Windows reports the numpad Enter key with the same virtual-key code as the
            // main Enter key. The numpad Enter key is marked as an extended key while the
            // main one is not, which lets us tell the two apart.
            if (key == Keys.Enter && (Marshal.ReadInt32(lParam, FLAGS_OFFSET) & LLKHF_EXTENDED) != 0) {
                key = KeysUtil.NumPadEnter;
            }

            KeyboardEventArgs KeyboardEventArgs = new KeyboardEventArgs(eventType, key);
            bool              isHandled         = false;

            lock (((IList) this.Handlers).SyncRoot) {
                foreach (HandleKeyboardInputDelegate handler in this.Handlers) {
                    isHandled = handler(KeyboardEventArgs);
                    if (isHandled) break;
                }
            }

            if (isHandled)
                return 1;
            else
                return HookExtern.CallNextHookEx(this.HookType, nCode, wParam, lParam);
        }

    }

}
