using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {

    public abstract class PathingBehavior {

        private const string PATHINGBEHAVIOR_STORENAME = "Behaviors";

        public static List<Type> AllAvailableBehaviors { get; }

        #region Load Static

        private static readonly PersistentStore _behaviorStore;

        static PathingBehavior() {
            _behaviorStore = GameService.Pathing.PathingStore.GetSubstore(PATHINGBEHAVIOR_STORENAME);

            AllAvailableBehaviors = PathingBehaviorAttribute.GetTypes(System.Reflection.Assembly.GetExecutingAssembly()).ToList();
        }

        #endregion

        protected PersistentStore BehaviorStore => _behaviorStore;

        public virtual void Load() { /* NOOP */ }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

    }

    public abstract class PathingBehavior<TPathable, TEntity> : PathingBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {
        
        public TPathable ManagedPathable { get; }

        public PathingBehavior(TPathable managedPathable) {
            this.ManagedPathable = managedPathable;
        }

    }

    /// <summary>
    /// Default behavior (even if one is not defined).  No automatic behaviors are performed if this is the
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

        public float ZoneRadius { get; set; } = 10;

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

    public enum BehaviorWhen {
        Always,
        InZone
    }

    [PathingBehavior("bounce")]
    public class BounceWhenClose<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private BehaviorWhen _bounceWhen = BehaviorWhen.Always;

        private float _bounceDelay;
        private float _bounceHeight   = 2f;
        private float _bounceDuration = 1f;

        private float _originalVerticalOffset;

        private Activator.Activator _bounceActivator;

        public BehaviorWhen BounceWhen {
            get => _bounceWhen;
            set {
                _bounceWhen = value;
                UpdateBounceState();
            }
        }

        public float BounceHeight {
            get => _bounceHeight;
            set {
                _bounceHeight = value;
                UpdateBounceState();
            }
        } 

        public float BounceDelay {
            get => _bounceDelay;
            set {
                _bounceDelay = value;
                UpdateBounceState();
            }
        }

        public float BounceDuration {
            get => _bounceDuration;
            set {
                _bounceDuration = value;
                UpdateBounceState();
            }
        }

        public BounceWhenClose(TPathable managedPathable) : base(managedPathable) {
            _originalVerticalOffset = managedPathable.ManagedEntity.VerticalOffset;
        }

        private Tween _bounceAnimation;

        public override void Load() {
            _bounceAnimation?.CancelAndComplete();
            _bounceAnimation = null;

            UpdateBounceState();
        }

        private void UpdateBounceState() {
            if (_bounceActivator != null) {
                _bounceActivator.Activated   -= BounceActivatorOnActivated;
                _bounceActivator.Deactivated -= BounceActivatorOnDeactivated;

                _bounceActivator.Dispose();
            }

            StopBouncing();

            switch (this.BounceWhen) {
                case BehaviorWhen.InZone:
                    _bounceActivator = new ZoneActivator() {
                        ActivationDistance = 2f,
                        DistanceFrom       = DistanceFrom.Player,
                        Position           = ManagedPathable.Position
                    };

                    _bounceActivator.Activated   += BounceActivatorOnActivated;
                    _bounceActivator.Deactivated += BounceActivatorOnDeactivated;

                    break;
                case BehaviorWhen.Always:
                default:
                    StartBouncing();
                    break;
            }
        }

        private void BounceActivatorOnActivated(object sender, EventArgs e) {
            StartBouncing();
        }

        private void BounceActivatorOnDeactivated(object sender, EventArgs e) {
            StopBouncing();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime) {
            if (_bounceActivator != null && _bounceActivator is ZoneActivator zoneActivator) {
                zoneActivator.Position = ManagedPathable.Position;
                zoneActivator.Update(gameTime);
            }

            base.Update(gameTime);
        }

        private void StartBouncing() {
            _bounceAnimation?.CancelAndComplete();

            _bounceAnimation = GameService.Animation.Tweener.Tween(this.ManagedPathable.ManagedEntity,
                                                                   new { VerticalOffset = _originalVerticalOffset + _bounceHeight },
                                                                   _bounceDuration,
                                                                   _bounceDelay)
                                                            .From(new { VerticalOffset = _originalVerticalOffset })
                                                            .Ease(Ease.QuadInOut)
                                                            .Repeat()
                                                            .Reflect();
        }

        private void StopBouncing() {
            _bounceAnimation?.Cancel();

            _bounceAnimation = GameService.Animation.Tweener.Tween(this.ManagedPathable.ManagedEntity,
                                                                   new { VerticalOffset = _originalVerticalOffset },
                                                                   this.ManagedPathable.ManagedEntity.VerticalOffset / 2f)
                                                            .Ease(Ease.BounceOut);
        }

        public void LoadWithAttributes(IEnumerable<XmlAttribute> attributes) {
            float fOut;

            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
                    case "bounce":
                    case "bounce-height":
                        InvariantUtil.TryParseFloat(attr.Value, out _bounceHeight);
                        break;
                    case "bounce-delay":
                        InvariantUtil.TryParseFloat(attr.Value, out _bounceDelay);
                        break;
                    case "bounce-duration":
                        InvariantUtil.TryParseFloat(attr.Value, out _bounceDuration);
                        break;
                    case "bounce-when":
                        Enum.TryParse(attr.Value, true, out _bounceWhen);
                        break;
                }
            }
        }

    }

}