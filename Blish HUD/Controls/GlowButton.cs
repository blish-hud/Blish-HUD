using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class GlowButton : Control {

        private const int BUTTON_WIDTH = 32;
        private const int BUTTON_HEIGHT = 32;

        private const int ICON_WIDTH = 32;
        private const int ICON_HEIGHT = 32;

        protected Texture2D _icon;
        public Texture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        protected Color _glowColor = Color.White;
        public Color GlowColor {
            get => _glowColor;
            set {
                if (SetProperty(ref _glowColor, value)) {
                    // Need to update the DrawEffect if it's currently active
                    _glowEffect?.Parameters["GlowColor"]
                                .SetValue(
                                          new Vector4(
                                                      value.R / 255.0f,
                                                      value.G / 255.0f,
                                                      value.B / 255.0f,
                                                      value.A / 255.0f
                                                     )
                                         );
                }
            }
        }

        private static Effect _glowEffect;
        private Effect GetGlowEffect() {
            _glowEffect = _glowEffect ?? Overlay.cm.Load<Effect>(@"effects\glow");
            _glowEffect.Parameters["TextureWidth"].SetValue((float)this.Width);
            _glowEffect.Parameters["GlowColor"].SetValue(
                                                         new Vector4(
                                                                     this.GlowColor.R / 255.0f,
                                                                     this.GlowColor.G / 255.0f,
                                                                     this.GlowColor.B / 255.0f,
                                                                     this.GlowColor.A / 255.0f
                                                                    )
                                                        );

            return _glowEffect;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        public GlowButton() {
            _spriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            this.Size = new Point(BUTTON_WIDTH, BUTTON_HEIGHT);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);

            _spriteBatchParameters.Effect = GetGlowEffect();
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            _spriteBatchParameters.Effect = null;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_icon != null) {
                spriteBatch.DrawOnCtrl(this, _icon, bounds);
            }
        }

    }
}
