using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {

    public class StandardButton:Control {

        static class CachedButtonTextures {

            private static bool Cached = false;

            private static Texture2D _ButtonIdle;
            public static Texture2D ButtonIdle {
                get {
                    if (!Cached) Load();

                    return _ButtonIdle;
                }
            }

            public static Texture2D SpriteButtonBorder;

            public static void Load() {
                _ButtonIdle = _ButtonIdle ?? GameService.Content.GetTexture(@"common\button-states");

                SpriteButtonBorder = SpriteButtonBorder ?? Content.GetTexture("button-border");

                Cached = true;
            }

            public static void Unload() {

                Cached = false;
            }

        }

        private string _text = "button";
        public string Text {
            get => _text;
            set {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged();
            }
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        public StandardButton() {
            CachedButtonTextures.Load();

            InitAnim();
        }

        private void InitAnim() {
            anim = GameService.Animation.Tween(0, 8, ANIM_FRAME_TIME * 9, AnimationService.EasingMethod.Linear);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);

            anim?.Start();
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            if (anim != null) {
                anim.Reverse();
                anim.AnimationCompleted += delegate { InitAnim(); };
            }
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            Content.PlaySoundEffectByName(@"audio\button-click");
        }

        private int AnimFrame = -1;
        private double AnimEllapsedTime = 0;

        private const int ATLAS_SPRITE_WIDTH = 350;
        private const int ATLAS_SPRITE_HEIGHT = 20;
        private const int ANIM_FRAME_TIME = 300 / 9;

        private EaseAnimation anim;

        public override void Update(GameTime gameTime) {
            if (this.MouseOver) {
                if (anim == null)
                    anim = GameService.Animation.Tween(0, 8, ANIM_FRAME_TIME * 9 * (this.Width / ATLAS_SPRITE_WIDTH), AnimationService.EasingMethod.Linear);
            }

            if (anim != null) {
                ActiveAtlasRegion = new Rectangle(anim.CurrentValueInt * ATLAS_SPRITE_WIDTH, 0, ATLAS_SPRITE_WIDTH, ATLAS_SPRITE_HEIGHT);
                
                if (anim.Active) Invalidate();
            }

            base.Update(gameTime);
        }

        private Rectangle ActiveAtlasRegion = new Rectangle(0, 0, 350, 20);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Button Texture
            spriteBatch.Draw(CachedButtonTextures.ButtonIdle, bounds.Add(3, 3, -6, -5), ActiveAtlasRegion, Color.White);

            // Top Shadow
            spriteBatch.Draw(CachedButtonTextures.SpriteButtonBorder, new Rectangle(2, 0, this.Width - 5, 4), new Rectangle(0, 0, 1, 4), Color.White);

            // Right Shadow
            spriteBatch.Draw(CachedButtonTextures.SpriteButtonBorder, new Rectangle(this.Width - 4, 2, 4, this.Height - 3), new Rectangle(0, 1, 4, 1), Color.White);

            // Bottom Shadow
            spriteBatch.Draw(CachedButtonTextures.SpriteButtonBorder, new Rectangle(3, this.Height - 4, this.Width - 6, 4), new Rectangle(1, 0, 1, 4), Color.White);

            // Left Shadow
            spriteBatch.Draw(CachedButtonTextures.SpriteButtonBorder, new Rectangle(0, 2, 4, this.Height - 3), new Rectangle(0, 3, 4, 1), Color.White);

            // Button Text
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular), this.Text, bounds, Color.Black, Utils.DrawUtil.HorizontalAlignment.Center, Utils.DrawUtil.VerticalAlignment.Middle);
        }

    }

}
