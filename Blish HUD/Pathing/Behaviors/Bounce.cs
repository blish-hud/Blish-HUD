using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {
    [PathingBehavior("bounce")]
    public class Bounce<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private BehaviorWhen _bounceWhen = BehaviorWhen.Always;

        private float _bounceDelay;
        private float _bounceHeight = 2f;
        private float _bounceDuration = 1f;

        private float _originalVerticalOffset;

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

        public Bounce(TPathable managedPathable) : base(managedPathable) {
            _originalVerticalOffset = managedPathable.ManagedEntity.VerticalOffset;
        }

        private Tween _bounceAnimation;

        public override void Load() {
            _bounceAnimation?.CancelAndComplete();
            _bounceAnimation = null;

            UpdateBounceState();
        }

        private void UpdateBounceState() {
            if (this.Activator != null) {
                this.Activator.Activated   -= BounceActivatorOnActivated;
                this.Activator.Deactivated -= BounceActivatorOnDeactivated;

                this.Activator.Dispose();
            }

            StopBouncing();

            switch (this.BounceWhen) {
                case BehaviorWhen.InZone:
                    this.Activator = new ZoneActivator<TPathable, TEntity>(this) {
                        ActivationDistance = 2f,
                        DistanceFrom = DistanceFrom.Player
                    };

                    this.Activator.Activated += BounceActivatorOnActivated;
                    this.Activator.Deactivated += BounceActivatorOnDeactivated;

                    break;
                case BehaviorWhen.Always:
                    this.Activator = new Always<TPathable, TEntity>(this);
                    break;
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
