using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public static class ColorUtil {

        public static bool TryParseHex(string hex, out Color result) {
            hex = hex.TrimStart('#');

            if (hex.Length == 6) {
                hex = "FF" + hex;
            }

            if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int clrVal)) {
                result = default;
                return false;
            }

            result = new Color((clrVal >> 16) & 0xFF,
                               (clrVal >> 8)  & 0xFF,
                               (clrVal >> 0)  & 0xFF,
                               (clrVal >> 24) & 0xFF);

            return true;
        }

    }
}
