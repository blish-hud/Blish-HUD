namespace Blish_HUD {
    internal static class PropertyUtil {

        /// <summary>
        /// Set a value to another value if the values do not match.
        /// Returns <c>true</c> if the value was changed.  Otherwise <c>false</c>.
        /// </summary>
        public static bool SetProperty<T>(ref T property, in T newValue) {
            if (Equals(property, newValue)) {
                return false;
            }

            property = newValue;

            return true;
        }

    }
}
