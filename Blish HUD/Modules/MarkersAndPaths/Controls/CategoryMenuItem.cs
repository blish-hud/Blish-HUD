using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.MarkersAndPaths.Controls {
    public class CategoryMenuItem : Blish_HUD.Controls.MenuItem {

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintContainer(spriteBatch, bounds);

            Utils.DrawUtil.DrawAlignedText(
                                           spriteBatch,
                                           Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular),
                                           "Test Item 1",
                                           bounds,
                                           Color.White,
                                           DrawUtil.HorizontalAlignment.Right,
                                           DrawUtil.VerticalAlignment.Top);

            Utils.DrawUtil.DrawAlignedText(
                                           spriteBatch,
                                           Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular),
                                           "Test Item 2",
                                           bounds,
                                           Color.White,
                                           DrawUtil.HorizontalAlignment.Right,
                                           DrawUtil.VerticalAlignment.Bottom);
        }

    }
}
