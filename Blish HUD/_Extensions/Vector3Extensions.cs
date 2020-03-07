using System;
using Gw2Sharp.Models;

namespace Blish_HUD {
    public static class Vector3Extensions {

        // Note: GW2 uses left handed coordinates and XNA uses right handed coordinates.
        public static Microsoft.Xna.Framework.Vector3 ToXnaVector3(this GW2NET.Common.Drawing.Vector3D gw2v3d) {
            return new Microsoft.Xna.Framework.Vector3((float)gw2v3d.X, (float)gw2v3d.Z, (float)gw2v3d.Y);
        }

        public static Microsoft.Xna.Framework.Vector3 ToXnaVector3(this Coordinates3 gw2v3d) {
            return new Microsoft.Xna.Framework.Vector3((float)gw2v3d.X, (float)gw2v3d.Z, (float)gw2v3d.Y);
        }

        public static string ToRoundedString(this Microsoft.Xna.Framework.Vector3 v3) {
            return String.Format("X: {0:0,0} Y: {1:0,0} Z: {2:0,0}",
                v3.X,
                v3.Y,
                v3.Z
            );
        }

    }
}
