using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Blish_HUD.Entities {
    public abstract class Entity : INotifyPropertyChanged, IUpdatable, IRenderable3D, IDisposable {

        #region Load Static

        protected static readonly BasicEffect StandardEffect;

        static Entity() {
            StandardEffect = new BasicEffect(BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice) {
                TextureEnabled = true
            };
        }

        #endregion

        protected Vector3 _position     = Vector3.Zero;
        protected Vector3 _rotation     = Vector3.Zero;
        protected Vector3 _renderOffset = Vector3.Zero;
        protected float   _opacity      = 1.0f;
        protected bool    _visible      = true;

        private bool _pendingRebuild = true;

        public virtual Vector3 Position {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public virtual Vector3 Rotation {
            get => _rotation;
            set {
                var previousRotation = _rotation;

                if (SetProperty(ref _rotation, value)) {
                    // We do this to make sure we raise PropertyChanged events for alias properties
                    if (previousRotation.X != _rotation.X)
                        OnPropertyChanged(nameof(this.RotationX));
                    if (previousRotation.Y != _rotation.Y)
                        OnPropertyChanged(nameof(this.RotationY));
                    if (previousRotation.Z != _rotation.Z)
                        OnPropertyChanged(nameof(this.RotationZ));
                }
            }
        }

        public float RotationX {
            get => _rotation.X;
            set => this.Rotation = new Vector3(value, _rotation.Y, _rotation.Z);
        }

        public float RotationY {
            get => _rotation.Y;
            set => this.Rotation = new Vector3(_rotation.X, value, _rotation.Z);
        }

        public float RotationZ {
            get => _rotation.Z;
            set => this.Rotation = new Vector3(_rotation.X, _rotation.Y, value);
        }

        /// <summary>
        /// If <c>true</c>, the <see cref="Entity"/> will rebuild its <see cref="VertexBuffer"/> during the next update cycle.
        /// </summary>
        public bool PendingRebuild => _pendingRebuild;

        /// <summary>
        /// The offset that this entity is rendered from its origin.
        /// </summary>
        public virtual Vector3 RenderOffset {
            get => _renderOffset;
            set {
                var previousRenderOffset = _renderOffset;

                if (SetProperty(ref _renderOffset, value)) {
                    // We do this to make sure we raise PropertyChanged events for alias properties
                    if (previousRenderOffset.X != _renderOffset.X)
                        OnPropertyChanged(nameof(this.HorizontalOffset));
                    if (previousRenderOffset.Y != _renderOffset.Y)
                        OnPropertyChanged(nameof(this.DepthOffset));
                    if (previousRenderOffset.Z != _renderOffset.Z)
                        OnPropertyChanged(nameof(this.VerticalOffset));
                }
            }
        }

        public virtual float HorizontalOffset {
            get => _renderOffset.X;
            set => this.RenderOffset = new Vector3(value, _renderOffset.Y, _renderOffset.Z);
        }

        public virtual float DepthOffset {
            get => _renderOffset.Y;
            set => this.RenderOffset = new Vector3(_renderOffset.X, value, _renderOffset.Z);
        }

        public virtual float VerticalOffset {
            get => _renderOffset.Z;
            set => this.RenderOffset = new Vector3(_renderOffset.X, _renderOffset.Y, value);
        }

        public virtual float Opacity {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public virtual bool Visible {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        private EntityBillboard _billboard;
        public EntityBillboard Billboard {
            get => _billboard ?? _basicTitleTextBillboard;
            set => SetProperty(ref _billboard, value);
        }

        private EntityText _basicTitleTextBillboard;
        public string BasicTitleText {
            get => _basicTitleTextBillboard?.Text ?? string.Empty;
            set {
                _basicTitleTextBillboard      = _basicTitleTextBillboard ?? BuildTitleText();
                _basicTitleTextBillboard.Text = value;
            }
        }

        public Color BasicTitleTextColor {
            get => _basicTitleTextBillboard?.TextColor ?? Color.White;
            set {
                _basicTitleTextBillboard           = _basicTitleTextBillboard ?? BuildTitleText();
                _basicTitleTextBillboard.TextColor = value;
            }
        }

        public virtual float DistanceFromPlayer => Vector3.Distance(this.Position, GameService.Player.Position);
        public virtual float DistanceFromCamera => Vector3.Distance(this.Position, GameService.Camera.Position);

        private EntityText BuildTitleText() {
            var entityText = new EntityText(this) {
                VerticalOffset = 2f
            };

            return entityText;
        }

        public virtual void DoUpdate(GameTime gameTime) {
            if (_pendingRebuild) {
                HandleRebuild(GameService.Graphics.GraphicsDevice);
                _pendingRebuild = false;
            }

            Update(gameTime);

            this.Billboard?.DoUpdate(gameTime);
        }

        public virtual void DoDraw(GraphicsDevice graphicsDevice) {
            Draw(graphicsDevice);

            this.Billboard?.DoDraw(graphicsDevice);
        }

        public abstract void HandleRebuild(GraphicsDevice graphicsDevice);

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

        public virtual void Draw(GraphicsDevice graphicsDevice) { /* NOOP */ }

        #region Property Management and Binding

        protected bool SetProperty<T>(ref T property, T newValue, bool rebuildEntity = false, [CallerMemberName] string propertyName = null) {
            if (Equals(property, newValue) || propertyName == null) return false;

            property = newValue;

            _pendingRebuild = _pendingRebuild || rebuildEntity;

            OnPropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDispose

        private protected bool _disposed;

        /// <summary>
        /// Indicates that <see cref="Dispose"/> has been called on this <see cref="Entity"/> instance.
        /// </summary>
        public bool Disposed => _disposed;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed && disposing) {
                _billboard?.Dispose();
                _basicTitleTextBillboard?.Dispose();
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
