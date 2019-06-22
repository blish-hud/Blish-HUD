using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {

    /// <summary>
    /// Utilities that help provide consistent value type parsing despite possible differences in the <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public static class InvariantUtil {

        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Converts the string representation of a number in <see cref="NumberStyles.Any"/> style and <see cref="CultureInfo.InvariantCulture"/> format to its <see langword="float" /> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A string containing a number to convert.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns><see langword="true" /> if <param name="value"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryParseFloat(string value, out float result) => float.TryParse(value, NumberStyles.Any, _invariantCulture, out result);

        /// <summary>
        /// Converts the string representation of a number in <see cref="NumberStyles.Any"/> style and <see cref="CultureInfo.InvariantCulture"/> format to its  <see langword="int" /> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">A string containing a number to convert.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns><see langword="true" /> if <param name="value"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryParseInt(string value, out int result) => int.TryParse(value, NumberStyles.Any, _invariantCulture, out result);

    }

}
