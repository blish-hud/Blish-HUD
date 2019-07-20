using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public abstract class Cuboid:Entity {

        protected VertexBuffer _geometryBuffer;

        protected Vector3 _size;

        public Vector3 Size {
            get => _size;
            set => SetProperty(ref _size, value, true);
        }

        private Texture2D _texture;

        public Texture2D Texture {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

        /// <inheritdoc />
        public Cuboid() : this(Vector3.One) { /* NOOP */ }

        public Cuboid(float size) : this(new Vector3(size)) { /* NOOP */ }

        public Cuboid(Vector3 size) : base() {
            this.Texture = ContentService.Textures.Error;

            _size = size;
        }

        private void GenerateCuboid(GraphicsDevice device, Vector3 size) {
            VertexPositionTexture[] verts = new VertexPositionTexture[24];

            verts[0].Position = new Vector3(-1, 1, -1) * size;
            verts[0].TextureCoordinate = new Vector2(0, 0);
            verts[1].Position = new Vector3(1, 1, -1) * size;
            verts[1].TextureCoordinate = new Vector2(1, 0);
            verts[2].Position = new Vector3(-1, 1, 1) * size;
            verts[2].TextureCoordinate = new Vector2(0, 1);
            verts[3].Position = new Vector3(1, 1, 1) * size;
            verts[3].TextureCoordinate = new Vector2(1, 1);

            verts[4].Position = new Vector3(-1, -1, 1) * size;
            verts[4].TextureCoordinate = new Vector2(0, 0);
            verts[5].Position = new Vector3(1, -1, 1) * size;
            verts[5].TextureCoordinate = new Vector2(1, 0);
            verts[6].Position = new Vector3(-1, -1, -1) * size;
            verts[6].TextureCoordinate = new Vector2(0, 1);
            verts[7].Position = new Vector3(1, -1, -1) * size;
            verts[7].TextureCoordinate = new Vector2(1, 1);

            verts[8].Position = new Vector3(-1, 1, -1) * size;
            verts[8].TextureCoordinate = new Vector2(0, 0);
            verts[9].Position = new Vector3(-1, 1, 1) * size;
            verts[9].TextureCoordinate = new Vector2(1, 0);
            verts[10].Position = new Vector3(-1, -1, -1) * size;
            verts[10].TextureCoordinate = new Vector2(0, 1);
            verts[11].Position = new Vector3(-1, -1, 1) * size;
            verts[11].TextureCoordinate = new Vector2(1, 1);

            verts[12].Position = new Vector3(-1, 1, 1) * size;
            verts[12].TextureCoordinate = new Vector2(0, 0);
            verts[13].Position = new Vector3(1, 1, 1) * size;
            verts[13].TextureCoordinate = new Vector2(1, 0);
            verts[14].Position = new Vector3(-1, -1, 1) * size;
            verts[14].TextureCoordinate = new Vector2(0, 1);
            verts[15].Position = new Vector3(1, -1, 1) * size;
            verts[15].TextureCoordinate = new Vector2(1, 1);

            verts[16].Position = new Vector3(1, 1, 1) * size;
            verts[16].TextureCoordinate = new Vector2(0, 0);
            verts[17].Position = new Vector3(1, 1, -1) * size;
            verts[17].TextureCoordinate = new Vector2(1, 0);
            verts[18].Position = new Vector3(1, -1, 1) * size;
            verts[18].TextureCoordinate = new Vector2(0, 1);
            verts[19].Position = new Vector3(1, -1, -1) * size;
            verts[19].TextureCoordinate = new Vector2(1, 1);

            verts[20].Position = new Vector3(1, 1, -1) * size;
            verts[20].TextureCoordinate = new Vector2(0, 0);
            verts[21].Position = new Vector3(-1, 1, -1) * size;
            verts[21].TextureCoordinate = new Vector2(1, 0);
            verts[22].Position = new Vector3(1, -1, -1) * size;
            verts[22].TextureCoordinate = new Vector2(0, 1);
            verts[23].Position = new Vector3(-1, -1, -1) * size;
            verts[23].TextureCoordinate = new Vector2(1, 1);

            _geometryBuffer?.Dispose();
            _geometryBuffer = new VertexBuffer(GameService.Graphics.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 24, BufferUsage.WriteOnly);
            _geometryBuffer.SetData(verts);
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice) {
            GenerateCuboid(graphicsDevice, _size);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            if (_geometryBuffer == null) return;

            StandardEffect.View       = GameService.Camera.View;
            StandardEffect.Projection = GameService.Camera.Projection;
            StandardEffect.World      = Matrix.CreateTranslation(_position);
            StandardEffect.Alpha      = _opacity;

            graphicsDevice.SetVertexBuffer(_geometryBuffer, 0);

            foreach (var pass in StandardEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }

            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 24);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing) {
            if (!_disposed && disposing) {
                Texture = null;
                _geometryBuffer.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}
