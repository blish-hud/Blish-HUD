using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls.Effects {
    public abstract class ControlEffect {
        
        protected Control AssignedControl { get; }


        protected Vector2? _size;
        /// <summary>
        /// The size within the <see cref="Control"/> it applies to.  If not explicitly set, the size of the assigned control will be used.
        /// </summary>
        public Vector2 Size {
            get => _size ?? AssignedControl.Size.ToVector2();
            set => _size = value;
        }

        protected Vector2? _location;
        /// <summary>
        /// The location relative to the <see cref="Control"/> it applies to.  If not explicitly set, <see cref="Vector2.Zero"/> will be used.
        /// </summary>
        public Vector2 Location {
            get => _location ?? Vector2.Zero;
            set => _location = value;
        }

        protected bool _enabled = true;
        /// <summary>
        /// If the <see cref="ControlEffect"/> should render or not.
        /// </summary>
        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;

                _enabled = value;

                if (_enabled)
                    OnEnable();
                else
                    OnDisable();
            }
        }

        protected ControlEffect(Control assignedControl) {
            this.AssignedControl = assignedControl;
        }

        protected abstract SpriteBatchParameters GetSpriteBatchParameters();

        protected virtual void OnEnable() { /* NOOP */ }
        protected virtual void OnDisable() { /* NOOP */ }

        /// <summary>
        /// Enables the <see cref="Effect"/> on the <see cref="Control"/>.
        /// </summary>
        public void Enable() { this.Enabled = true; }

        /// <summary>
        /// Disables the <see cref="Effect"/> on the <see cref="Control"/>.
        /// </summary>
        public void Disable() { this.Enabled = false; }

        /// <summary>
        /// Enables or disables the <see cref="ControlEffect"/> depending on the value of <param name="enabled"></param>.
        /// </summary>
        public void SetEnableState(bool enabled) {
            this.Enabled = enabled;
        }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }
        public virtual void PaintEffect(SpriteBatch spriteBatch, Rectangle bounds) { /* NOOP */ }

        public void Draw(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_enabled) {
                spriteBatch.Begin(GetSpriteBatchParameters());

                PaintEffect(spriteBatch, new Rectangle(this.Location.ToPoint(), this.Size.ToPoint()));

                spriteBatch.End();
            }
        }

    }
}
