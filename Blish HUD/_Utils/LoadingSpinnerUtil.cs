using System;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {
    public static class LoadingSpinnerUtil {

        #region Load Static

        private static readonly Texture2D _loadingSpinnerTexture;

        static LoadingSpinnerUtil() {
            _loadingSpinnerTexture = GameService.Content.GetTexture("spinner-atlas");
        }

        #endregion

        /// <summary>
        /// Draws an animated loading spinner at the provided <param name="bounds">bounds</param>.
        /// </summary>
        /// <param name="control">The control the loading spinner will be drawn on.</param>
        /// <param name="spriteBatch">The active spritebatch.</param>
        /// <param name="bounds">The location to draw the loading spinner.</param>
        public static void DrawLoadingSpinner(Control control, SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(control,
                                   _loadingSpinnerTexture,
                                   bounds,
                                   new Rectangle(((int)(GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds * (64f / 3f))) % 64 * 64, 0, 64, 64));
        }

    }
}