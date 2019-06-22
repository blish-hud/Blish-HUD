using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;

namespace Blish_HUD.Pathing.Behaviors.Activator {

    public abstract class Activator : IDisposable {

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        private bool _active = false;

        public bool Active => _active;

        protected void Activate() {
            _active = true;
            this.Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void Deactivate() {
            _active = false;
            this.Deactivated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

        protected virtual void OnDispose() { /* NOOP */ }

        private void Dispose(bool disposing) {
            if (disposing) {
                OnDispose();
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}
