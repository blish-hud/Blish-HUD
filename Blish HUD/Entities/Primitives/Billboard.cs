using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Entities.Primitives {

    public enum BillboardConstraint {
        None,
        XAxis,
        YAxis,
        ZAxis
    }

    public class Billboard : Entity {

        private VertexPositionTexture[] _verts;
        
        public Texture2D Texture { get; set; }

        private Vector2 _size = Vector2.One;
        public Vector2 Size {
            get => _size;
            set {
                _size = value;
                UpdateSize(value);

                OnPropertyChanged();
            }
        }

        private static BasicEffect BillboardEffect;

        public Billboard() :
            this(null, Vector3.Zero, Vector2.One) { }

        public Billboard(Texture2D image) :
            this(image, Vector3.Zero) { }

        public Billboard(Texture2D image, Vector3 position) :
            this(image, position, new Vector2(image.Width / 100f, image.Height / 100f)) { }

        public Billboard(Texture2D image, Vector3 position, Vector2 size) : base() {
            Initialize();

            this.Texture = image;
            this.Size = size;
            this.Position = position;
        }

        private void Initialize() {
            _verts = new VertexPositionTexture[4];

            BillboardEffect = BillboardEffect ?? new BasicEffect(GameService.Graphics.GraphicsDevice);
            BillboardEffect.TextureEnabled = true;
        }

        private void UpdateSize(Vector2 newSize) {
            _verts[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1));
            _verts[1] = new VertexPositionTexture(new Vector3(newSize.X, 0, 0), new Vector2(0, 1));
            _verts[2] = new VertexPositionTexture(new Vector3(0, newSize.Y, 0), new Vector2(1, 0));
            _verts[3] = new VertexPositionTexture(new Vector3(newSize.X, newSize.Y, 0), new Vector2(0, 0));
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            BillboardEffect.View = GameService.Camera.View;
            BillboardEffect.Projection = GameService.Camera.Projection;
            BillboardEffect.World = Matrix.CreateTranslation(new Vector3(this.Size.X / -2, this.Size.Y / -2, 0)) * Matrix.CreateBillboard(this.Position, GameService.Camera.Position, new Vector3(0, 0, 1), GameService.Camera.Forward); 

            BillboardEffect.Alpha = this.Opacity;
            BillboardEffect.Texture = this.Texture;

            foreach (var pass in BillboardEffect.CurrentTechnique.Passes) {
                pass.Apply();

                if (this.DistanceFromPlayer < 10) {
                    //Console.WriteLine(this.Position + " vs. " + BillboardEffect.World.Translation);
                }
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _verts, 0, 2);
            }
        }

        public override void Update(GameTime gameTime) {
            // NOOP
        }
    }
}
