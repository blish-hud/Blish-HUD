using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public static class VectorUtil {

        public static Vector3 UpVectorFromCameraForward(Vector3 camForward) {
            var cameraRight = Vector3.Cross(camForward,  Vector3.Backward);
            var cameraUp    = Vector3.Cross(cameraRight, camForward);
            cameraUp.Normalize();

            return cameraUp;
        }

    }

}
