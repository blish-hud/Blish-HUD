using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming

namespace Blish_HUD.Utils {

    public static class WindowUtil {

        private const int GWL_STYLE   = -16;
        private const int GWL_EXSTYLE = -20;

        private const uint CS_VREDRAW = 0x0001;
        private const uint CS_HREDRAW = 0x0002;

        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("Dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, UIntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private struct Margins {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        internal static void SetForegroundWindowEx(IntPtr handle) => SetForegroundWindow(handle);

        private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex) => IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);

        private static int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong) => IntPtr.Size != 8 ? SetWindowLong32(hWnd, nIndex, dwNewLong) : (int) SetWindowLongPtr64(hWnd, nIndex, new UIntPtr(dwNewLong));

        internal static void SetupOverlay(IntPtr winHandle) {
            SetWindowLong(winHandle, GWL_STYLE, CS_HREDRAW | CS_VREDRAW);

            SetWindowLong(winHandle, GWL_EXSTYLE, (uint)GetWindowLong(winHandle, GWL_EXSTYLE) | WindowStyles.WS_EX_LAYERED | WindowStyles.WS_EX_TRANSPARENT);

            SetLayeredWindowAttributes(winHandle, 0, 0, 1);
            SetLayeredWindowAttributes(winHandle, 0, 255, 2);
        }

        private static Rectangle pos;

        private static bool _onTop = true;

        /// <summary>
        /// Indicates if the overlay window is currently forcing itself on top of Guild Wars 2 or not.
        /// </summary>
        public static bool OnTop => _onTop;

        internal static bool UpdateOverlay(IntPtr winHandle, IntPtr gw2WindowHandle) {
            var clientRect = new RECT();
            bool errGetClientRectResult = GetClientRect(gw2WindowHandle, ref clientRect);

            // Probably errors caused by gw2 closing at the time of the call or the call is super early and has the wrong handle somehow
            if (!errGetClientRectResult) {
                Console.WriteLine($"{nameof(GetClientRect)} failed with error code {Marshal.GetLastWin32Error()}.");
                return false;
            }

            var marg = new Margins {
                cxLeftWidth = 0,
                cyTopHeight = 0,
                cxRightWidth = clientRect.Right,
                cyBottomHeight = clientRect.Bottom
            };

            var screenPoint = System.Drawing.Point.Empty;
            bool errClientToScreen = ClientToScreen(gw2WindowHandle, ref screenPoint);

            // Probably errors caused by gw2 closing at the time of the call
            if (!errClientToScreen) {
                Console.WriteLine($"{nameof(ClientToScreen)} failed with error code {Marshal.GetLastWin32Error()}.");
                return false;
            }

            GameService.Debug.StartTimeFunc("GetForegroundWindow");
            var activeWindowHandle = GetForegroundWindow();
            GameService.Debug.StopTimeFunc("GetForegroundWindow");

            // If gw2 is not the focused application, stop being
            // topmost so that whatever is active can render on top
            if (activeWindowHandle != gw2WindowHandle && Form.ActiveForm == null) {
                if (_onTop) {
                    Console.WriteLine("GW2 is no longer the active window.");
                    SetWindowPos(winHandle,  HWND_NOTOPMOST, pos.X, pos.Y, pos.Width, pos.Height, 0);

                    _onTop = false;
                }
                return true;
            }

            if (!_onTop)
                Console.WriteLine("GW2 is now the active window - reactivating the overlay.");

            if (clientRect.Left + screenPoint.X != pos.X || clientRect.Top + screenPoint.Y != pos.Y || clientRect.Right - clientRect.Left != pos.Width || clientRect.Bottom - clientRect.Top != pos.Height || _onTop == false) {
                pos = new Rectangle(
                    clientRect.Left + screenPoint.X,
                    clientRect.Top + screenPoint.Y,
                    clientRect.Right - clientRect.Left,
                    clientRect.Bottom - clientRect.Top
                );

                GameService.Graphics.Resolution = new Point(
                    pos.Width,
                    pos.Height
                );

                SetWindowPos(winHandle, HWND_TOPMOST, clientRect.Left + screenPoint.X, clientRect.Top + screenPoint.Y, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top, 0);
                
                DwmExtendFrameIntoClientArea(winHandle, ref marg);

                _onTop = true;
            }

            return true;
        }

        internal static string GetClassNameOfWindow(IntPtr hwnd) {
            string        className = "";
            StringBuilder classText = null;
            try {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!string.IsNullOrEmpty(classText.ToString()))
                    className = classText.ToString();
            } catch (Exception ex) {
                className = ex.Message;
            }
            return className;
        }

        //private static RECT prevProcRect;
        //private static System.Drawing.Point prevProcPos;

        //public static void UpdateOverlayPositionAndSize(IntPtr thisWindowHandle, Process process) {
        //    var processWindowHandle = process.MainWindowHandle;

        //    var procRect = new RECT();
        //    GetClientRect(processWindowHandle, ref procRect);

        //    var procPos = new System.Drawing.Point();
        //    ClientToScreen(processWindowHandle, ref procPos);

        //    if (procPos != prevProcPos || !procRect.Equals(prevProcRect)) {
        //        GameService.Graphics.Resolution = new Point(
        //            procRect.Right - procRect.Left,
        //            procRect.Bottom - procRect.Top
        //        );

        //        SetWindowPos(
        //            thisWindowHandle,
        //            HWND_TOPMOST,
        //            procPos.X,
        //            procPos.Y,
        //            procRect.Right - procRect.Left,
        //            procRect.Bottom - procRect.Top,
        //            SWP_SHOWWINDOW
        //        );

        //        prevProcPos = procPos;
        //        prevProcRect = procRect;
        //    }
        //}
    }
}
