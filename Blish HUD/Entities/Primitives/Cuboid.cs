using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public abstract class Cuboid:Entity {

        private VertexPositionNormalTexture[] _verts;

        private Vector3 _size = Vector3.One;
        public Vector3 Size { get { return _size; } set { _size = value; UpdateSize(); } }

        public Texture2D Texture { get; set; }

        private static BasicEffect basicEffect;

        public Cuboid() : base() {
            _verts = new VertexPositionNormalTexture[36];

            basicEffect = basicEffect ?? new BasicEffect(GameService.Graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.EnableDefaultLighting();

            UpdateSize();
        }

        private void UpdateSize() {
            // Calculate the position of the vertices on the top face.
            var topLeftFront = this.Position + new Vector3(-1.0f, 1.0f, -1.0f) * this.Size;
            var topLeftBack = this.Position + new Vector3(-1.0f, 1.0f, 1.0f) * this.Size;
            var topRightFront = this.Position + new Vector3(1.0f, 1.0f, -1.0f) * this.Size;
            var topRightBack = this.Position + new Vector3(1.0f, 1.0f, 1.0f) * this.Size;

            // Calculate the position of the vertices on the bottom face.
            var btmLeftFront = this.Position + new Vector3(-1.0f, -1.0f, -1.0f) * this.Size;
            var btmLeftBack = this.Position + new Vector3(-1.0f, -1.0f, 1.0f) * this.Size;
            var btmRightFront = this.Position + new Vector3(1.0f, -1.0f, -1.0f) * this.Size;
            var btmRightBack = this.Position + new Vector3(1.0f, -1.0f, 1.0f) * this.Size;

            // Normal vectors for each face (needed for lighting / display)
            var normalFront = new Vector3(0.0f, 0.0f, 1.0f) * this.Size;
            var normalBack = new Vector3(0.0f, 0.0f, -1.0f) * this.Size;
            var normalTop = new Vector3(0.0f, 1.0f, 0.0f) * this.Size;
            var normalBottom = new Vector3(0.0f, -1.0f, 0.0f) * this.Size;
            var normalLeft = new Vector3(-1.0f, 0.0f, 0.0f) * this.Size;
            var normalRight = new Vector3(1.0f, 0.0f, 0.0f) * this.Size;

            // UV texture coordinates
            //Vector2 textureTopLeft = new Vector2(1.0f * Size.X, 0.0f * Size.Y);
            //Vector2 textureTopRight = new Vector2(0.0f * Size.X, 0.0f * Size.Y);
            //Vector2 textureBottomLeft = new Vector2(1.0f * Size.X, 1.0f * Size.Y);
            //Vector2 textureBottomRight = new Vector2(0.0f * Size.X, 1.0f * Size.Y);
            var textureTopLeft = new Vector2(1.0f, 0.0f);
            var textureTopRight = new Vector2(0.0f, 0.0f);
            var textureBottomLeft = new Vector2(1.0f, 1.0f);
            var textureBottomRight = new Vector2(0.0f, 1.0f);

            // Add the vertices for the FRONT face.
            _verts[0] = new VertexPositionNormalTexture(topLeftFront, normalFront, textureTopLeft);
            _verts[1] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            _verts[2] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);
            _verts[3] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            _verts[4] = new VertexPositionNormalTexture(btmRightFront, normalFront, textureBottomRight);
            _verts[5] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);

            // Add the vertices for the BACK face.
            _verts[6] = new VertexPositionNormalTexture(topLeftBack, normalBack, textureTopRight);
            _verts[7] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            _verts[8] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            _verts[9] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            _verts[10] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            _verts[11] = new VertexPositionNormalTexture(btmRightBack, normalBack, textureBottomLeft);

            // Add the vertices for the TOP face.
            _verts[12] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            _verts[13] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);
            _verts[14] = new VertexPositionNormalTexture(topLeftBack, normalTop, textureTopLeft);
            _verts[15] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            _verts[16] = new VertexPositionNormalTexture(topRightFront, normalTop, textureBottomRight);
            _verts[17] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);

            // Add the vertices for the BOTTOM face. 
            _verts[18] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            _verts[19] = new VertexPositionNormalTexture(btmLeftBack, normalBottom, textureBottomLeft);
            _verts[20] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            _verts[21] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            _verts[22] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            _verts[23] = new VertexPositionNormalTexture(btmRightFront, normalBottom, textureTopRight);

            // Add the vertices for the LEFT face.
            _verts[24] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);
            _verts[25] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            _verts[26] = new VertexPositionNormalTexture(btmLeftFront, normalLeft, textureBottomRight);
            _verts[27] = new VertexPositionNormalTexture(topLeftBack, normalLeft, textureTopLeft);
            _verts[28] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            _verts[29] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);

            // Add the vertices for the RIGHT face. 
            _verts[30] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            _verts[31] = new VertexPositionNormalTexture(btmRightFront, normalRight, textureBottomLeft);
            _verts[32] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
            _verts[33] = new VertexPositionNormalTexture(topRightBack, normalRight, textureTopRight);
            _verts[34] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            _verts[35] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            basicEffect.View = GameService.Camera.View;
            basicEffect.Projection = GameService.Camera.Projection;
            basicEffect.World = Matrix.CreateTranslation(this.Position);

            basicEffect.Texture = this.Texture;

            basicEffect.Alpha = this.Opacity;

            foreach (var pass in basicEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _verts, 0, 12);
            }
        }

        public override void Update(GameTime gameTime) {
        }
    }
}
