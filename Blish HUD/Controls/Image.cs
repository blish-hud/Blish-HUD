using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Image : Control {

        protected AsyncTexture2D _texture;
        public AsyncTexture2D Texture {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

        private SpriteEffects _spriteEffects;
        public SpriteEffects SpriteEffects {
            get => _spriteEffects;
            set => SetProperty(ref _spriteEffects, value);
        }

        private Rectangle? _sourceRectangle;
        public Rectangle SourceRectangle {
            get => _sourceRectangle ?? _texture.Texture.Bounds;
            set => SetProperty(ref _sourceRectangle, value);
        }

        private Color _tint = Color.White;
        public Color Tint {
            get => _tint;
            set => SetProperty(ref _tint, value);
        }

        public Image() { /* NOOP */ }

        public Image(AsyncTexture2D texture) {
            this.Texture = texture;
            this.Size = texture.Texture.Bounds.Size;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_texture == null) return;

            // Draw the texture
            spriteBatch.DrawOnCtrl(this,
                                   _texture,
                                   bounds,
                                   this.SourceRectangle,
                                   _tint,
                                   0f,
                                   Vector2.Zero,
                                   _spriteEffects);
        }

    }
}
