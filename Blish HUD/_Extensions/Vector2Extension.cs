using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public static class Vector2Extension {

        public static Vector2 OffsetBy(this Vector2 v, float xOffset, float yOffset) {
            return new Vector2(v.X + xOffset, v.Y + yOffset);
        }

    }
}
