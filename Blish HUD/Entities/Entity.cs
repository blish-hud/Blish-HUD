using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Blish_HUD.Annotations;

namespace Blish_HUD.Entities {
    public abstract class Entity : INotifyPropertyChanged, IUpdatable, IRenderable3D {

        private static BasicEffect _standardEffect;
        private static BasicEffect StandardEffect =>
            _standardEffect ?? 
            (_standardEffect = new BasicEffect(GameService.Graphics.GraphicsDevice) { TextureEnabled = true });

        private Vector3 _position         = Vector3.Zero;
        private Vector3 _renderOffset     = Vector3.Zero;
        private float   _opacity          = 1.0f;
        private bool    _visible          = true;

        private Effect _entityEffect;
        public virtual Effect EntityEffect {
            get => _entityEffect ?? StandardEffect;
            set => _entityEffect = value;
        }

        public virtual Vector3 Position {
            get => _position;
            set => SetProperty(ref _position, value);
        }

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
                if (SetProperty(ref _renderOffset, new Vector3(_renderOffset.X, _renderOffset.Y, value), nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float HorizontalOffset {
            get => _renderOffset.X;
            set {
                if (SetProperty(ref _renderOffset, new Vector3(value, _renderOffset.Y, _renderOffset.Z), nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float DepthOffset {
            get => _renderOffset.Y;
            set {
                if (SetProperty(ref _renderOffset, new Vector3(_renderOffset.X, value, _renderOffset.Y), nameof(this.RenderOffset)))
                    OnPropertyChanged();
            }
        }

        public virtual float Opacity {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        // TODO: Consider calling 'OnPropertyChanged' for 'DistanceFromPlayer' and 'DistanceFromCamera' somehow reasonable
        public virtual float DistanceFromPlayer => Vector3.Distance(this.Position, GameService.Player.Position);
        public virtual float DistanceFromCamera => Vector3.Distance(this.Position, GameService.Camera.Position);

        public virtual bool Visible {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

        public virtual void Draw(GraphicsDevice graphicsDevice) {
            if (this.EntityEffect != StandardEffect) return;

            StandardEffect.View = GameService.Camera.View;
            StandardEffect.Projection = GameService.Camera.Projection;

            StandardEffect.Alpha = this.Opacity;
        }

        #region Property Management and Binding

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null) {
            if (Equals(property, newValue) || propertyName == null) return false;

            property = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

}
