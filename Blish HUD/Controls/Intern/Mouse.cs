using System;
using System.Collections.Generic;
using Blish_HUD.Controls.Extern;
namespace Blish_HUD.Controls.Intern
{
    public enum MouseButton
    {
        LEFT,
        RIGHT,
        MIDDLE,
        XBUTTON
    }
    public static class Mouse
    {
        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint WM_MOUSEHWHEEL = 0x020E;
        private const int WHEEL_DELTA = 120;
        private const uint WM_MOUSEMOVE = 0x0200;

        private static Dictionary<MouseButton, MouseEventF> ButtonPress = new Dictionary<MouseButton, MouseEventF>()
        {
            { MouseButton.LEFT, MouseEventF.LEFTDOWN },
            { MouseButton.RIGHT, MouseEventF.RIGHTDOWN },
            { MouseButton.MIDDLE, MouseEventF.MIDDLEDOWN },
            { MouseButton.XBUTTON, MouseEventF.XDOWN }
        };
        private static Dictionary<MouseButton, MouseEventF> ButtonRelease = new Dictionary<MouseButton, MouseEventF>()
        {
            { MouseButton.LEFT, MouseEventF.LEFTUP },
            { MouseButton.RIGHT, MouseEventF.RIGHTUP },
            { MouseButton.MIDDLE, MouseEventF.MIDDLEUP },
            { MouseButton.XBUTTON, MouseEventF.XUP }
        };
        private static Dictionary<MouseButton, VirtualKeyShort> VirtualButtonShort = new Dictionary<MouseButton, VirtualKeyShort>()
        {
            { MouseButton.LEFT, VirtualKeyShort.LBUTTON },
            { MouseButton.RIGHT, VirtualKeyShort.RBUTTON },
            { MouseButton.MIDDLE, VirtualKeyShort.MBUTTON },
            { MouseButton.XBUTTON, VirtualKeyShort.XBUTTON1 }
        };
        private static Dictionary<MouseButton, uint> WM_BUTTONDOWN = new Dictionary<MouseButton, uint>()
        {
            { MouseButton.LEFT, 0x0201 },
            { MouseButton.RIGHT, 0x0204 },
            { MouseButton.MIDDLE, 0x0207 },
            { MouseButton.XBUTTON, 0x020B }
        };
        private static Dictionary<MouseButton, uint> WM_BUTTONUP = new Dictionary<MouseButton, uint>()
        {
            { MouseButton.LEFT, 0x0202 },
            { MouseButton.RIGHT, 0x0205 },
            { MouseButton.MIDDLE, 0x0208 },
            { MouseButton.XBUTTON, 0x020C }
        };
        private static Dictionary<MouseButton, uint> WM_BUTTONDBLCLK = new Dictionary<MouseButton, uint>()
        {
            { MouseButton.LEFT, 0x0203 },
            { MouseButton.RIGHT, 0x0206 },
            { MouseButton.MIDDLE, 0x0209 },
            { MouseButton.XBUTTON, 0x020D }
        };
        /// <summary>
        /// Presses a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to press.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Press(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            if (xPos == -1 || yPos == -1)
            {
                xPos = GameService.Input.MouseState.X;
                yPos = GameService.Input.MouseState.Y;
            }
            if (!GameService.GameIntegration.Gw2IsRunning || sendToSystem)
            {
                var nInputs = new[]
                {
                    new Extern.Input
                    {
                        type = InputType.MOUSE,
                        U = new InputUnion
                        {
                            mi = new MouseInput
                            {
                                dx = xPos,
                                dy = yPos,
                                mouseData = 0,
                                dwFlags = ButtonPress[button],
                                time = 0
                            }
                        }
                    }
                };
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
            }
            else
            {
                uint wParam = (uint)VirtualButtonShort[button];
                int lParam = xPos | (yPos << 16);
                PInvoke.PostMessage(GameService.GameIntegration.Gw2WindowHandle, WM_BUTTONDOWN[button], wParam, lParam);
            }
        }
        /// <summary>
        /// Releases a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to release.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Release(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            if (xPos == -1 || yPos == -1)
            {
                xPos = GameService.Input.MouseState.X;
                yPos = GameService.Input.MouseState.Y;
            }
            if (!GameService.GameIntegration.Gw2IsRunning || sendToSystem)
            {
                var nInputs = new[]
                {
                    new Extern.Input
                    {
                        type = InputType.MOUSE,
                        U = new InputUnion
                        {
                            mi = new MouseInput
                            {
                                dx = xPos,
                                dy = yPos,
                                mouseData = 0,
                                dwFlags = ButtonRelease[button],
                                time = 0
                            }
                        }
                    }
                };
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
            }
            else
            {
                uint wParam = (uint)VirtualButtonShort[button];
                int lParam = xPos | (yPos << 16);
                PInvoke.PostMessage(GameService.GameIntegration.Gw2WindowHandle, WM_BUTTONUP[button], wParam, lParam);
            }
        }
        /// <summary>
        /// Rotates the mouse wheel.
        /// </summary>
        /// <param name="wheelDistance">Distance of movement by multiples or divisions of 120 (WHEEL_DELTA). A positive value indicates the wheel to rotate forward, away from the user; a negative value indicates the wheel to rotate backward, toward the user.</param>
        /// <param name="horizontalWheel">Indicates the wheel to rotate horizontally.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void RotateWheel(int wheelDistance, bool horizontalWheel = false, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            wheelDistance = wheelDistance % WHEEL_DELTA;
            if (wheelDistance == 0) return;

            if (xPos == -1 || yPos == -1)
            {
                xPos = GameService.Input.MouseState.X;
                yPos = GameService.Input.MouseState.Y;
            }
            if (!GameService.GameIntegration.Gw2IsRunning || sendToSystem)
            {
                var nInputs = new[]
                {
                    new Extern.Input
                    {
                        type = InputType.MOUSE,
                        U = new InputUnion
                        {
                            mi = new MouseInput
                            {
                                dx = xPos,
                                dy = yPos,
                                mouseData = wheelDistance,
                                dwFlags = horizontalWheel ? MouseEventF.HWHEEL : MouseEventF.WHEEL,
                                time = 0
                            }
                        }
                    }
                };
                PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
            }
            else
            {
                uint wParam = (uint)(0 | wheelDistance << 16);
                int lParam = xPos | (yPos << 16);
                PInvoke.PostMessage(GameService.GameIntegration.Gw2WindowHandle, horizontalWheel ? WM_MOUSEHWHEEL : WM_MOUSEWHEEL, wParam, lParam);
            }
        }
        /// <summary>
        /// Sets the cursors absolute position.
        /// </summary>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void SetPosition(int xPos, int yPos, bool sendToSystem = false)
        {
            if (!GameService.GameIntegration.Gw2IsRunning || sendToSystem)
            {
                PInvoke.SetCursorPos(xPos, yPos);
            }
            else
            {
                int lParam = xPos | (yPos << 16);
                PInvoke.PostMessage(GameService.GameIntegration.Gw2WindowHandle, WM_MOUSEMOVE, 0, lParam);
            }
        }
        /// <summary>
        /// Presses and immediately releases a mouse button ONCE.
        /// </summary>
        /// <param name="button">The mouse button to click.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Click(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            Press(button, xPos, yPos, sendToSystem);
            Release(button, xPos, yPos, sendToSystem);
        }
        /// <summary>
        /// Performs a double click of a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to click.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void DoubleClick(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            if (!GameService.GameIntegration.Gw2IsRunning || sendToSystem)
            {
                for (int i = 0; i <= 1; i++)
                {
                    Press(button, xPos, yPos, sendToSystem);
                    Release(button, xPos, yPos, sendToSystem);
                }
            }
            else
            {
                if (xPos == -1 || yPos == -1)
                {
                    xPos = GameService.Input.MouseState.X;
                    yPos = GameService.Input.MouseState.Y;
                }
                uint wParam = (uint)VirtualButtonShort[button];
                int lParam = xPos | (yPos << 16);
                PInvoke.PostMessage(GameService.GameIntegration.Gw2WindowHandle, WM_BUTTONDBLCLK[button], wParam, lParam);
            }
        }
    }
}