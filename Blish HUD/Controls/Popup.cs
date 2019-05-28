using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    // TODO: Popup control needs to be implemented more like it is in the events module before it is ready
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Popup:Control {

        private Popup() {
            this.Visible = false;
            this.Parent = Graphics.SpriteScreen;
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;
            this.Size = new Point(625, 425);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var tooltipBack = Content.GetTexture("tooltip");

            spriteBatch.Draw(tooltipBack, bounds.Add(0, 0, -3, -3), new Rectangle(0, 0, this.Width - 3, this.Height - 3), Color.White);
            spriteBatch.Draw(tooltipBack, new Rectangle(bounds.Right - 3, bounds.Top, 3, bounds.Height), new Rectangle(0, 3, 3, this.Height - 3), Color.White);
            spriteBatch.Draw(tooltipBack, new Rectangle(bounds.Left, bounds.Bottom - 3, bounds.Width, 3), new Rectangle(3, 0, this.Width - 6, 3), Color.White);
        }

    }

}
