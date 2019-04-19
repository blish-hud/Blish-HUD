using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Label = Blish_HUD.Controls.Label;

namespace Blish_HUD.Modules.EventTimers {
    public class EventNotification : Container {

        private const int NOTIFICATION_WIDTH = 264;
        private const int NOTIFICATION_HEIGHT = 64;

        private Texture2D Icon;

        private static int VisibleNotifications = 0;

        private Glide.Tween notificationLifecycle;

        private EventNotification(string title, Texture2D icon, string message) {
            Icon = icon;

            this.Opacity = 0f;
            this.Size = new Point(NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
            this.Location = new Point(60, 60 + (NOTIFICATION_HEIGHT + 15) * VisibleNotifications);

            string wrappedTitle = Utils.DrawUtil.WrapText(Content.DefaultFont14, title, this.Width - NOTIFICATION_HEIGHT - 20 - 32);
            var titleLbl = new Label() {
                Parent            = this,
                Location          = new Point(NOTIFICATION_HEIGHT                   + 10, 0),
                Size              = new Point(this.Width - NOTIFICATION_HEIGHT - 10 - 32, this.Height / 2),
                VerticalAlignment = DrawUtil.VerticalAlignment.Middle,
                Font              = Content.DefaultFont14,
                Text              = wrappedTitle,
            };

            string wrapped = Utils.DrawUtil.WrapText(Content.DefaultFont14, message, this.Width - NOTIFICATION_HEIGHT - 20 - 32);
            var messageLbl = new Label() {
                Parent            = this,
                Location          = new Point(NOTIFICATION_HEIGHT                   + 10, this.Height / 2),
                Size              = new Point(this.Width - NOTIFICATION_HEIGHT - 10 - 32, this.Height / 2),
                VerticalAlignment = DrawUtil.VerticalAlignment.Middle,
                Text              = wrapped,
            };

            VisibleNotifications++;

            this.RightMouseButtonReleased += delegate { this.Dispose(); };
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(Content.GetTexture("ns-button"), bounds, Color.White * 0.85f);

            int icoSize = Math.Min(Icon.Width, 52);

            spriteBatch.Draw(Icon, new Rectangle(NOTIFICATION_HEIGHT / 2 - icoSize / 2, NOTIFICATION_HEIGHT / 2 - icoSize / 2, icoSize, icoSize), Color.White);
        }

        private void Show(float duration) {
            Content.PlaySoundEffectByName(@"audio\color-change");

            notificationLifecycle = Animation.Tweener
                                             .Tween(this, new { Opacity = 1f }, 0.2f)
                                             .Repeat(1)
                                             .RepeatDelay(duration)
                                             .Reflect()
                                             .OnComplete(Dispose);
        }

        public static void ShowNotification(string title, Texture2D icon, string message, float duration) {
            var notif = new EventNotification(title, icon, message) {
                Parent = Graphics.SpriteScreen
            };

            notif.Show(duration);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            notificationLifecycle = null;
            VisibleNotifications--;
        }

    }
}
