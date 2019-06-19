using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {
    public class Cube : Primitives.Cuboid {

        public Color Color { get; set; }

        public Cube() : base() {
            this.Texture = ContentService.Textures.Pixel;
            this.Size = new Vector3(0.25f, 0.25f, 0.25f);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            ((BasicEffect) this.EntityEffect).EmissiveColor = this.Color.ToVector3();

            base.Draw(graphicsDevice);
        }

    }
}
