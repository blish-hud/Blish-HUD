using System;
using System.Runtime.InteropServices;
using Blish_HUD.Input.WinApi;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    internal class WinApiKeyboardHookManager : WinApiInputHookManager<HandleKeyboardInputDelegate>, IKeyboardHookManager {

        protected override HookType HookType { get; } = HookType.WH_KEYBOARD_LL;

        protected override int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0)
                return HookExtern.CallNextHookEx(HookType, nCode, wParam, lParam);

            var eventType = (KeyboardEventType)((uint)wParam % 2 + 256); // filter out SysKeyDown & SysKeyUp
            var key = (Keys)Marshal.ReadInt32(lParam);

            var KeyboardEventArgs = new KeyboardEventArgs(eventType, key);
            var isHandled = false;
            foreach (var handler in Handlers) {
                isHandled = handler(KeyboardEventArgs);
                if (isHandled)
                    break;
            }

            if (isHandled)
                return 1;
            else
                return HookExtern.CallNextHookEx(HookType, nCode, wParam, lParam);
        }
    }
}
