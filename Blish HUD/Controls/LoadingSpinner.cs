using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class LoadingSpinner : Control {

        private const int DRAWLENGTH = 64;

        public LoadingSpinner() {
            this.Size = new Point(DRAWLENGTH, DRAWLENGTH);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
        }

    }
}
