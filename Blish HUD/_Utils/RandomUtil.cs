using System;
using MonoGame.Extended;

namespace Blish_HUD {
    public static class RandomUtil {

        private static readonly FastRandom _sharedRandom;

        static RandomUtil() {
            _sharedRandom = new FastRandom();
        }

        /// <summary>
        /// Returns a random integer that is within a specific range.
        /// </summary>
        public static int GetRandom(int minValue, int maxValue) {
            return _sharedRandom.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer that is within a specific range from a specified seed value.
        /// </summary>
        public static (int, FastRandom) GetRandomWithSeed(int minValue, int maxValue, int seed) {
            var seededRandom = new FastRandom(seed);

            return (seededRandom.Next(minValue, maxValue), seededRandom);
        }

    }
}
