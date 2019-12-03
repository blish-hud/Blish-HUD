using System;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    public class KeyboardEventArgs : EventArgs {

        /// <summary>
        /// The type of keyboard event.
        /// </summary>
        public KeyboardEventType EventType { get; }

        /// <summary>
        /// The key that triggered the event.
        /// </summary>
        public Keys Key { get; }

        public KeyboardEventArgs(KeyboardEventType eventType, Keys key) {
            this.EventType = eventType;
            this.Key       = key;
        }

    }
}
