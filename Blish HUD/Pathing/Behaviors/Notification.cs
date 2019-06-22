using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {

    [IdentifyingBehaviorAttributePrefix("notification")]
    public class Notification<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private ZoneActivator _activator;

        private string _notificationMessage = string.Empty;
        private ScreenNotification.NotificationType _notificationType = ScreenNotification.NotificationType.Info;

        private bool _canResendMessage = true;

        public string NotificationMessage {
            get => _notificationMessage;
            set => _notificationMessage = value;
        }

        public ScreenNotification.NotificationType NotificationType {
            get => _notificationType;
            set => _notificationType = value;
        }

        /// <inheritdoc />
        public Notification(TPathable managedPathable) : base(managedPathable) { }

        /// <inheritdoc />
        public override void Load() {
            if (_activator != null) {
                _activator.Activated   -= ActivatorOnActivated;
                _activator.Deactivated -= ActivatorOnDeactivated;

                _activator.Dispose();
            }

            _activator = new ZoneActivator() {
                ActivationDistance = 4f,
                DistanceFrom       = DistanceFrom.Player,
                Position           = ManagedPathable.Position
            };

            _activator.Activated   += ActivatorOnActivated;
            _activator.Deactivated += ActivatorOnDeactivated;
        }

        private void ActivatorOnActivated(object sender, EventArgs e) {
            if (_canResendMessage)
                Controls.ScreenNotification.ShowNotification(_notificationMessage, _notificationType);

            _canResendMessage = false;
        }

        private void ActivatorOnDeactivated(object sender, EventArgs e) {
            _canResendMessage = true;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime) {
            _activator.Position = ManagedPathable.Position;
            _activator.Update(gameTime);
        }

        /// <inheritdoc />
        public void LoadWithAttributes(IEnumerable<XmlAttribute> attributes) {
            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
                    case "notification":
                        _notificationMessage = attr.Value;
                        break;
                    case "notification-type":
                        Enum.TryParse(attr.Value, true, out _notificationType);
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
