using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    /*
    public class ScrollingHighlight : Control {
        
        private float _shaderRoller = 0;
        public float ShaderRoller {
            get => _shaderRoller;
            set {
                if (SetProperty(ref _shaderRoller, value)) {
                    GetScrollEffect().Parameters["Roller"].SetValue(this.ShaderRoller);
                }
            }
        }

        private Effect _scrollEffect;
        private Effect GetScrollEffect() {
            if (_scrollEffect == null) {
                _scrollEffect = Overlay.cm.Load<Effect>(@"effects\menuitem");
                _scrollEffect.Parameters["Mask"].SetValue(Content.GetTexture("156072"));
                _scrollEffect.Parameters["Overlay"].SetValue(Content.GetTexture("156071"));
            }

            return _scrollEffect;
        }

        private Glide.Tween _shaderAnim;

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);

            this.DrawEffect = GetScrollEffect();

            this.ShaderRoller = 0f;
            _shaderAnim = GameService.Animation.Tweener
                                     .Tween(
                                            this,
                                            new { ShaderRoller = 1.0f },
                                            0.6f
                                           );
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            this.DrawEffect = null;
            
            _shaderAnim?.Cancel();
            _shaderAnim = null;

            this.ShaderRoller = 0;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_enabled)
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Transparent);
        }

    }
    */
}
