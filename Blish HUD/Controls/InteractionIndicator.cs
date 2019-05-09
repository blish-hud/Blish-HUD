using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class InteractionIndicator : LabelBase {

        private const int CONTROL_WIDTH = 256;
        private const int CONTROL_HEIGHT = 64;

        private const float LEFT_OFFSET = 0.65f;
        private const float TOP_OFFSET = 0.67f;

        private const string DEFAULT_INTERACT_TEXT = "Interact";

        private Tween _fadeAnimation;

        protected Keys[] _interactionKeys = new [] { Keys.F };
        protected int _verticalIndex = 1;

        public int VerticalIndex {
            get => _verticalIndex;
            set {
                if (SetProperty(ref _verticalIndex, value, invalidateLayout: true))
                    this.Top = (int) (Graphics.WindowHeight * TOP_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)) - CONTROL_HEIGHT * _verticalIndex;
            }
        }

        public Keys[] InteractionKeys {
            get => _interactionKeys;
            set => SetProperty(ref _interactionKeys, value);
        }

        public InteractionIndicator() {
            this.Text = DEFAULT_INTERACT_TEXT;
            this.VerticalAlignment = DrawUtil.VerticalAlignment.Middle;
            this.ShowShadow = true;
            this.StrokeText = true;
            this.Font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
            this.Size = new Point((int)(CONTROL_WIDTH * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)), (int)(CONTROL_HEIGHT * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)));
            this.Location = new Point((int)(Graphics.WindowWidth * LEFT_OFFSET), (int)(Graphics.WindowHeight * TOP_OFFSET) - CONTROL_HEIGHT * _verticalIndex);
            this.Opacity = 0f;
            this.Visible = false;
            this.Parent = Graphics.SpriteScreen;

            Graphics.SpriteScreen.Resized += delegate {
                this.Location = new Point(
                                          (int) (Graphics.WindowWidth * LEFT_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)), 
                                          (int) (Graphics.WindowHeight * TOP_OFFSET * Graphics.GetScaleRatio(GraphicsService.UiScale.Large)) - CONTROL_HEIGHT * _verticalIndex
                                          );
            };
        }

        public void Show() {
            _fadeAnimation?.Cancel();

            this.Visible = true;

            _fadeAnimation = Animation.Tweener.Tween(this, new {Opacity = 1f}, (1f - this.Opacity) / 2);
        }

        public void Hide() {
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
                     $"{DrawUtil.WrapText(this.Font, this.Text, bounds.Width * 0.5f)} [{string.Join(" + ", this.InteractionKeys)}]");
        }

    }
}
