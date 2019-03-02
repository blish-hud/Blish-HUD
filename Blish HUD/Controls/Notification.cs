using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Notification:Control { 

        public enum NotificationType {
            Red,
            Green,
            Blue,
            Gray
        }

        public int Duration { get; set; }
        public Texture2D Icon { get; set; }
        public string Message { get; set; }
        
        private Glide.Tween notificationLifecycle;

        private Notification(Texture2D icon, string message, int duration) {
            this.Icon = icon;
            this.Message = message;
            this.Duration = duration;

            this.Opacity = 0f;
            this.Size = new Point(1024, 256);
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;
            this.Location = new Point(Graphics.WindowWidth / 2 - this.Size.X / 2, Graphics.WindowHeight / 4 - this.Size.Y / 2);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(Content.GetTexture("chat-no-interaction-blue"), bounds, Color.White);
            spriteBatch.Draw(this.Icon, new Rectangle(64, 32, 128, 128).OffsetBy(bounds.Location), Color.White);

            Utils.DrawUtil.DrawAlignedText(spriteBatch,
                 Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular), this.Message,
                 new Rectangle(bounds.X + 64 + 128 + 17, 33, this.Width - 64 - 128, 128),
                 Color.Black,
                 Utils.DrawUtil.HorizontalAlignment.Left,
                 Utils.DrawUtil.VerticalAlignment.Middle
             );
            Utils.DrawUtil.DrawAlignedText(spriteBatch,
                 Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular), this.Message,
                 new Rectangle(bounds.X + 64 + 128 + 16, 32, this.Width - 64 - 128, 128),
                 Color.White,
                 Utils.DrawUtil.HorizontalAlignment.Left,
                 Utils.DrawUtil.VerticalAlignment.Middle
             );
        }

        private void Show() {
            notificationLifecycle = Animation.Tweener
                .Tween(this, new { Opacity = 1f }, 0.2f)
                .Repeat(1)
                .RepeatDelay(this.Duration)
                .Reflect()
                .OnComplete(Dispose);
        }

        public static void ShowNotification(Texture2D icon, string message, int duration) {
            var nNot = new Notification(icon, message, duration) {
                Parent = Graphics.SpriteScreen
            };

            nNot.Show();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            notificationLifecycle = null;
        }

    }
}
