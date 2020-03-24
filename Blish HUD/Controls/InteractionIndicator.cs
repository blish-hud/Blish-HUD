using Blish_HUD;
using Glide;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {
    public class InteractionIndicator : LabelBase {

        private const int CONTROL_WIDTH  = 256;
        private const int CONTROL_HEIGHT = 64;

        private const float LEFT_OFFSET = 0.65f;
        private const float TOP_OFFSET  = 0.67f;

        private Tween _fadeAnimation;

        protected Keys[] _interactionKeys = new [] { Keys.F };
        protected int _verticalIndex = 1;


        /// <summary>
        /// The text this <see cref="InteractionIndicator"/> should show.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        public int VerticalIndex {
            get => _verticalIndex;
            set {
                if (SetProperty(ref _verticalIndex, value, invalidateLayout: true))
                    this.Top = (int) (Graphics.WindowHeight * TOP_OFFSET * Graphics.GetScaleRatio(UiSize.Large)) - CONTROL_HEIGHT * _verticalIndex;
            }
        }

        public Keys[] InteractionKeys {
            get => _interactionKeys;
            set => SetProperty(ref _interactionKeys, value);
        }

        public InteractionIndicator() {
            _text              = Strings.Controls.InteractionIndicator_Interact;
            _verticalAlignment = VerticalAlignment.Middle;
            _showShadow        = true;
            _strokeText        = true;
            _font              = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
            _size              = new Point((int)(CONTROL_WIDTH        * Graphics.GetScaleRatio(UiSize.Large)), (int)(CONTROL_HEIGHT * Graphics.GetScaleRatio(UiSize.Large)));
            _location          = new Point((int)(Graphics.WindowWidth * LEFT_OFFSET),                                           (int)(Graphics.WindowHeight * TOP_OFFSET) - CONTROL_HEIGHT * _verticalIndex);
            _opacity           = 0f;
            _visible           = false;
            this.Parent        = Graphics.SpriteScreen;

            Graphics.SpriteScreen.Resized += delegate {
                this.Location = new Point((int) (Graphics.WindowWidth * LEFT_OFFSET * Graphics.GetScaleRatio(UiSize.Large)), 
                                          (int) (Graphics.WindowHeight * TOP_OFFSET * Graphics.GetScaleRatio(UiSize.Large)) - CONTROL_HEIGHT * _verticalIndex);
            };
        }

        public override void Show() {
            _fadeAnimation?.Cancel();

            this.Visible = true;

            _fadeAnimation = Animation.Tweener.Tween(this, new {Opacity = 1f}, (1f - this.Opacity) / 2);
        }

        public override void Hide() {
            _fadeAnimation?.Cancel();
            _fadeAnimation = Animation.Tweener.Tween(this, new { Opacity = 0f }, this.Opacity / 2).OnComplete(
                                                                                                          delegate {
                                                                                                              this.Visible = false;
                                                                                                          });
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, Content.GetTexture("156775"),
                             bounds,
                             bounds.OffsetBy(0, CONTROL_HEIGHT / 2),
                             Color.White);
            
            DrawText(spriteBatch, new Rectangle((int)(bounds.Width * 0.2),
                                                (int)(bounds.Height * 0.13),
                                                (int)(bounds.Width  * 0.78),
                                                (int)(bounds.Height * 0.5)),
                     $"{DrawUtil.WrapText(_font, _text, bounds.Width * 0.5f)} [{string.Join(" + ", this.InteractionKeys)}]");
        }

    }
}
