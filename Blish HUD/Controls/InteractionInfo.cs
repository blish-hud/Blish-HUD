using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class InteractionInfo : Label {

        private const int CONTROL_WIDTH = 170;
        private const int CONTROL_HEIGHT = 85;

        private const float LEFT_OFFSET = 0.67f;
        private const float TOP_OFFSET = 0.71f;

        private const string DEFAULT_INFO_TEXT = "Info";

        private Tween _fadeAnimation;

        private int _verticalIndex = 0;

        public int VerticalIndex {
            get => _verticalIndex;
            set => SetProperty(ref _verticalIndex, value, invalidateParentOnly: true);
        }

        public InteractionInfo() {
            this.Text = DEFAULT_INFO_TEXT;
            this.VerticalAlignment = DrawUtil.VerticalAlignment.Middle;
            this.ShowShadow = true;
            this.StrokeShadow = true;
            this.Font = Content.DefaultFont12;
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

        public void Show() {
            _fadeAnimation?.Cancel();

            this.Visible = true;

            _fadeAnimation = Animation.Tweener.Tween(this, new { Opacity = 0.9f }, (0.9f - this.Opacity) / 2);
        }

        public void Hide() {
            _fadeAnimation?.Cancel();
            _fadeAnimation = Animation.Tweener.Tween(this, new { Opacity = 0f }, this.Opacity / 2).OnComplete(
                                                                                                              delegate {
                                                                                                                  this.Visible = false;
                                                                                                              });
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var textRegion = new Rectangle(
                                           (int)(bounds.Width  * 0.24),
                                           (int)(bounds.Height * 0.34),
                                           (int)(bounds.Width  * 0.62),
                                           (int)(bounds.Height * 0.26)
                                          );
            
            spriteBatch.Draw(Content.GetTexture("156775"), bounds, Color.White);

            DrawText(spriteBatch, textRegion, $"{Utils.DrawUtil.WrapText(this.Font, this.Text, textRegion.Width)}");
        }

    }
}
