using System;
using System.Runtime.InteropServices;
using Blish_HUD.DebugHelper.Native;
using Blish_HUD.DebugHelperLib.Models;
using Blish_HUD.DebugHelperLib.Services;

namespace Blish_HUD.DebugHelper.Services {

    internal class MouseHookService : IDebugService, IDisposable {

        private const int CALLBACK_TIMEOUT = 10;

        private readonly IMessageService messageService;
        private readonly User32.HOOKPROC hookProc; // Store the callback delegate, otherwise it might get garbage collected
        private          IntPtr          hook;

        public MouseHookService(IMessageService messageService) {
            this.messageService = messageService;
            hookProc            = HookCallback;
        }

        public void Start() {
            if (hook == IntPtr.Zero) hook = User32.SetWindowsHookEx(HookType.WH_MOUSE_LL, hookProc, Marshal.GetHINSTANCE(typeof(MouseHookService).Module), 0);
        }

        public void Stop() {
            if (hook != IntPtr.Zero) User32.UnhookWindowsHookEx(hook);
            hook = IntPtr.Zero;
        }

        private int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0) return User32.CallNextHookEx(HookType.WH_MOUSE_LL, nCode, wParam, lParam);

            int               eventType  = (int)wParam;
            MOUSELLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MOUSELLHOOKSTRUCT>(lParam);

            var message = new MouseEventMessage {
                EventType = eventType,
                PointX    = hookStruct.pt.x,
                PointY    = hookStruct.pt.y,
                MouseData = hookStruct.mouseData,
                Flags     = hookStruct.flags,
                Time      = hookStruct.time,
                ExtraInfo = hookStruct.extraInfo
            };

            MouseResponseMessage? response = messageService.SendAndWait<MouseResponseMessage>(message, TimeSpan.FromMilliseconds(CALLBACK_TIMEOUT));

            if (response?.IsHandled == true)
                return 1;
            else
                return User32.CallNextHookEx(HookType.WH_MOUSE_LL, nCode, wParam, lParam);
        }

        #region IDisposable Support

        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool isDisposing) {
            if (!isDisposed) {
                if (isDisposing) Stop();
                isDisposed = true;
            }
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
