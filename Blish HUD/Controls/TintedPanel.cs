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

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintContainer(spriteBatch, bounds);

            // TODO: Don't do this if there isn't enough content to need to scroll
            if (this.CanScroll) {
                bounds = this.ContentRegion.Add(0, 0, -12, 0);
            }

            spriteBatch.Draw(ContentService.Textures.Pixel, bounds, Color.Black * 0.4f);
        }

    }
}
