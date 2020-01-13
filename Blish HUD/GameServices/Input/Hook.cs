using System;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal sealed class Hook {

        private static readonly Logger Logger = Logger.GetLogger<Hook>();

        public delegate bool HandleInputDelegate(IntPtr wParam, IntPtr lParam);

        private HookType HookType { get; }

        private IntPtr _hook = IntPtr.Zero;

        private readonly HandleInputDelegate _handleInputProc;

        private readonly HookExtern.HookCallbackDelegate _hookProc;

        public Hook(HookType hookType, HandleInputDelegate handleInputProc) {
            this.HookType    = hookType;
            _handleInputProc = handleInputProc;

            _hookProc = HookCallback;
        }

        public bool EnableHook() {
            Logger.Debug("Enabling {hookType} hook.", this.HookType);

            if (_hook == IntPtr.Zero) {
                _hook = HookExtern.SetWindowsHookEx(this.HookType, _hookProc, HookExtern.GetModuleHandleW(IntPtr.Zero), 0);
            }

            return _hook != IntPtr.Zero;
        }

        public void DisableHook() {
            if (_hook == IntPtr.Zero) return;

            Logger.Debug("Disabling the {hookType} hook.", this.HookType);

            HookExtern.UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }

        private int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0) return HookExtern.CallNextHookEx(this.HookType, nCode, wParam, lParam);

            return _handleInputProc(wParam, lParam)
                ? 1
                : HookExtern.CallNextHookEx(this.HookType, nCode, wParam, lParam);
        }

    }
}
