using System;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls;

namespace Blish_HUD {
    public static class ControlUtil {

        public static Point GetControlBounds(Control[] controls) {
            int farthestRight = 0;
            int farthestDown  = 0;

            foreach (var child in controls) {
                if (child == null) continue;

                farthestRight = Math.Max(farthestRight, child.Right);
                farthestDown  = Math.Max(farthestDown,  child.Bottom);
            }

            return new Point(farthestRight, farthestDown);
        }

    }
}