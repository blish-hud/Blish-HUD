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
                _ButtonIdle = _ButtonIdle ?? Content.GetTexture(@"common\button-states");

                SpriteButtonBorder = SpriteButtonBorder ?? Content.GetTexture("button-border");

                Cached = true;
            }

            public static void Unload() {

                Cached = false;
            }

        }

        protected string _text = "button";
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        public StandardButton() {
            CachedButtonTextures.Load();

            InitAnim();
        }

        private void InitAnim() {
            // TODO: Convert button animation from old animation service to glide library
            _anim = GameService.Animation.Tween(0, 8, ANIM_FRAME_TIME * 9, AnimationService.EasingMethod.Linear);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            _anim?.Start();

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            if (_anim != null) {
                _anim.Reverse();
                _anim.AnimationCompleted += delegate { InitAnim(); };
            }

            base.OnMouseLeft(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnClick(e);
        }

        private int _animFrame = -1;
        private double _animEllapsedTime = 0;

        private const int ATLAS_SPRITE_WIDTH = 350;
        private const int ATLAS_SPRITE_HEIGHT = 20;
        private const int ANIM_FRAME_TIME = 300 / 9;

        private EaseAnimation _anim;

        public override void DoUpdate(GameTime gameTime) {
            if (this.MouseOver) {
                if (_anim == null)
                    _anim = GameService.Animation.Tween(0, 8, ANIM_FRAME_TIME * 9 * (this.Width / ATLAS_SPRITE_WIDTH), AnimationService.EasingMethod.Linear);
            }

            if (_anim != null) {
                _activeAtlasRegion = new Rectangle(_anim.CurrentValueInt * ATLAS_SPRITE_WIDTH, 0, ATLAS_SPRITE_WIDTH, ATLAS_SPRITE_HEIGHT);
                
                if (_anim.Active) Invalidate();
            }

            base.DoUpdate(gameTime);
        }

        private Rectangle _activeAtlasRegion = new Rectangle(0, 0, 350, 20);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Button Texture
            spriteBatch.DrawOnCtrl(this,
                                   CachedButtonTextures.ButtonIdle,
                             new Rectangle(3, 3, _size.X - 6, _size.Y - 5),
                             _activeAtlasRegion);

            // Top Shadow
            spriteBatch.DrawOnCtrl(this, CachedButtonTextures.SpriteButtonBorder,
                             new Rectangle(2, 0, this.Width - 5, 4),
                             new Rectangle(0, 0, 1, 4));

            // Right Shadow
            spriteBatch.DrawOnCtrl(this, CachedButtonTextures.SpriteButtonBorder,
                             new Rectangle(this.Width - 4, 2, 4, this.Height - 3),
                             new Rectangle(0, 1, 4, 1));

            // Bottom Shadow
            spriteBatch.DrawOnCtrl(this, CachedButtonTextures.SpriteButtonBorder,
                             new Rectangle(3, this.Height - 4, this.Width - 6, 4), 
                             new Rectangle(1, 0, 1, 4));

            // Left Shadow
            spriteBatch.DrawOnCtrl(this, CachedButtonTextures.SpriteButtonBorder,
                             new Rectangle(0, 2, 4, this.Height - 3),
                             new Rectangle(0, 3, 4, 1));

            // Button Text
            spriteBatch.DrawStringOnCtrl(
                                         this,
                                         _text,
                                         Content.DefaultFont14,
                                         new Rectangle(Point.Zero, _size), 
                                         Color.Black,
                                         false,
                                         Utils.DrawUtil.HorizontalAlignment.Center
                                        );
        }

    }

}
