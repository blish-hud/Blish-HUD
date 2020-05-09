using System;
using System.Windows.Forms;
using Blish_HUD.Input.WinApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    public class MouseEventArgs : EventArgs {

        /// <summary>
        /// The type of mouse event.
        /// </summary>
        public MouseEventType EventType { get; }

        [Obsolete("Mouse state can be accessed directly via `GameService.Input.Mouse.State`.")]
        public MouseState MouseState => GameService.Input.Mouse.State;

        /// <summary>
        /// The mouse position when the event was fired relative to the application and scaled to the UI size.
        /// </summary>
        public Point MousePosition => GameService.Input.Mouse.Position;

        /// <summary>
        /// Indicates if the click event is considered a double-click.
        /// </summary>
        public bool IsDoubleClick { get; }

        internal int PointX { get; }

        internal int PointY { get; }

        internal int MouseData { get; }

        internal int Flags { get; }

        internal int Time { get; }

        internal int Extra { get; }

        internal int WheelDelta {
            get {
                int v = Convert.ToInt32((MouseData & 0xFFFF0000) >> 16);
                if (v > SystemInformation.MouseWheelScrollDelta) v -= (ushort.MaxValue + 1);
                return v;
            }
        }

        public MouseEventArgs(MouseEventType eventType) {
            this.EventType = eventType;
        }

        public MouseEventArgs(MouseEventType eventType, bool isDoubleClick) : this(eventType) {
            this.IsDoubleClick = isDoubleClick;
        }

        internal MouseEventArgs(MouseEventType eventType, MouseLLHookStruct details) : this(eventType, details.Point.X, details.Point.Y, details.MouseData, details.Flags, details.Time, (int)details.Extra) { }

        internal MouseEventArgs(MouseEventType eventType, int pointX, int pointY, int mouseData, int flags, int time, int extraInfo) : this(eventType) {
            this.PointX = pointX;
            this.PointY = pointY;
            this.MouseData = mouseData;
            this.Flags = flags;
            this.Time = time;
            this.Extra = extraInfo;
        }
    }
}
