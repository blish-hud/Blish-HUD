using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
// ReSharper disable InconsistentNaming

namespace Blish_HUD.Input {
    public class MouseHook {

        private static readonly Logger Logger = Logger.GetLogger<MouseHook>();

        internal struct MSLLHOOKSTRUCT {
            public Point  pt;
            public Int32  mouseData;
            public Int32  flags;
            public Int32  time;
            public IntPtr extra;

            public Int32 wheelDelta {
                get {
                    int v = Convert.ToInt32((mouseData & 0xFFFF0000) >> 16);
                    if (v > SystemInformation.MouseWheelScrollDelta) v -= (ushort.MaxValue + 1);
                    return v;
                }
            }
        }

        private const Int32 WH_MOUSE_LL = 14;
        private       IntPtr _mouseHook;

        private delegate Int32 MouseHookDelegate(Int32 nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);

        [MarshalAs(UnmanagedType.FunctionPtr)]
        private MouseHookDelegate _mouseProc;

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

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookExW(Int32 idHook, MouseHookDelegate HookProc, IntPtr hInstance, Int32 wParam);

        [DllImport("user32.dll")]
        private static extern Int32  CallNextHookEx(Int32 idHook, Int32 nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);

        public MouseHook() => _mouseProc = MouseHookProc;

        public bool HookMouse() {
            Logger.Debug("Enabling mouse hook.");

            if (_mouseHook == IntPtr.Zero) {
                _mouseHook = SetWindowsHookExW(WH_MOUSE_LL, _mouseProc, HookExtern.GetModuleHandleW(IntPtr.Zero), 0);
            }

            return _mouseHook != IntPtr.Zero;
        }

        public void UnhookMouse() {
            Logger.Debug("Disabling the mouse hook.");

            if (_mouseHook == IntPtr.Zero) return;

            HookExtern.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }

        private bool _cameraDragging;

        private Int32 MouseHookProc(Int32 nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam) {
            int action = wParam.ToInt32();
            if (_cameraDragging && action == 517) { // If the player has been holding WM_RightButtonDown, then we ignore WM_RightButtonUp (they are just releasing the camera)
                _cameraDragging = false;

            } else if (action > 512 && GameService.Input.HudFocused && action < 523 && !GameService.Input.HookOverride) {
                GameService.Input.ClickState = new MouseEvent((MouseMessages)action, lParam);

                if (action != 514)
                    return 1;

            } else if (action == 516) { // If WM_RightButtonDown, we ignore it so that we don't accidentally intercept the player moving the camera
                _cameraDragging = true;
            }

            return CallNextHookEx(WH_MOUSE_LL, nCode, wParam, ref lParam);
        }
    }

}
