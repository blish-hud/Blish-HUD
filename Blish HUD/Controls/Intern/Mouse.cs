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
        /// <summary>
        /// Presses a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to press.</param>
        public static void Press(MouseButton button)
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
                            dx = GameService.Input.MouseState.X,
                            dy = GameService.Input.MouseState.Y,
                            mouseData = 0,
                            dwFlags = ButtonPress[button],
                            time = 0
                        }
                    }
                }
            };
            PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
        }

        /// <summary>
        /// Releases a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to release.</param>
        public static void Release(MouseButton button)
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
                            dx = GameService.Input.MouseState.X,
                            dy = GameService.Input.MouseState.Y,
                            mouseData = 0,
                            dwFlags = ButtonRelease[button],
                            time = 0
                        }
                    }
                }
            };
            PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
        }
        /// <summary>
        /// Rotates the mouse wheel.
        /// </summary>
        /// <param name="wheelDistance">Distance of movement. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.</param>
        public static void RotateWheel(int wheelDistance)
        {
            if (wheelDistance == 0) return;
            var nInputs = new[]
            {
                new Extern.Input
                {
                    type = InputType.MOUSE,
                    U = new InputUnion
                    {
                        mi = new MouseInput
                        {
                            dx = GameService.Input.MouseState.X,
                            dy = GameService.Input.MouseState.Y,
                            mouseData = wheelDistance,
                            dwFlags = MouseEventF.WHEEL,
                            time = 0
                        }
                    }
                }
            };
            PInvoke.SendInput((uint)nInputs.Length, nInputs, Extern.Input.Size);
        }
        /// <summary>
        /// Sets the cursors absolute position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetPos(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);
        }
    }
}