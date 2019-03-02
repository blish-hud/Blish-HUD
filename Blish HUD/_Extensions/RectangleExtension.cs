using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {
    public static class RectangleExtension {

        public static Rectangle Add(this Rectangle u1, Rectangle u2) {
            return new Rectangle(u1.X + u2.X, u1.Y + u2.Y, u1.Width + u2.Width, u1.Height + u2.Height);
        }

        public static Rectangle Add(this Rectangle u1, int x, int y, int width, int height) {
            return new Rectangle(u1.X + x, u1.Y + y, u1.Width + width, u1.Height + height);
        }

        public static Rectangle Subtract(this Rectangle u1, Rectangle u2) {
            return new Rectangle(u1.X - u2.X, u1.Y - u2.Y, u1.Width - u2.Width, u1.Height - u2.Height);
        }

        public static Rectangle OffsetBy(this Rectangle r1, Point r2) {
            return new Rectangle(r1.Location + r2, r1.Size);
        }

        public static Rectangle OffsetBy(this Rectangle r1, int p1, int p2) {
            return new Rectangle(r1.Location + new Point(p1, p2), r1.Size);
        }

    }
}
