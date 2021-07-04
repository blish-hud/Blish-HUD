using System;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Content {
    public class AsyncTextureSwapEventArgs : EventArgs {

        /// <summary>
        /// The active texture before the swap
        /// </summary>
        public Texture2D OldTexture { get; }

        /// <summary>
        /// The new active texture after the swap
        /// </summary>
        public Texture2D NewTexture { get; }

        public AsyncTextureSwapEventArgs(Texture2D oldTexture, Texture2D newTexture) {
            this.OldTexture = oldTexture;
            this.NewTexture = newTexture;
        }
    }
}
