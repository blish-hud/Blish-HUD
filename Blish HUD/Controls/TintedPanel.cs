using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class TintedPanel : Panel {

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var tintBounds = new Rectangle(Point.Zero, _size);

            // TODO: Don't do this if there isn't enough content to need to scroll
            if (this.CanScroll) {
                tintBounds = tintBounds.Add(0, 0, -12, 0);
            }

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, tintBounds, Color.Black * 0.4f);
        }

    }
}
