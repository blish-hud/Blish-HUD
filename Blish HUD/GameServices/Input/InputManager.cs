using System;
using System.Threading;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {
    public abstract class InputManager {

        private readonly HookType _hookType;

        private Hook   _hook;
        private Thread _hookThread;

        protected internal InputManager(HookType hookType) {
            _hookType = hookType;
        }

        private void DoHook() {
            _hook = new Hook(_hookType, HandleNewInput);
            _hook.EnableHook();

            System.Windows.Forms.Application.Run();

            _hook.DisableHook();
        }

        internal void Enable() {
            _hook?.DisableHook();

            _hookThread              = new Thread(DoHook);
            _hookThread.IsBackground = true;
            _hookThread.Start();
        }

        internal void Disable() {
            _hook.DisableHook();
        }

        internal abstract void Update();

        protected abstract bool HandleNewInput(IntPtr wParam, IntPtr lParam);

    }
}
