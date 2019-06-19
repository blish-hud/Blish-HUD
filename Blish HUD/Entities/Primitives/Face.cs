using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public class Face:Entity {

        private VertexPositionTexture[] _verts;

        private Vector2 _size = Vector2.One;
        public Vector2 Size { get { return _size; } set { _size = value; UpdateSize(); } }

        public Texture2D Texture { get; set; }

        private static BasicEffect basicEffect;

        public override void Update(GameTime gameTime) {
            // NOOP
        }

        private void UpdateSize() {
            var _3DSize = new Vector3(this.Size.X, this.Size.Y, 0);

            _verts[0] = new VertexPositionTexture(Vector3.UnitX * _3DSize, Vector2.One);
            _verts[1] = new VertexPositionTexture(Vector3.Zero, Vector2.UnitY);
            _verts[2] = new VertexPositionTexture(new Vector3(1f, 1f, 0) * _3DSize, Vector2.UnitX);
            _verts[3] = new VertexPositionTexture(Vector3.UnitY * _3DSize, Vector2.Zero);
        }
        
        private void Initialize() {
            _verts = new VertexPositionTexture[4];

            basicEffect = basicEffect ?? new BasicEffect(GameService.Graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
        }

        public Face() :
            this(null, Vector3.Zero, Vector2.One) { }

        public Face(Texture2D image) :
            this(image, Vector3.Zero) { }

        public Face(Texture2D image, Vector3 position) :
            this(image, position, new Vector2(image.Width / 100f, image.Height / 100f)) { }

        public Face(Texture2D image, Vector3 position, Vector2 size) : base() {
            Initialize();

            this.Texture = image;
            this.Size = size;
            this.Position = position;

            //Services.Services.Input.MouseMoved += Input_MouseMoved;
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            basicEffect.View = GameService.Camera.View;
            basicEffect.Projection = GameService.Camera.Projection;
            basicEffect.World = Matrix.CreateTranslation(this.Position + new Vector3(this.Size.X / -2, this.Size.Y / -2, 0));

            basicEffect.Texture = this.Texture;

            basicEffect.Alpha = this.Opacity;

            foreach (var pass in basicEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _verts, 0, 2);
            }
        }

    }
}
