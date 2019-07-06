using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.Content {
    public enum RenderServiceTextureSize {
        /// <summary>
        /// When using an alternative render service, the texture will be requested to return as 64x64.
        /// </summary>
        X64 = 64,
        /// <summary>
        /// When using an alternative render service, the texture will be requested to return as 32x32.
        /// </summary>
        X32 = 32,
        /// <summary>
        /// When using an alternative render service, the texture will be requested to return as 16x16.
        /// </summary>
        X16 = 16,
        /// <summary>
        /// Does not specify a size in the request.
        /// </summary>
        Unspecified = X64
    }
}
