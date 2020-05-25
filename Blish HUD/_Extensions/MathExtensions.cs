using System;

namespace Blish_HUD {
    public static class MathExtensions {
        /// <summary>
        /// Restricts a value to be between a minimum and a maximum.
        /// </summary>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }
    }
}