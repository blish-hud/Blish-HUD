using Blish_HUD.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Graphics {
    public interface IRenderable3D {

        void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera);

    }
}
