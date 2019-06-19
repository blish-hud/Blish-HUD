using Microsoft.Xna.Framework;
using System;
using Blish_HUD.Controls;

namespace Blish_HUD {
    public static class RectangleExtension {

        public static Rectangle WithPadding(this Rectangle rect, Thickness thickness) {
            return  new Rectangle(rect.X - (int)thickness.Left, rect.Y - (int)thickness.Top, rect.Width + (int)thickness.Left + (int)thickness.Right, rect.Height + (int)thickness.Top + (int)thickness.Bottom);
        }

        public static Rectangle Add(this Rectangle u1, Rectangle u2) {
            return new Rectangle(u1.X + u2.X, u1.Y + u2.Y, u1.Width + u2.Width, u1.Height + u2.Height);
        }

        public static Rectangle Add(this Rectangle u1, int x, int y, int width, int height) {
            return new Rectangle(u1.X + x, u1.Y + y, u1.Width + width, u1.Height + height);
        }

        public static Rectangle WithSetDimension(this Rectangle rect, int? x, int? y, int? width, int? height) {
            return new Rectangle(x      ?? rect.X,
                                 y      ?? rect.Y,
                                 width  ?? rect.Width,
                                 height ?? rect.Height);
        }

        public static Rectangle Subtract(this Rectangle u1, Rectangle u2) {
            return new Rectangle(u1.X - u2.X, u1.Y - u2.Y, u1.Width - u2.Width, u1.Height - u2.Height);
        }

        public static Rectangle OffsetBy(this Rectangle r1, Point r2) {
            return new Rectangle(r1.Location + r2, r1.Size);
        }

        public static Rectangle ToBounds(this Rectangle r1, Rectangle bounds) {
            return new Rectangle(r1.Location + bounds.Location, r1.Size);
        }

        public static Rectangle MoveRelativeToBoundsLocation(this Rectangle r1, Point boundsLocation) {
            return new Rectangle(r1.Location + boundsLocation, r1.Size);
        }

        public static Rectangle OffsetBy(this Rectangle r1, int p1, int p2) {
            return new Rectangle(r1.Location + new Point(p1, p2), r1.Size);
        }

        public static Rectangle ScaleBy(this Rectangle rectangle, float scale) {
            return new Rectangle((int)Math.Floor(rectangle.X * scale), (int)Math.Floor(rectangle.Y * scale), (int)Math.Ceiling(rectangle.Width * scale), (int)Math.Ceiling(rectangle.Height * scale));
        }

    }
}
