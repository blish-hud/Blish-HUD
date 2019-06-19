using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Image : Control {

        protected Texture2D _texture;
        public Texture2D Texture {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

        public Image() { /* NOOP */ }

        public Image(Texture2D texture) {
            this.Texture = texture;
            this.Size = texture.Bounds.Size;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_texture == null) return;

            // Draw the texture
            spriteBatch.DrawOnCtrl(
                                   this,
                                   _texture,
                                   bounds
                                  );
        }

    }
}
