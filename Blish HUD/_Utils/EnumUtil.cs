using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD {
    public static class EnumUtil {

        private static readonly Dictionary<Type, Array> _cachedEnumValues = new Dictionary<Type, Array>();

        /// <summary>
        /// Returns the individual values in an enum as an array.
        /// The results are cached so future calls do not repeat calls to <see cref="Enum.GetValues"/>.
        /// </summary>
        public static T[] GetCachedValues<T>() where T : Enum {
            if (!_cachedEnumValues.ContainsKey(typeof(T))) {
                _cachedEnumValues.Add(typeof(T), Enum.GetValues(typeof(T)));
            }

            return _cachedEnumValues[typeof(T)].Cast<T>().ToArray();
        }

    }
}
