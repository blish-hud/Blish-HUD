using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public abstract class Cuboid:Entity {

        protected VertexBuffer _geometryBuffer;
        protected IndexBuffer  _indexBuffer;

        protected Vector3 _size;

        public Vector3 Size {
            get => _size;
            set => SetProperty(ref _size, value, true);
        }

        public Texture2D Texture {
            get => _renderEffect.Texture;
            set => _renderEffect.Texture = value;
        }

        /// <inheritdoc />
        public override float Opacity {
            get => _renderEffect.Alpha;
            set => _renderEffect.Alpha = value;
        }

        protected readonly VertexPositionTexture[] _verts;
        protected readonly BasicEffect             _renderEffect;

        public Cuboid() : this(new Vector3(1f)) { /* NOOP */ }

        public Cuboid(float size) : this(new Vector3(size)) { /* NOOP */ }

        public Cuboid(Vector3 size) : base() {
            _verts = new VertexPositionTexture[24];

            _renderEffect = StandardEffect;
            _renderEffect.TextureEnabled = true;
            _renderEffect.VertexColorEnabled = false;

            this.Texture = ContentService.Textures.Error;

            _size = size;
        }

        private void GenerateCuboid(GraphicsDevice device, Vector3 size) {
            _verts[0].Position = new Vector3(-1, 1, -1) * size;
            _verts[0].TextureCoordinate = new Vector2(0, 0);
            _verts[1].Position = new Vector3(1, 1, -1) * size;
            _verts[1].TextureCoordinate = new Vector2(1, 0);
            _verts[2].Position = new Vector3(-1, 1, 1) * size;
            _verts[2].TextureCoordinate = new Vector2(0, 1);
            _verts[3].Position = new Vector3(1, 1, 1) * size;
            _verts[3].TextureCoordinate = new Vector2(1, 1);

            _verts[4].Position = new Vector3(-1, -1, 1) * size;
            _verts[4].TextureCoordinate = new Vector2(0, 0);
            _verts[5].Position = new Vector3(1, -1, 1) * size;
            _verts[5].TextureCoordinate = new Vector2(1, 0);
            _verts[6].Position = new Vector3(-1, -1, -1) * size;
            _verts[6].TextureCoordinate = new Vector2(0, 1);
            _verts[7].Position = new Vector3(1, -1, -1) * size;
            _verts[7].TextureCoordinate = new Vector2(1, 1);

            _verts[8].Position = new Vector3(-1, 1, -1) * size;
            _verts[8].TextureCoordinate = new Vector2(0, 0);
            _verts[9].Position = new Vector3(-1, 1, 1) * size;
            _verts[9].TextureCoordinate = new Vector2(1, 0);
            _verts[10].Position = new Vector3(-1, -1, -1) * size;
            _verts[10].TextureCoordinate = new Vector2(0, 1);
            _verts[11].Position = new Vector3(-1, -1, 1) * size;
            _verts[11].TextureCoordinate = new Vector2(1, 1);

            _verts[12].Position = new Vector3(-1, 1, 1) * size;
            _verts[12].TextureCoordinate = new Vector2(0, 0);
            _verts[13].Position = new Vector3(1, 1, 1) * size;
            _verts[13].TextureCoordinate = new Vector2(1, 0);
            _verts[14].Position = new Vector3(-1, -1, 1) * size;
            _verts[14].TextureCoordinate = new Vector2(0, 1);
            _verts[15].Position = new Vector3(1, -1, 1) * size;
            _verts[15].TextureCoordinate = new Vector2(1, 1);

            _verts[16].Position = new Vector3(1, 1, 1) * size;
            _verts[16].TextureCoordinate = new Vector2(0, 0);
            _verts[17].Position = new Vector3(1, 1, -1) * size;
            _verts[17].TextureCoordinate = new Vector2(1, 0);
            _verts[18].Position = new Vector3(1, -1, 1) * size;
            _verts[18].TextureCoordinate = new Vector2(0, 1);
            _verts[19].Position = new Vector3(1, -1, -1) * size;
            _verts[19].TextureCoordinate = new Vector2(1, 1);

            _verts[20].Position = new Vector3(1, 1, -1) * size;
            _verts[20].TextureCoordinate = new Vector2(0, 0);
            _verts[21].Position = new Vector3(-1, 1, -1) * size;
            _verts[21].TextureCoordinate = new Vector2(1, 0);
            _verts[22].Position = new Vector3(1, -1, -1) * size;
            _verts[22].TextureCoordinate = new Vector2(0, 1);
            _verts[23].Position = new Vector3(-1, -1, -1) * size;
            _verts[23].TextureCoordinate = new Vector2(1, 1);

            _geometryBuffer?.Dispose();
            _geometryBuffer = new VertexBuffer(GameService.Graphics.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 24, BufferUsage.WriteOnly);
            _geometryBuffer.SetData(_verts);

            //var indices = new int[36];
            //indices[0] = 0; indices[1] = 1; indices[2] = 2;
            //indices[3] = 1; indices[4] = 3; indices[5] = 2;

            //indices[6] = 4; indices[7]  = 5; indices[8]  = 6;
            //indices[9] = 5; indices[10] = 7; indices[11] = 6;

            //indices[12] = 8; indices[13] = 9; indices[14]  = 10;
            //indices[15] = 9; indices[16] = 11; indices[17] = 10;

            //indices[18] = 12; indices[19] = 13; indices[20] = 14;
            //indices[21] = 13; indices[22] = 15; indices[23] = 14;

            //indices[24] = 16; indices[25] = 17; indices[26] = 18;
            //indices[27] = 17; indices[28] = 19; indices[29] = 18;

            //indices[30] = 20; indices[31] = 21; indices[32] = 22;
            //indices[33] = 21; indices[34] = 23; indices[35] = 22;

            //_indexBuffer = new IndexBuffer(device, typeof(int), 36, BufferUsage.WriteOnly);
            //_indexBuffer.SetData(indices);
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice) {
            GenerateCuboid(graphicsDevice, _size);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            if (_geometryBuffer == null) return;

            _renderEffect.View       = GameService.Gw2Mumble.PlayerCamera.View;
            _renderEffect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
            _renderEffect.World      = Matrix.CreateTranslation(_position);

            graphicsDevice.SetVertexBuffer(_geometryBuffer, 0);

            foreach (var pass in _renderEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }

            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 24);
        }

    }
}
