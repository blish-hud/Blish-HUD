using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Utils {

    public class GWL {
        public const int WNDPROC = (-4);
        public const int HINSTANCE = (-6);
        public const int HWNDPARENT = (-8);
        public const int STYLE = (-16);
        public const int EXSTYLE = (-20);
        public const int USERDATA = (-21);
        public const int ID = (-12);
    }

    public static class Window {

        private const uint CS_VREDRAW = 0x0001;
        private const uint CS_HREDRAW = 0x0002;

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private static readonly uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        static extern bool AdjustWindowRect(ref RECT lpRect, uint dwStyle, bool bMenu);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("Dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LockSetForegroundWindow(uint uLockCode);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

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

        public struct MARGINS {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex) {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong) {
            if (IntPtr.Size != 8)
                return SetWindowLong32(hWnd, nIndex, dwNewLong);
            return (int) SetWindowLongPtr64(hWnd, nIndex, new UIntPtr(dwNewLong));
        }

        internal static void SetupOverlay(IntPtr winHandle) {
            //int width = 600;
            //int height = 600;

            //var marg = new MARGINS() {
            //    cxLeftWidth = 0,
            //    cyTopHeight = 0,
            //    cxRightWidth = width,
            //    cyBottomHeight = height
            //};

            //DwmExtendFrameIntoClientArea(winHandle, ref marg);

            //var windowRect = new RECT();
            //GetWindowRect(winHandle, ref windowRect);
            //windowRect.Right = windowRect.Left + width;
            //windowRect.Bottom = windowRect.Top + height;

            //AdjustWindowRect(ref windowRect, CS_HREDRAW | CS_VREDRAW, false);

            //SetWindowPos(winHandle, IntPtr.Zero, windowRect.Top, windowRect.Left, width, height, 0);
            SetWindowLong(winHandle, GWL.STYLE, CS_HREDRAW | CS_VREDRAW);

            SetWindowLong(winHandle, GWL.EXSTYLE, (uint)GetWindowLong(winHandle, GWL.EXSTYLE) | WindowStyles.WS_EX_LAYERED | WindowStyles.WS_EX_TRANSPARENT);

            SetLayeredWindowAttributes(winHandle, 0, 0, 1);
            SetLayeredWindowAttributes(winHandle, 0, 255, 2);
        }

        private static Rectangle pos;
        public static bool OnTop = true; // Used to force a SetWindowPos with HWND_TOPMOST when GW2 becomes active again

        internal static bool UpdateOverlay(IntPtr winHandle, IntPtr gw2WindowHandle) {
            var clientRect = new RECT();
            bool errGetClientRectResult = GetClientRect(gw2WindowHandle, ref clientRect);

            // Probably errors caused by gw2 closing at the time of the call or the call is super early and has the wrong handle somehow
            if (!errGetClientRectResult) {
                Console.WriteLine($"{nameof(GetClientRect)} failed with error code {Marshal.GetLastWin32Error()}.");
                return false;
            }

            var marg = new MARGINS {
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
            if (activeWindowHandle != gw2WindowHandle) {
                if (OnTop) {
                    Console.WriteLine("GW2 is no longer the active window.");
                    SetWindowPos(winHandle,  HWND_NOTOPMOST, pos.X, pos.Y, pos.Width, pos.Height, 0);

                    OnTop = false;
                }
                return true;
            }

            if (!OnTop)
                Console.WriteLine("GW2 is now the active window - reactivating the overlay.");

            if (clientRect.Left + screenPoint.X != pos.X || clientRect.Top + screenPoint.Y != pos.Y || clientRect.Right - clientRect.Left != pos.Width || clientRect.Bottom - clientRect.Top != pos.Height || OnTop == false) {
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
                
                //SetLayeredWindowAttributes(winHandle, 0, 0,   1);
                //SetLayeredWindowAttributes(winHandle, 0, 255, 2);

                OnTop = true;
            }

            return true;
        }

        public static string GetClassNameOfWindow(IntPtr hwnd) {
            string        className = "";
            StringBuilder classText = null;
            try {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!string.IsNullOrEmpty(classText.ToString()) && !string.IsNullOrWhiteSpace(classText.ToString()))
                    className = classText.ToString();
            } catch (Exception ex) {
                className = ex.Message;
            }
            return className;
        }

        private static RECT prevProcRect;
        private static System.Drawing.Point prevProcPos;
        public static void UpdateOverlayPositionAndSize(IntPtr thisWindowHandle, Process process) {
            var processWindowHandle = process.MainWindowHandle;

            var procRect = new RECT();
            GetClientRect(processWindowHandle, ref procRect);

            var procPos = new System.Drawing.Point();
            ClientToScreen(processWindowHandle, ref procPos);

            if (procPos != prevProcPos || !procRect.Equals(prevProcRect)) {
                GameServices.GetService<GraphicsService>().Resolution = new Point(
                    procRect.Right - procRect.Left,
                    procRect.Bottom - procRect.Top
                );

                SetWindowPos(
                    thisWindowHandle,
                    HWND_TOPMOST,
                    procPos.X,
                    procPos.Y,
                    procRect.Right - procRect.Left,
                    procRect.Bottom - procRect.Top,
                    SWP_SHOWWINDOW
                );

                prevProcPos = procPos;
                prevProcRect = procRect;
            }
        }
    }
}
