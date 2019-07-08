using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Trails;
using Microsoft.Scripting.Runtime;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors.Activator {

    public abstract class Activator<TPathable, TEntity> : IDisposable
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        public bool Active { get; private set; } = false;

        protected PathingBehavior<TPathable, TEntity> AssociatedBehavior { get; }

        public Activator([NotNull] PathingBehavior<TPathable, TEntity> associatedBehavior) {
            this.AssociatedBehavior = associatedBehavior;
        }

        protected void Activate() {
            this.Active = true;
            this.Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void Deactivate() {
            this.Active = false;
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
