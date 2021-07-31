using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal abstract class WinApiBaseHookManager<THandlerDelegate> : IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<WinApiMouseHookManager>();

        private readonly User32.HOOKPROC hookProc; // Store the callback delegate, otherwise it might get garbage collected
        private          IntPtr          hook;

        public WinApiBaseHookManager() { hookProc = HookCallback; }


        protected abstract HookType HookType { get; }

        protected IList<THandlerDelegate> Handlers { get; } = new SynchronizedCollection<THandlerDelegate>();


        public virtual bool EnableHook() {
            if (hook != IntPtr.Zero) return true;

            Logger.Debug("Enabling");

            hook = User32.SetWindowsHookEx(this.HookType, hookProc, Marshal.GetHINSTANCE(typeof(WinApiBaseHookManager<>).Module), 0);
            return hook != IntPtr.Zero;
        }

        public virtual void DisableHook() {
            if (hook == IntPtr.Zero) return;

            Logger.Debug("Disabling");

            User32.UnhookWindowsHookEx(hook);
            hook = IntPtr.Zero;
        }

        public virtual void RegisterHandler(THandlerDelegate handleInputCallback) { this.Handlers.Add(handleInputCallback); }

        public virtual void UnregisterHandler(THandlerDelegate handleInputCallback) { this.Handlers.Remove(handleInputCallback); }

        protected abstract int HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        #region IDisposable Support

        private bool isDisposed;
        protected virtual void Dispose(bool disposing) {
            if (isDisposed) return;

            if (disposing) {
                // No managed resources
            }

            User32.UnhookWindowsHookEx(hook);
            hook = IntPtr.Zero;

            isDisposed = true;
        }

        ~WinApiBaseHookManager() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
