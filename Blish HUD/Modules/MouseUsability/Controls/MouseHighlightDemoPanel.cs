using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.MouseUsability {

    public class MouseHighlightDemoPanel:Controls.Container {

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw background
            spriteBatch.Draw(Content.GetTexture(@"common\solid"), bounds, Color.Black * 0.3f);
        }

    }

}
