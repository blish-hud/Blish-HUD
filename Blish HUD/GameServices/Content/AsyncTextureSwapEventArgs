using System;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Content {
    public class AsyncTextureSwapEventArgs : EventArgs {

        /// <summary>
        /// The type of keyboard event.
        /// </summary>
        public Texture2D OldTexture { get; }

        /// <summary>
        /// The key that triggered the event.
        /// </summary>
        public Texture2D NewTexture { get; }

        public AsyncTextureSwapEventArgs(Texture2D oldTexture, Texture2D newTexture) {
            this.OldTexture = oldTexture;
            this.NewTexture = newTexture;
        }
    }
}
