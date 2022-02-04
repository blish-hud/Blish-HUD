using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Common.Gw2 {

    /// <summary>
    /// A collection of the built-in GW2 keybindings.
    /// </summary>
    public static class KeyBindings {

        /// <summary>
        /// General context-sensitive interact prompt.
        /// Used for interacting with the environment, including Talk, Loot, Revive, etc.
        /// </summary>
        public static readonly KeyBinding Interact = GameService.Overlay.InteractKey.Value;

    }
}
