using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blish_HUD.WinApi {

    public class MouseHook {

        public struct MSLLHOOKSTRUCT {

            public Point  pt;
            public Int32  mouseData;
            public Int32  flags;
            public Int32  time;
            public IntPtr extra;

            public Int32 wheelDelta {
                get {
                    int v                                              = Convert.ToInt32((mouseData & 0xFFFF0000) >> 16);
                    if (v > SystemInformation.MouseWheelScrollDelta) v = v - (ushort.MaxValue + 1);
                    return v;
                }
            }

        }

        private       IntPtr _mouseHook;
        private const Int32  WH_MOUSE_LL = 14;

        private delegate Int32 MouseHookDelegate(Int32 nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);

        [MarshalAs(UnmanagedType.FunctionPtr)] private MouseHookDelegate _mouseProc;

        public enum MouseMessages {

            WM_MouseMove            = 512,
            WM_LeftButtonDown       = 513,
            WM_LeftButtonUp         = 514,
            WM_LeftDblClick         = 515,
            WM_RightButtonDown      = 516,
            WM_RightButtonUp        = 517,
            WM_RightDblClick        = 518,
            WM_MiddleButtonDown     = 519,
            WM_MiddleButtonUp       = 520,
            WM_MiddleButtonDblClick = 521,
            WM_MouseWheel           = 522,

        }

        [DllImport("user32.dll")]   private static extern IntPtr SetWindowsHookExW(Int32    idHook, MouseHookDelegate HookProc, IntPtr hInstance, Int32 wParam);
        [DllImport("user32.dll")]   private static extern bool   UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")]   private static extern Int32  CallNextHookEx(Int32       idHook, Int32 nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
        [DllImport("kernel32.dll")] private static extern int    GetCurrentThreadId();
        [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandleW(IntPtr fakezero);

        private InputService input;

        public bool HookMouse() {
            if (_mouseHook == IntPtr.Zero) {
                _mouseProc = new MouseHookDelegate(MouseHookProc);
                _mouseHook = SetWindowsHookExW(WH_MOUSE_LL, _mouseProc, GetModuleHandleW(IntPtr.Zero), 0);
            }

            input = GameServices.GetService<InputService>();

            return _mouseHook != IntPtr.Zero;
        }

        public void UnHookMouse() {
            if (_mouseHook == IntPtr.Zero) return;

            UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }

        public bool NonClick { get; private set; } = false;

        private Int32 MouseHookProc(Int32 nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam) {
            int action = wParam.ToInt32();
            if (this.NonClick && action == 517) {
                this.NonClick = false;

            } else if (action > 512 && input.HudFocused && action < 523 && !input.HookOverride) {
                input.ClickState = new InputService.MouseEvent((MouseMessages)action, lParam);

                if (action != 514)
                    return 1;
            } else if (action == 516) {
                this.NonClick = true;
            }
            return CallNextHookEx(WH_MOUSE_LL, nCode, wParam, ref lParam);
        }
    }

}
