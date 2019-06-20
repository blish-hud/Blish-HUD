using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class Notification : Control {

        private const int DURATION_DEFAULT = 4;

        private const int NOTIFICATION_WIDTH  = 1024;
        private const int NOTIFICATION_HEIGHT = 256;

        #region Load Static

        private static Notification _activeNotification;

        private static readonly BitmapFont _fontMenomonia36Regular;

        private static readonly Texture2D _textureGrayBackground;
        private static readonly Texture2D _textureBlueBackground;
        private static readonly Texture2D _textureGreenBackground;
        private static readonly Texture2D _textureRedBackground;

        static Notification() {
            _fontMenomonia36Regular = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular);

            _textureGrayBackground  = Content.GetTexture(@"controls\notification\notification-gray");
            _textureBlueBackground  = Content.GetTexture(@"controls\notification\notification-blue");
            _textureGreenBackground = Content.GetTexture(@"controls\notification\notification-green");
            _textureRedBackground   = Content.GetTexture(@"controls\notification\notification-red");
        }

        #endregion

        public enum NotificationType {
            Info,
            Warning,
            Error,

            Gray,
            Blue,
            Green,
            Red,
        }

        private NotificationType _type = NotificationType.Info;
        public NotificationType Type {
            get => _type;
            set => SetProperty(ref _type, value, true);
        }

        private int _duration;
        public int Duration {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private Texture2D _icon;

        public Texture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private string _message;
        public string Message {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        
        private Glide.Tween _animFadeLifecycle;

        private Rectangle _layoutMessageBounds;
        private Rectangle _layoutIconBounds;

        private Notification(string message, NotificationType type = NotificationType.Info, Texture2D icon = null, int duration = DURATION_DEFAULT) {
            _message  = message;
            _type     = type;
            _icon     = icon;
            _duration = duration;

            this.Opacity = 0f;
            this.Size = new Point(NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;
            this.Location = new Point(Graphics.WindowWidth / 2 - this.Size.X / 2, Graphics.WindowHeight / 4 - this.Size.Y / 2);
        }

        /// <inheritdoc />
        protected override CaptureType CapturesInput() {
            return CaptureType.ForceNone;
        }

        /// <inheritdoc />
        public override void RecalculateLayout() {
            switch (_type) {
                case NotificationType.Info:
                case NotificationType.Warning:
                case NotificationType.Error:
                    _layoutMessageBounds = this.LocalBounds;
                    break;

                case NotificationType.Gray:
                case NotificationType.Blue:
                case NotificationType.Green:
                case NotificationType.Red:
                    _layoutMessageBounds = this.LocalBounds;
                    break;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (string.IsNullOrEmpty(_message)) return;

            Color messageColor = Color.White;

            switch (_type) {
                case NotificationType.Info:
                    messageColor = Color.White;
                    break;

                case NotificationType.Warning:
                    messageColor = StandardColors.Yellow;
                    break;

                case NotificationType.Error:
                    messageColor = StandardColors.Red;
                    break;

                case NotificationType.Gray:
                    spriteBatch.DrawOnCtrl(this, _textureGrayBackground, bounds);
                    break;

                case NotificationType.Blue:
                    spriteBatch.DrawOnCtrl(this, _textureBlueBackground, bounds);
                    break;

                case NotificationType.Green:
                    spriteBatch.DrawOnCtrl(this, _textureGreenBackground, bounds);
                    break;

                case NotificationType.Red:
                    spriteBatch.DrawOnCtrl(this, _textureRedBackground, bounds);
                    break;
            }

            // TODO: Add back drawing icon

            //spriteBatch.Draw(this.Icon, new Rectangle(64, 32, 128, 128).OffsetBy(bounds.Location), Color.White);
            
            spriteBatch.DrawStringOnCtrl(this,
                                         this.Message,
                                         _fontMenomonia36Regular,
                                         //_layoutMessageBounds.OffsetBy(1, 1),
                                         bounds.OffsetBy(1, 1),
                                         Color.Black,
                                         false,
                                         HorizontalAlignment.Center);

            spriteBatch.DrawStringOnCtrl(this,
                                         this.Message,
                                         _fontMenomonia36Regular,
                                         //_layoutMessageBounds,
                                         bounds,
                                         messageColor,
                                         false,
                                         HorizontalAlignment.Center);
        }

        /// <inheritdoc />
        public override void Show() {
            _animFadeLifecycle = Animation.Tweener
                                          .Tween(this, new { Opacity = 1f }, 0.2f)
                                          .Repeat(1)
                                          .RepeatDelay(this.Duration)
                                          .Reflect()
                                          .OnComplete(Dispose);

            base.Show();
        }

        public static void ShowNotification(string message, NotificationType type = NotificationType.Info, Texture2D icon = null, int duration = DURATION_DEFAULT) {
            var nNot = new Notification(message, type, icon, duration) {
                Parent = Graphics.SpriteScreen
            };

            _activeNotification?.Hide();

            _activeNotification = nNot;

            nNot.Show();
        }

    }
}
