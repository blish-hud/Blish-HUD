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
    [PathingBehavior("message")]
    public class Message<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private BehaviorWhen _messageWhen = BehaviorWhen.InZone;

        private float _messageDuration = 1f;

        private float _originalVerticalOffset;

        public BehaviorWhen MessageWhen {
            get => _messageWhen;
            set {
                _messageWhen = value;
                UpdateMessageState();
            }
        }

        public float MessageDuration {
            get => _messageDuration;
            set {
                _messageDuration = value;
                UpdateMessageState();
            }
        }

        public Message(TPathable managedPathable) : base(managedPathable) {
            _originalVerticalOffset = managedPathable.ManagedEntity.VerticalOffset;
        }

        private Tween _messageAnimation;

        public override void Load() {
            _messageAnimation?.CancelAndComplete();
            _messageAnimation = null;

            UpdateMessageState();
        }

        private void UpdateMessageState() {
            if (this.Activator != null) {
                this.Activator.Activated   -= MessageActivatorOnActivated;
                this.Activator.Deactivated -= MessageActivatorOnDeactivated;

                this.Activator.Dispose();
            }

            StopMessage();

            switch (this.MessageWhen) {
                case BehaviorWhen.InZone:
                    this.Activator = new ZoneActivator<TPathable, TEntity>(this) {
                        ActivationDistance = 2f,
                        DistanceFrom = DistanceFrom.Player
                    };

                    this.Activator.Activated += MessageActivatorOnActivated;
                    this.Activator.Deactivated += MessageActivatorOnDeactivated;

                    break;
                case BehaviorWhen.Always:
                    this.Activator = new Always<TPathable, TEntity>(this);
                    break;
                default:
                    StartMessage();
                    break;
            }
        }

        private void MessageActivatorOnActivated(object sender, EventArgs e) {
            StartMessage();
        }

        private void MessageActivatorOnDeactivated(object sender, EventArgs e) {
            StopMessage();
        }

        private void StartMessage() {
            _messageAnimation?.CancelAndComplete();

            _messageAnimation = GameService.Animation.Tweener.Tween(this.ManagedPathable.ManagedEntity,
                                                                   new { VerticalOffset = _originalVerticalOffset },
                                                                   _messageDuration,
                                                                   0)
                                                            .From(new { VerticalOffset = _originalVerticalOffset })
                                                            .Ease(Ease.QuadInOut)
                                                            .Repeat()
                                                            .Reflect();
        }

        private void StopMessage() {
            _messageAnimation?.Cancel();

            _messageAnimation = GameService.Animation.Tweener.Tween(this.ManagedPathable.ManagedEntity,
                                                                   new { VerticalOffset = _originalVerticalOffset },
                                                                   this.ManagedPathable.ManagedEntity.VerticalOffset / 2f)
                                                            .Ease(Ease.BounceOut);
        }

        public void LoadWithAttributes(IEnumerable<XmlAttribute> attributes) {
            float fOut;

            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
                    case "message":
                        break;
                }
            }
        }

    }
}
