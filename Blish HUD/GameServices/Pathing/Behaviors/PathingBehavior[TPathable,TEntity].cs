using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {
    public abstract class PathingBehavior<TPathable, TEntity> : PathingBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public Activator<TPathable, TEntity> Activator { get; set; }

        public TPathable ManagedPathable { get; }

        public PathingBehavior(TPathable managedPathable) {
            this.ManagedPathable = managedPathable;
        }

        /// <inheritdoc />
        public override void UpdateBehavior(GameTime gameTime) {
            this.Activator?.Update(gameTime);
            this.Update(gameTime);
        }

        protected virtual void Update(GameTime gameTime) { /* NOOP */ }

    }
}
