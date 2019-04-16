using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Markers;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behavior {

    public abstract class PathingBehavior {

        public static List<PathingBehavior> AllAvailableBehaviors;

        private const string PATHINGBEHAVIOR_STORENAME = "Behaviors";

        protected static PersistentStore _behaviorStore;
        protected        PersistentStore BehaviorStore => _behaviorStore ?? (_behaviorStore = GameService.Pathing.PathingStore.GetSubstore(PATHINGBEHAVIOR_STORENAME));

    }

    public abstract class PathingBehavior<TPathable, TEntity> :PathingBehavior, IUpdatable
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {
        
        public TPathable ManagedPathable { get; }

        public PathingBehavior(TPathable managedPathable) {
            this.ManagedPathable = managedPathable;
        }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

    }

    /// <summary>
    /// Default behavior (even if one is not defined).  Not automatic behaviors are performed if this is the
    /// only <see cref="PathingBehavior"/> applied to the <see cref="ManagedPathable{TEntity}"/>.
    /// </summary>
    public class Default<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public Default(TPathable managedPathable) : base(managedPathable) { /* NOOP */ }
    }

    public abstract class InZone<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public float ZoneRadius { get; set; }

        public bool InZoneRadius { get; protected set; }

        protected InZone(TPathable managedPathable) : base(managedPathable) { }
        
        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (this.ManagedPathable.ManagedEntity.DistanceFromPlayer < this.ZoneRadius) {
                if (!this.InZoneRadius) {
                    this.InZoneRadius = true;
                    OnEnterZoneRadius(gameTime);
                }
                WhileInZoneRadius(gameTime);
            } else if (this.InZoneRadius) {
                this.InZoneRadius = false;
                OnLeftZoneRadius(gameTime);
            }
        }

        public virtual void OnEnterZoneRadius(GameTime gameTime) { /* NOOP */ }

        public virtual void OnLeftZoneRadius(GameTime gameTime) { /* NOOP */ }

        public virtual void WhileInZoneRadius(GameTime gameTime) { /* NOOP */ }

    }

    public abstract class Interactable<TPathable, TEntity> : InZone<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private InteractionIndicator _indicator;

        protected Interactable(TPathable managedPathable) : base(managedPathable) {
            //_indicator = new InteractionIndicator();
        }



    }



    public class BounceWhenClose<TPathable, TEntity> : InZone<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public bool BounceWhileInZone { get; set; } = true;

        public BounceWhenClose(TPathable managedPathable) : base(managedPathable) { }

        private Tween _bounceAnimation;

        public override void OnEnterZoneRadius(GameTime gameTime) {
            _bounceAnimation?.CancelAndComplete();

            _bounceAnimation = GameService.Animation.Tweener.Tween(
                                                        this.ManagedPathable.ManagedEntity,
                                                        new {VerticalOffset = 2f},
                                                        1f,
                                                        0f, false
                                                       )
                                          .Ease(Ease.QuadInOut);

            if (this.BounceWhileInZone)
                _bounceAnimation.Repeat().Reflect();
        }

        public override void OnLeftZoneRadius(GameTime gameTime) {
            var animTimeLeft = _bounceAnimation.TimeRemaining;

            _bounceAnimation?.Cancel();

            _bounceAnimation = GameService.Animation.Tweener.Tween(
                                                                   this.ManagedPathable.ManagedEntity,
                                                                   new {VerticalOffset = 0f},
                                                                   this.ManagedPathable.ManagedEntity.VerticalOffset / 2f,
                                                                   0f, true
                                                                  )
                                          .Ease(Ease.BounceOut);
        }

    }

}