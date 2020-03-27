using System;
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

        internal MouseLLHookStruct Details { get; }

        public MouseEventArgs(MouseEventType eventType) {
            this.EventType = eventType;
        }

        public MouseEventArgs(MouseEventType eventType, bool isDoubleClick) : this(eventType) {
            this.IsDoubleClick = isDoubleClick;
        }

        internal MouseEventArgs(MouseEventType eventType, MouseLLHookStruct details) : this(eventType) {
            this.Details = details;
        }

    }
}
