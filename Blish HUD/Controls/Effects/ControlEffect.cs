using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Blish_HUD.Controls.Effects {
    public abstract class ControlEffect {
        
        protected Control AssignedControl { get; }

        /// <summary>
        /// The size within the <see cref="Control"/> it applies to.
        /// </summary>
        public Vector2 Size     { get; set; }

        /// <summary>
        /// The location relative to the <see cref="Control"/> it applies to.
        /// </summary>
        public Vector2 Location { get; set; }

        private bool _enabled = true;
        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;

                _enabled = value;

                if (_enabled) OnEnable();
                else OnDisable();
            }
        }

        public ControlEffect(Control assignedControl) {
            this.AssignedControl = assignedControl;

            this.Size = assignedControl.Size.ToVector2();
        }

        public abstract SpriteBatchParameters GetSpriteBatchParameters();

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

        public virtual void Update(GameTime gameTime) { /* NOOP */ }
        public virtual void PaintEffect(SpriteBatch spriteBatch, Rectangle bounds) { /* NOOP */ }

        public void Draw(SpriteBatch spriteBatch, Rectangle drawBounds) {
            if (_enabled) {
                spriteBatch.Begin(GetSpriteBatchParameters());

                PaintEffect(spriteBatch, new Rectangle(this.Location.ToPoint(), this.Size.ToPoint()));

                spriteBatch.End();
            }
        }

    }
}
