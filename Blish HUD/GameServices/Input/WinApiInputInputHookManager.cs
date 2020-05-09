using System;
using System.Collections.Generic;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal abstract class WinApiInputHookManager<THandlerDelegate> {

        private static readonly Logger Logger = Logger.GetLogger<WinApiMouseHookManager>();

        private readonly HookExtern.HookCallbackDelegate hookProc; // Store the callback delegate, otherwise it might get garbage collected
        private IntPtr hook;


        public WinApiInputHookManager() {
            hookProc = HookCallback;
        }


        protected abstract HookType HookType { get; }

        protected IList<THandlerDelegate> Handlers { get; } = new List<THandlerDelegate>();


        public virtual bool EnableHook() {
            if (hook != IntPtr.Zero)
                return true;

            Logger.Debug("Enabling");

            hook = HookExtern.SetWindowsHookEx(HookType, hookProc, IntPtr.Zero, 0);
            return hook != IntPtr.Zero;
        }

        public virtual void DisableHook() {
            if (hook == IntPtr.Zero)
                return;

            Logger.Debug("Disabling");

            HookExtern.UnhookWindowsHookEx(hook);
            hook = IntPtr.Zero;
        }

        public virtual void RegisterHandler(THandlerDelegate handleInputCallback) {
            Handlers.Add(handleInputCallback);
        }

        public virtual void UnregisterHandler(THandlerDelegate handleInputCallback) {
            Handlers.Remove(handleInputCallback);
        }

        protected abstract int HookCallback(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
