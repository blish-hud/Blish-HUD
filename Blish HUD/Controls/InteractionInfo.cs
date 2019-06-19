using Blish_HUD.Utils;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class InteractionInfo : LabelBase {

        private const int CONTROL_WIDTH = 170;
        private const int CONTROL_HEIGHT = 85;

        private const float LEFT_OFFSET = 0.67f;
        private const float TOP_OFFSET = 0.71f;

        private const string DEFAULT_INFO_TEXT = "Info";

        private Tween _fadeAnimation;

        /// <summary>
        /// The text this <see cref="InteractionInfo"/> should show.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        protected int _verticalIndex = 0;

        public int VerticalIndex {
            get => _verticalIndex;
            set => SetProperty(ref _verticalIndex, value, invalidateLayout: true);
        }

        public InteractionInfo() {
            _text = DEFAULT_INFO_TEXT;
            _verticalAlignment = DrawUtil.VerticalAlignment.Middle;
            _showShadow = true;
            _strokeText = true;
            _font = Content.DefaultFont12;
            this.Size = new Point(CONTROL_WIDTH, CONTROL_HEIGHT);
            this.Location = new Point((int)(Graphics.WindowWidth * LEFT_OFFSET), (int)(Graphics.WindowHeight * TOP_OFFSET) - CONTROL_HEIGHT * _verticalIndex);
            this.Opacity = 0f;
            this.Visible = false;
            this.Parent = Graphics.SpriteScreen;

            Graphics.SpriteScreen.Resized += delegate {
                this.Location = new Point(
                                          (int)(Graphics.WindowWidth * LEFT_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)),
                                          (int)(Graphics.WindowHeight * TOP_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)) - CONTROL_HEIGHT * _verticalIndex
                                         );
            };

        }

        public override void Show() {
            _fadeAnimation?.Cancel();

            this.Visible = true;

            _fadeAnimation = Animation.Tweener.Tween(this, new { Opacity = 0.9f }, (0.9f - this.Opacity) / 2);
        }

        public override void Hide() {
            _fadeAnimation?.Cancel();
            _fadeAnimation = Animation.Tweener.Tween(this, new { Opacity = 0f }, this.Opacity / 2).OnComplete(
                                                                                                              delegate {
                                                                                                                  this.Visible = false;
                                                                                                              });
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var textRegion = new Rectangle(
                                           (int)(_size.X  * 0.24),
                                           (int)(bounds.Height * 0.34),
                                           (int)(_size.X * 0.62),
                                           (int)(_size.Y * 0.26)
                                          );

            spriteBatch.DrawOnCtrl(this,
                                   Content.GetTexture("156775"),
                                   bounds);

            DrawText(spriteBatch,
                     textRegion,
                     $"{Utils.DrawUtil.WrapText(_font, _text, textRegion.Width)}");
        }

    }
}
