using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Blish_HUD.Graphics;

namespace Blish_HUD.Entities {
    public abstract class Entity : INotifyPropertyChanged, IEntity {

        protected static BasicEffect StandardEffect { get; } = new BasicEffect(BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice) { TextureEnabled = true };

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
            set => SetProperty(ref _rotation, value);
        }

        public float RotationX {
            get => _rotation.X;
            set {
                if (SetProperty(ref _rotation, new Vector3(value, _rotation.Y, _rotation.Z), false, nameof(Rotation)))
                    OnPropertyChanged();
            }
        }

        public float RotationY {
            get => _rotation.Y;
            set {
                if (SetProperty(ref _rotation, new Vector3(_rotation.X, value, _rotation.Z), false, nameof(Rotation)))
                    OnPropertyChanged();
            }
        }

        public float RotationZ {
            get => _rotation.Z;
            set {
                if (SetProperty(ref _rotation, new Vector3(_rotation.X, _rotation.Y, value), false, nameof(Rotation)))
                    OnPropertyChanged();
            }
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
            set => SetProperty(ref _renderOffset, value);
        }

        public virtual float VerticalOffset {
            get => _renderOffset.Z;
            set {
                if (SetProperty(ref _renderOffset, new Vector3(_renderOffset.X, _renderOffset.Y, value), false, nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float HorizontalOffset {
            get => _renderOffset.X;
            set {
                if (SetProperty(ref _renderOffset, new Vector3(value, _renderOffset.Y, _renderOffset.Z), false, nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float DepthOffset {
            get => _renderOffset.Y;
            set {
                if (SetProperty(ref _renderOffset, new Vector3(_renderOffset.X, value, _renderOffset.Y), false, nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
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
                _basicTitleTextBillboard ??= BuildTitleText();
                _basicTitleTextBillboard.Text = value;
            }
        }

        public Color BasicTitleTextColor {
            get => _basicTitleTextBillboard?.TextColor ?? Color.White;
            set {
                _basicTitleTextBillboard ??= BuildTitleText();
                _basicTitleTextBillboard.TextColor = value;
            }
        }

        public virtual float DistanceFromPlayer => Vector3.Distance(this.Position, GameService.Gw2Mumble.PlayerCharacter.Position);
        public virtual float DistanceFromCamera => Vector3.Distance(this.Position, GameService.Gw2Mumble.PlayerCamera.Position);

        public float DrawOrder => Vector3.DistanceSquared(this.Position, GameService.Gw2Mumble.PlayerCamera.Position);

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

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            DoDraw(graphicsDevice);
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

    }

}
