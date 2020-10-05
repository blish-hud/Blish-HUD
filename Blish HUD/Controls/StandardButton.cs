using System.ComponentModel;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public class StandardButton : LabelBase {

        public const int STANDARD_CONTROL_HEIGHT = 26;
        public const int DEFAULT_CONTROL_WIDTH   = 128;

        private const int ICON_SIZE        = 16;
        private const int ICON_TEXT_OFFSET = 4;

        private const int ATLAS_SPRITE_WIDTH  = 350;
        private const int ATLAS_SPRITE_HEIGHT = 20;

        private const int   ANIM_FRAME_COUNT = 8;
        private const float ANIM_FRAME_TIME  = 0.25f;

        #region Load Static

        private static readonly Texture2D _textureButtonIdle;
        private static readonly Texture2D _textureButtonBorder;
        
        static StandardButton() {
            _textureButtonIdle   = Content.GetTexture(@"common/button-states");
            _textureButtonBorder = Content.GetTexture("button-border");
        }

        #endregion

        /// <summary>
        /// The text shown on the button.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        private AsyncTexture2D _icon;

        /// <summary>
        /// An icon to show on the <see cref="StandardButton"/>.  For best results, the <see cref="Icon"/> should be 16x16.
        /// </summary>
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value, true);
        }

        private bool _resizeIcon;

        /// <summary>
        /// If true, the <see cref="Icon"/> texture will be resized to 16x16.
        /// </summary>
        public bool ResizeIcon {
            get => _resizeIcon;
            set => SetProperty(ref _resizeIcon, value, true);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        /// <summary>
        /// Do not directly manipulate this property.  It is only public because the animation library requires it to be public.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int AnimationState { get; set; } = 0;

        private Tween _animIn;
        private Tween _animOut;

        public StandardButton() {
            _textColor           = Color.Black;
            _horizontalAlignment = HorizontalAlignment.Left;
            _verticalAlignment   = VerticalAlignment.Middle;

            this.Size = new Point(DEFAULT_CONTROL_WIDTH, STANDARD_CONTROL_HEIGHT);
        }

        private void TriggerAnimation(bool directionIn) {
            _animIn?.Pause();
            _animOut?.Pause();

            if (directionIn) {
                _animIn = GameService.Animation.Tweener.Tween(this,
                                                              new { AnimationState = ANIM_FRAME_COUNT },
                                                              ANIM_FRAME_TIME - (_animOut?.TimeRemaining ?? 0));
            } else {
                _animOut = GameService.Animation.Tweener.Tween(this,
                                                               new { AnimationState = 0 },
                                                               ANIM_FRAME_TIME - (_animIn?.TimeRemaining ?? 0));
            }
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            TriggerAnimation(true);

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            TriggerAnimation(false);

            base.OnMouseLeft(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnClick(e);
        }

        private Rectangle _layoutIconBounds = Rectangle.Empty;
        private Rectangle _layoutTextBounds = Rectangle.Empty;

        public override void RecalculateLayout() {
            // TODO: Ensure that these calculations are correctly placing the image in the middle and clean things up
            var textSize = GetTextDimensions();

            int textLeft = (int)(_size.X / 2 - textSize.Width / 2);

            if (_icon != null) {
                if (textSize.Width > 0) {
                    textLeft += ICON_SIZE / 2 + ICON_TEXT_OFFSET / 2;
                } else {
                    textLeft += ICON_SIZE / 2;
                }

                var iconSize = _resizeIcon ? new Point(ICON_SIZE) : _icon.Texture.Bounds.Size;

                _layoutIconBounds = new Rectangle(textLeft - iconSize.X - ICON_TEXT_OFFSET, _size.Y / 2 - iconSize.Y / 2, iconSize.X, iconSize.Y);
            }

            _layoutTextBounds = new Rectangle(textLeft, 0, _size.X - textLeft, _size.Y);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Button Texture
            if (_enabled) {
                spriteBatch.DrawOnCtrl(this,
                                       _textureButtonIdle,
                                       new Rectangle(3, 3, _size.X - 6, _size.Y - 5),
                                       new Rectangle(this.AnimationState * ATLAS_SPRITE_WIDTH, 0, ATLAS_SPRITE_WIDTH, ATLAS_SPRITE_HEIGHT));
            } else { 
                // TODO: Use the actual button texture instead
                spriteBatch.DrawOnCtrl(this,
                                       ContentService.Textures.Pixel,
                                       new Rectangle(3, 3, _size.X - 6, _size.Y - 5),
                                       Color.FromNonPremultiplied(121, 121, 121, 255));
            }

            // Top Shadow
            spriteBatch.DrawOnCtrl(this,
                                   _textureButtonBorder,
                                   new Rectangle(2, 0, this.Width - 5, 4),
                                   new Rectangle(0, 0, 1,              4));

            // Right Shadow
            spriteBatch.DrawOnCtrl(this,
                                   _textureButtonBorder,
                                   new Rectangle(this.Width - 4, 2, 4, this.Height - 3),
                                   new Rectangle(0,              1, 4, 1));

            // Bottom Shadow
            spriteBatch.DrawOnCtrl(this,
                                   _textureButtonBorder,
                                   new Rectangle(3, this.Height - 4, this.Width - 6, 4),
                                   new Rectangle(1, 0,               1,              4));

            // Left Shadow
            spriteBatch.DrawOnCtrl(this,
                                   _textureButtonBorder,
                                   new Rectangle(0, 2, 4, this.Height - 3),
                                   new Rectangle(0, 3, 4, 1));

            // Draw Icon
            if (_icon != null) {
                spriteBatch.DrawOnCtrl(this,
                                       _icon,
                                       _layoutIconBounds);
            }

            // TODO: Don't set button text color like this
            _textColor = _enabled ? Color.Black : Color.FromNonPremultiplied(51, 51, 51, 255);
            // Button Text
            DrawText(spriteBatch, _layoutTextBounds);
        }

    }

}
