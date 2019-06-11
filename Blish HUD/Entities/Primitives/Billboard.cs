using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Pathing.Entities;

namespace Blish_HUD.Entities.Primitives {

    public class Billboard : Entity {

        private VertexPositionTexture[] _verts;

        private bool                        _autoResizeBillboard = true;
        private Vector2                     _size                = Vector2.One;
        private float                       _scale               = 1f;
        private Texture2D                   _texture;
        private BillboardVerticalConstraint _verticalConstraint = BillboardVerticalConstraint.CameraPosition;

        /// <summary>
        /// If set to true, the <see cref="Size"/> will automatically
        /// update if a new <see cref="Texture"/> is set.
        /// </summary>
        public bool AutoResizeBillboard {
            get => _autoResizeBillboard;
            set => SetProperty(ref _autoResizeBillboard, value);
        }

        public BillboardVerticalConstraint VerticalConstraint {
            get => _verticalConstraint;
            set => SetProperty(ref _verticalConstraint, value);
        }

        public Vector2 Size {
            get => _size;
            set {
                if (SetProperty(ref _size, value))
                    RecalculateSize(_size, _scale);
            }
        }

        /// <summary>
        /// Scales the render size of the <see cref="Billboard"/>.
        /// </summary>
        public float Scale {
            get => _scale;
            set {
                if (SetProperty(ref _scale, value))
                    RecalculateSize(_size, _scale);
            }
        }

        public Texture2D Texture {
            get => _texture;
            set {
                if (SetProperty(ref _texture, value) && _autoResizeBillboard && _texture != null)
                    this.Size = new Vector2(Utils.World.GameToWorldCoord(_texture.Width),
                                            Utils.World.GameToWorldCoord(_texture.Height));
            }
        }

        private static BasicEffect _billboardEffect;

        public Billboard() :
            this(null, Vector3.Zero, Vector2.Zero) { }

        public Billboard(Texture2D image) :
            this(image, Vector3.Zero) { }

        public Billboard(Texture2D image, Vector3 position) :
            this(image, position, Vector2.Zero) { }

        public Billboard(Texture2D image, Vector3 position, Vector2 size) : base() {
            Initialize();

            this.AutoResizeBillboard = (size == Vector2.Zero);
            this.Size = size;
            this.Texture = image;
            this.Position = position;
        }

        private void Initialize() {
            _verts = new VertexPositionTexture[4];

            _billboardEffect = _billboardEffect ?? new BasicEffect(GameService.Graphics.GraphicsDevice);
            _billboardEffect.TextureEnabled = true;
        }

        private void RecalculateSize(Vector2 newSize, float scale) {
            _verts[0] = new VertexPositionTexture(new Vector3(0,                 0,                 0),                    new Vector2(1, 1));
            _verts[1] = new VertexPositionTexture(new Vector3(newSize.X * scale, 0,                 0),                    new Vector2(0, 1));
            _verts[2] = new VertexPositionTexture(new Vector3(0,                 newSize.Y * scale, 0),                    new Vector2(1, 0));
            _verts[3] = new VertexPositionTexture(new Vector3(newSize.X                    * scale, newSize.Y * scale, 0), new Vector2(0, 0));
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            if (this.Texture == null) return;

            _billboardEffect.View = GameService.Camera.View;
            _billboardEffect.Projection = GameService.Camera.Projection;
            _billboardEffect.World = Matrix.CreateTranslation(new Vector3(this.Size.X / -2, this.Size.Y / -2, 0))
                                   * Matrix.CreateScale(_scale, _scale, 1)
                                   * Matrix.CreateBillboard(this.Position + this.RenderOffset,
                                                            new Vector3(GameService.Camera.Position.X,
                                                                        GameService.Camera.Position.Y,
                                                                        _verticalConstraint == BillboardVerticalConstraint.CameraPosition
                                                                            ? GameService.Camera.Position.Z
                                                                            : GameService.Player.Position.Z),
                                                            new Vector3(0, 0, 1),
                                                            GameService.Camera.Forward);



            _billboardEffect.Alpha = this.Opacity;
            _billboardEffect.Texture = this.Texture;

            foreach (var pass in _billboardEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _verts, 0, 2);
            }
        }

        public override void Update(GameTime gameTime) {
            // NOOP
        }
    }
}
