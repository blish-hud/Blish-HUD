using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {
    public interface IRenderable2D {

        void Draw(GraphicsDevice graphicsDevice, Rectangle bounds);

    }
}
