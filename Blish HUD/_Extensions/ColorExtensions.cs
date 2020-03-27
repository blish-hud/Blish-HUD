using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD._Extensions {

    public static class ColorExtensions {

        public static Microsoft.Xna.Framework.Color ToXnaColor(this Gw2Sharp.WebApi.V2.Models.ColorMaterial color) {
            return new Color(color.Rgb[0], color.Rgb[1], color.Rgb[2]);
        }

    }

}