﻿using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class ScreenNotification : Control {

        private const int DURATION_DEFAULT = 4;

        private const int NOTIFICATION_WIDTH  = 1024;
        private const int NOTIFICATION_HEIGHT = 256;

        #region Load Static

        private static readonly SynchronizedCollection<ScreenNotification> _activeScreenNotifications;

        private static readonly BitmapFont _fontMenomonia36Regular;

        private static readonly Texture2D _textureGrayBackground;
        private static readonly Texture2D _textureBlueBackground;
        private static readonly Texture2D _textureGreenBackground;
        private static readonly Texture2D _textureRedBackground;

        static ScreenNotification() {
            _activeScreenNotifications = new SynchronizedCollection<ScreenNotification>();

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

        private NotificationType _type;
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
        private int _targetTop = 0;

        private Rectangle _layoutMessageBounds;
        private Rectangle _layoutIconBounds;

        private ScreenNotification(string message, NotificationType type = NotificationType.Info, Texture2D icon = null, int duration = DURATION_DEFAULT) {
            _message  = message;
            _type     = type;
            _icon     = icon;
            _duration = duration;

            this.Opacity = 0f;
            this.Size = new Point(NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;
            this.Location = new Point(Graphics.WindowWidth / 2 - this.Size.X / 2, Graphics.WindowHeight / 4 - this.Size.Y / 2);

            _targetTop = this.Top;
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

            Color     messageColor           = Color.White;
            Texture2D notificationBackground = null;

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
                    notificationBackground = _textureGrayBackground;
                    break;

                case NotificationType.Blue:
                    notificationBackground = _textureBlueBackground;
                    break;

                case NotificationType.Green:
                    notificationBackground = _textureGreenBackground;
                    break;

                case NotificationType.Red:
                    notificationBackground = _textureRedBackground;
                    break;
            }

            if (notificationBackground != null)
                spriteBatch.DrawOnCtrl(this, notificationBackground, _layoutMessageBounds);

            // TODO: Add back drawing icon: (something like) spriteBatch.Draw(this.Icon, new Rectangle(64, 32, 128, 128).OffsetBy(bounds.Location), Color.White);

            spriteBatch.DrawStringOnCtrl(this,
                                         this.Message,
                                         _fontMenomonia36Regular,
                                         bounds.OffsetBy(1, 1),
                                         Color.Black,
                                         false,
                                         HorizontalAlignment.Center);

            spriteBatch.DrawStringOnCtrl(this,
                                         this.Message,
                                         _fontMenomonia36Regular,
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

        private void SlideDown(int distance) {
            _targetTop += distance;

            Animation.Tweener.Tween(this, new {Top = _targetTop}, 0.1f);

            if (_opacity < 1f) return;

            _animFadeLifecycle = Animation.Tweener
                                          .Tween(this, new {Opacity = 0f}, 1f)
                                          .OnComplete(Dispose);
        }

        /// <inheritdoc />
        protected override void DisposeControl() {
            _activeScreenNotifications.Remove(this);

            base.DisposeControl();
        }

        public static void ShowNotification(string message, NotificationType type = NotificationType.Info, Texture2D icon = null, int duration = DURATION_DEFAULT) {
            var nNot = new ScreenNotification(message, type, icon, duration) {
                Parent = Graphics.SpriteScreen
            };

            nNot.ZIndex = _activeScreenNotifications.DefaultIfEmpty(nNot).Max(n => n.ZIndex) + 1;

            foreach (var activeScreenNotification in _activeScreenNotifications) {
                activeScreenNotification.SlideDown((int)(_fontMenomonia36Regular.LineHeight * 0.75f));
            }

            _activeScreenNotifications.Add(nNot);

            nNot.Show();
        }

    }
}
