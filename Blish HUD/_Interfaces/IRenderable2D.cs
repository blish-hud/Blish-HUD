using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {
    public interface IRenderable2D {

        void Draw(GraphicsDevice graphicsDevice, Rectangle bounds);

    }
}
