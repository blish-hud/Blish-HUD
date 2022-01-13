using Blish_HUD.Content;
using Blish_HUD.Controls;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace Blish_HUD.Overlay.SelfUpdater.Controls {
    internal class BouncyNotification : Control {

        // Consider pivoting this to a common control with support for showing multiple.

        private const int   BOUNCE_COUNT    = 15;
        private const float BOUNCE_DURATION = 1f;
        private const float BOUNCE_DELAY    = 1.4f;

        private const float BOUNCE_ROTATION = -MathHelper.PiOver4 / 4;

        private static readonly Texture2D _shineTexture = GameService.Content.GetTexture("controls/bouncynotification/965696");

        private AsyncTexture2D _chestTexture;
        public AsyncTexture2D ChestTexture {
            get => _chestTexture;
            set => SetProperty(ref _chestTexture, value);
        }

        private AsyncTexture2D _openChestTexture;
        public AsyncTexture2D OpenChestTexture {
            get => _openChestTexture;
            set => SetProperty(ref _openChestTexture, value);
        }

        private bool _chestOpen = false;
        public bool ChestOpen {
            get => _chestOpen;
            set => SetProperty(ref _chestOpen, value);
        }

        private int   _wiggleDirection = 1;
        private bool  _nonOpp          = false;
        private float _rotation        = 0f;

        public BouncyNotification(AsyncTexture2D chestTexture, AsyncTexture2D openChestTexture = null) {
            _chestTexture     = chestTexture;
            _openChestTexture = openChestTexture;

            this.Size             = new Point(64, 64);
            this.ClipsBounds      = false;

            DoWiggle();
        }

        private async Task DoWiggle() {
            _nonOpp = !_nonOpp;
            _wiggleDirection = 1;
            await Task.Delay(TimeSpan.FromSeconds(BOUNCE_DELAY));
            GameService.Animation.Tweener.Tween(this, new { _rotation = BOUNCE_ROTATION }, BOUNCE_DURATION / BOUNCE_COUNT)
                .Reflect()
                .Repeat(BOUNCE_COUNT)
                .Ease(Ease.BounceInOut)
                .Rotation(Tween.RotationUnit.Radians) 
                // Almost certainly a better way to do this if I thought about it for a bit longer
                .OnRepeat(() => _wiggleDirection *= (_nonOpp = !_nonOpp) ? -1 : 1) 
                .OnComplete(DoWiggle);
        }

        public override void DoUpdate(GameTime gameTime) {
            this.Location = new Point(GameService.Graphics.SpriteScreen.Width - this.Width - 24 /* Distance from right edge */, (GameService.Gw2Mumble.UI.IsCompassTopRight 
                                                                                                     /* COMPASS TOP    RIGHT */ ? GameService.Graphics.SpriteScreen.Height - 24 /* Distance from bottom edge */
                                                                                                     /* COMPASS BOTTOM RIGHT */ : GameService.Graphics.SpriteScreen.Height - 35 /* Distance from bottom edge of map */ - (int)(GameService.Gw2Mumble.UI.CompassSize.Height / 0.897f))
                                                                                                                              - this.Height - 64 /* Above actual bouncy chests */ - 12 /* Buffer from other bouncy chests */);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame) {
                // Only show when we're in game. This is a deliberate form over function choice.
                return;
            }

            spriteBatch.DrawOnCtrl(this, _shineTexture, bounds.ScaleBy(1.5f).OffsetBy(bounds.Width / 2, bounds.Height / 2), null, Color.White * 0.8f, (float)GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds * -1.3f, _shineTexture.Bounds.Size.ToVector2() / 2);
            spriteBatch.DrawOnCtrl(this, this.MouseOver || this.ChestOpen ? this.OpenChestTexture : this.ChestTexture, bounds.OffsetBy(bounds.Width / 2, bounds.Height / 2), null, Color.White, this.ChestOpen ? 0 : _rotation * _wiggleDirection, this.ChestTexture.Texture.Bounds.Size.ToVector2() / 2);
        }

    }
}
