using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Annotations;

namespace Blish_HUD.Entities {
    public abstract class Entity : INotifyPropertyChanged {

        private static BasicEffect _standardEffect;
        private static BasicEffect StandardEffect {
            get {
                // Lazy load basic effect for all entities
                return _standardEffect ?? (_standardEffect = new BasicEffect(GameService.Graphics.GraphicsDevice) {
                        TextureEnabled = true
                    });

                return _standardEffect;
            }
        }

        private Effect _entityEffect;
        public Effect EntityEffect {
            get => _entityEffect ?? StandardEffect;
            set => _entityEffect = value;
        }

        public Vector3 Position { get; set; } = Vector3.Zero;
        
        //public float FadeNear { get; set; } = 0;
        //public float FadeFar { get; set; } = 0;
        
        public float Opacity { get; set; } = 1.0f;

        public float DistanceFromPlayer => Vector3.Distance(this.Position, GameService.Player.Position);
        public float DistanceFromCamera => Vector3.Distance(this.Position, GameService.Camera.Position);

        private bool _visible = true;
        public bool Visible {
            get => _visible;
            set {
                if (_visible == value) return;

                _visible = value;
                OnPropertyChanged();
            }
        }

        protected Entity() {
        }

        public abstract void Update(GameTime gameTime);
        public void DoUpdate(GameTime gameTime) {
            Update(gameTime);
        }

        public abstract void Draw(GraphicsDevice graphicsDevice);
        public void DoDraw(GraphicsDevice graphicsDevice) {
            if (this.EntityEffect == StandardEffect) {
                StandardEffect.View = GameService.Camera.View;
                StandardEffect.Projection = GameService.Camera.Projection;

                StandardEffect.Alpha = this.Opacity;
            }

            Draw(graphicsDevice);
        }

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
