using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public static class Vector2Extension {

        public static Vector2 OffsetBy(this Vector2 vector, float xOffset, float yOffset) {
            return new Vector2(vector.X + xOffset, vector.Y + yOffset);
        }

        public static Vector2 ToWorldCoord(this Vector2 vector) {
            return new Vector2(Blish_HUD.WorldUtil.GameToWorldCoord(vector.X), Blish_HUD.WorldUtil.GameToWorldCoord(vector.Y));
        }

        public static Vector2 ToGameCoord(this Vector2 vector) {
            return new Vector2(Blish_HUD.WorldUtil.WorldToGameCoord(vector.X), Blish_HUD.WorldUtil.WorldToGameCoord(vector.Y));   
        }

    }
}
