using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public static class Vector3Extensions {

        /// <summary>
        /// Converts Gw2Sharp's left-handed <see cref="Coordinates3"/> to XNA's right-handed <see cref="Vector3"/>.
        /// </summary>
        public static Vector3 ToXnaVector3(this Coordinates3 vector) {
            return new Vector3((float)vector.X, (float)vector.Z, (float)vector.Y);
        }

        public static string ToRoundedString(this Vector3 vector) {
            return $"X: {vector.X:0,0} Y: {vector.Y:0,0} Z: {vector.Z:0,0}";
        }

    }

}
