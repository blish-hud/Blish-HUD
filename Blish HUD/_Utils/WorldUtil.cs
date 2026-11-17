namespace Blish_HUD {

    public static class WorldUtil {

        private const float METER_TO_INCH = 39.37f;

        /// <summary>
        /// Converts a world (meters) coordinate to game (inches) coordinate.
        /// </summary>
        public static float WorldToGameCoord(float worldCoord) {
            return worldCoord * METER_TO_INCH;
        }

        /// <summary>
        /// Converts a game (inches) coordinate to world (meters) coordinate.
        ///
        /// World coordinates are GW2 coordinates.  Game coordinates are Blish HUD coordinates.
        /// </summary>
        public static float GameToWorldCoord(float gameCoord) {
            return gameCoord / METER_TO_INCH;
        }

    }

}
