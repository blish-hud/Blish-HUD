using System;

namespace Microsoft.Xna.Framework.Input {

    [Flags]
    public enum ModifierKeys {
        /// <summary>
        /// No modifier keys.
        /// </summary>
        None  = 0,

        /// <summary>
        /// The Control key.
        /// </summary>
        Ctrl  = 1,

        /// <summary>
        /// The Alt key.
        /// </summary>
        Alt   = 2,

        /// <summary>
        /// The Shift key.
        /// </summary>
        Shift = 4
    }

}