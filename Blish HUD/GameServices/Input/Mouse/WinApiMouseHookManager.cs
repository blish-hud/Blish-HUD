using System;
using System.Runtime.InteropServices;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal class WinApiMouseHookManager : WinApiInputHookManager<HandleMouseInputDelegate>, IMouseHookManager {

        protected override HookType HookType { get; } = HookType.WH_MOUSE_LL;

        protected override int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0)
                return HookExtern.CallNextHookEx(HookType, nCode, wParam, lParam);

            var mouseEventArgs = new MouseEventArgs((MouseEventType)wParam, Marshal.PtrToStructure<MouseLLHookStruct>(lParam));
            var isHandled = false;
            foreach (var handler in Handlers) {
                isHandled = handler(mouseEventArgs);
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
