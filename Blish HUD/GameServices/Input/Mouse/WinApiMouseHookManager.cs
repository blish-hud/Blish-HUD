using System;
using System.Collections;
using System.Runtime.InteropServices;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal class WinApiMouseHookManager : WinApiInputHookManager<HandleMouseInputDelegate>, IMouseHookManager {

        protected override HookType HookType { get; } = HookType.WH_MOUSE_LL;

        protected override int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0) return HookExtern.CallNextHookEx(this.HookType, nCode, wParam, lParam);

            MouseEventArgs mouseEventArgs = new MouseEventArgs((MouseEventType)wParam, Marshal.PtrToStructure<MouseLLHookStruct>(lParam));
            bool           isHandled      = false;

            lock (((IList) this.Handlers).SyncRoot) {
                foreach (HandleMouseInputDelegate handler in this.Handlers) {
                    isHandled = handler(mouseEventArgs);
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
