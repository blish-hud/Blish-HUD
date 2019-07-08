using Blish_HUD.Entities;

namespace Blish_HUD.Pathing.Behaviors {
    public abstract class Interactable<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        protected Interactable(TPathable managedPathable) : base(managedPathable) {
            
        }

    }
}
