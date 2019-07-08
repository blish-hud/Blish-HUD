using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class LoadingSpinner : Control {

        private const string DRAWATLAS = "spinner-atlas";
        private const int DRAWLENGTH = 64;

        private Rectangle _activeAtlasRegion = new Rectangle(0, 416, 256, 32);

        public int Rotation { get; set; } = 0;

        public LoadingSpinner() {
            this.Size = new Point(DRAWLENGTH, DRAWLENGTH);

            Animation.Tweener.Tween(this, new { Rotation = 64 }, 3).Repeat().Round();
        }

        public override void DoUpdate(GameTime gameTime) {
            _activeAtlasRegion = new Rectangle(DRAWLENGTH * this.Rotation, 0, DRAWLENGTH, DRAWLENGTH);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // TODO: Add this texture in with the rest of the UI elements in the ControlUI atlas
            spriteBatch.DrawOnCtrl(this, Content.GetTexture(DRAWATLAS), bounds, _activeAtlasRegion);
        }

    }
}
