using System;
using Blish_HUD.Entities;
using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities {

    public enum BillboardVerticalConstraint {
        CameraPosition,
        PlayerPosition,
    }

    public class Marker : Entity {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #region Load Static

        private static readonly Effect _sharedMarkerEffect;

        static Marker() {
            _sharedMarkerEffect = BlishHud.ActiveContentManager.Load<Effect>(@"effects\marker");
        }

        #endregion

        private VertexPositionTexture[] _verts;

        private bool                        _autoResize = true;
        private Vector2                     _size       = Vector2.One;
        private float                       _scale      = 1f;
        private Texture2D                   _texture;
        private BillboardVerticalConstraint _verticalConstraint = BillboardVerticalConstraint.CameraPosition;
        private float                       _fadeNear           = -1;
        private float                       _fadeFar            = -1;

        /// <summary>
        /// If set to true, the <see cref="Size"/> will automatically
        /// update if a new <see cref="Texture"/> is set.
        /// </summary>
        public bool AutoResize {
            get => _autoResize;
            set => SetProperty(ref _autoResize, value);
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

        public float FadeNear {
            get => Math.Min(_fadeNear, _fadeFar);
            set => SetProperty(ref _fadeNear, value);
        }

        public float FadeFar {
            get => Math.Max(_fadeNear, _fadeFar);
            set => SetProperty(ref _fadeFar, value);
        }

        public Texture2D Texture {
            get => _texture;
            set {
                if (SetProperty(ref _texture, value) && _autoResize && _texture != null) {
                    this.Size = new Vector2(WorldUtil.GameToWorldCoord(_texture.Width),
                                            WorldUtil.GameToWorldCoord(_texture.Height));
                }
            }
        }

        private Effect _markerEffect;

        private VertexBuffer _vertexBuffer;

        public Marker() : this(null, Vector3.Zero, Vector2.Zero) { /* NOOP */ }

        public Marker(Texture2D image) : this(image, Vector3.Zero) { /* NOOP */ }

        public Marker(Texture2D image, Vector3 position) : this(image, position, Vector2.Zero) { /* NOOP */ }

        public Marker(Texture2D image, Vector3 position, Vector2 size) {
            Initialize();

            _autoResize = (size == Vector2.Zero);
            _size     = size;
            _texture  = image;
            _position = position;

            //this.VerticalConstraint = image.Height == image.Width
            //                              ? BillboardVerticalConstraint.PlayerPosition
            //                              : BillboardVerticalConstraint.PlayerPosition;

            //GameService.Input.MouseMoved += Input_MouseMoved;
        }

        private void Initialize() {
            _verts        = new VertexPositionTexture[4];
            _vertexBuffer = new VertexBuffer(GameService.Graphics.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);

            _markerEffect = _sharedMarkerEffect.Clone();
        }

        private void RecalculateSize(Vector2 newSize, float scale) {
            _verts[0] = new VertexPositionTexture(new Vector3(0,                 0,                 0),                    new Vector2(1, 1));
            _verts[1] = new VertexPositionTexture(new Vector3(newSize.X * scale, 0,                 0),                    new Vector2(0, 1));
            _verts[2] = new VertexPositionTexture(new Vector3(0,                 newSize.Y * scale, 0),                    new Vector2(1, 0));
            _verts[3] = new VertexPositionTexture(new Vector3(newSize.X                    * scale, newSize.Y * scale, 0), new Vector2(0, 0));

            _vertexBuffer.SetData(_verts);
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice) {
            RecalculateSize(_size, _scale);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            if (_texture == null) return;

            var modelMatrix = Matrix.CreateTranslation(_size.X / -2, _size.Y / -2, 0)
                            * Matrix.CreateScale(_scale);

            if (this.Rotation == Vector3.Zero) {
                modelMatrix *= Matrix.CreateBillboard(this.Position + this.RenderOffset,
                                                      new Vector3(GameService.Camera.Position.X,
                                                                  GameService.Camera.Position.Y,
                                                                  _verticalConstraint == BillboardVerticalConstraint.CameraPosition
                                                                      ? GameService.Camera.Position.Z
                                                                      : GameService.Player.Position.Z),
                                                      new Vector3(0, 0, 1),
                                                      GameService.Camera.Forward);
            } else {
                modelMatrix *= Matrix.CreateRotationX(this.Rotation.X)
                             * Matrix.CreateRotationY(this.Rotation.Y)
                             * Matrix.CreateRotationZ(this.Rotation.Z)
                             * Matrix.CreateTranslation(this.Position + this.RenderOffset);
            }

            _markerEffect.Parameters["PlayerPosition"].SetValue(GameService.Player.Position);
            _markerEffect.Parameters["Opacity"].SetValue(this.Opacity);
            _markerEffect.Parameters["FadeNear"].SetValue(_fadeNear);
            _markerEffect.Parameters["FadeFar"].SetValue(_fadeFar);
            _markerEffect.Parameters["Texture"].SetValue(_texture);
            _markerEffect.Parameters["World"].SetValue(modelMatrix);
            _markerEffect.Parameters["View"].SetValue(GameService.Camera.View);
            _markerEffect.Parameters["Projection"].SetValue(GameService.Camera.Projection);

            graphicsDevice.SetVertexBuffer(_vertexBuffer);

            foreach (var pass in _markerEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        private bool _mouseOver = false;

        private void Input_MouseMoved(object sender, MouseEventArgs e) {
            var screenPosition = GameService.Graphics.GraphicsDevice.Viewport.Project(this.Position, GameService.Camera.Projection, GameService.Camera.View, Matrix.Identity);

            float xdist = screenPosition.X - e.MouseState.Position.X;
            float ydist = screenPosition.Y - e.MouseState.Position.Y;
            
            // Z < 1 means that the point is in front of the camera, not behind it
            _mouseOver = screenPosition.Z < 1 && xdist < 2 && ydist < 2;
        }

    }
}
