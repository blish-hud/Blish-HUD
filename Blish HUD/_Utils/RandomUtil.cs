using System;
using System.Security.Cryptography;
using System.Text;
using MonoGame.Extended;

namespace Blish_HUD {
    public static class RandomUtil {

        private static readonly FastRandom _sharedRandom;
        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

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

        /// <summary>
        /// Generates a random key of the given length.
        /// </summary>
        /// <param name="length">The length of the key.</param>
        public static string GetUniqueKey(int length) {
            byte[] data = new byte[4 * length];
            using (var crypto = RandomNumberGenerator.Create()) {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++) {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % _chars.Length;

                result.Append(_chars[idx]);
            }

            return result.ToString();
        }

    }
}
