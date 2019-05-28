using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Control = Blish_HUD.Controls.Control;

namespace Blish_HUD.Controls {
    public class Image : Control {

        protected Texture2D _texture;
        public Texture2D Texture {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

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
                                   new Rectangle(Point.Zero, _size)
                                  );
        }

    }
}
