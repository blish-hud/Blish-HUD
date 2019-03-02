using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Paths {

    /// <summary>
    /// Uses a series of dots to guide the player along a route.
    /// Meant to match the functionality of routes imported in from TacO.
    /// </summary>
    [Obsolete("This path type is implemented primarily to match TacO functionality."
            + "You may prefer to utilize FollowablePath, which provides a more obvious path (AugTyr's method).")]
    public class Route : Path {

        public override void Update(GameTime gameTime) {
            
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            
        }

    }
}
