using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public static class PointExtensions {

        public static System.Drawing.Point ToSystemDrawingPoint(this Microsoft.Xna.Framework.Point point) {
            return new System.Drawing.Point(point.X, point.Y);
        }

        public static Microsoft.Xna.Framework.Point ScaleToUi(this Microsoft.Xna.Framework.Point point) {
            return new Microsoft.Xna.Framework.Point((int)(point.X * GameService.Graphics.UIScaleMultiplier),
                                                     (int)(point.Y * GameService.Graphics.UIScaleMultiplier));
        }

        public static Microsoft.Xna.Framework.Point UiToScale(this Microsoft.Xna.Framework.Point point) {
            float scale = GameService.Graphics.GetScaleRatio(GameService.Graphics.UIScale);

            return new Microsoft.Xna.Framework.Point((int)(point.X / GameService.Graphics.UIScaleMultiplier),
                                                     (int)(point.Y / GameService.Graphics.UIScaleMultiplier));
        }

        public static Rectangle InBounds(this Point point, Rectangle bounds) {
            return new Rectangle(bounds.Location, point);
        }

        public static System.Drawing.Size ToSystemDrawingSize(this Microsoft.Xna.Framework.Point point) {
            return new System.Drawing.Size(point.X, point.Y);
        }

        public static Microsoft.Xna.Framework.Point ToXnaPoint(this System.Drawing.Point point) {
            return new Microsoft.Xna.Framework.Point(point.X, point.Y);
        }

    }
}
