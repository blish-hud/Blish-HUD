using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public abstract class ScrollingButtonContainer : Container {

        private float _shaderRoller = 0;
        public float ShaderRoller {
            get => _shaderRoller;
            set {
                if (_shaderRoller == value) return;

                _shaderRoller = value;

                GetScrollEffect().Parameters["Roller"].SetValue(this.ShaderRoller);
                GetScrollEffect().Parameters["VerticalDraw"].SetValue(GetVerticalDrawPercent());

                this.Invalidate();
            }
        }

        private Effect _scrollEffect;
        private Effect GetScrollEffect() {
            if (_scrollEffect == null) {
                _scrollEffect = Overlay.cm.Load<Effect>(@"effects\menuitem2");
                _scrollEffect.Parameters["Mask"].SetValue(Content.GetTexture("156072"));
                _scrollEffect.Parameters["Overlay"].SetValue(Content.GetTexture("156071"));
            }

            return _scrollEffect;
        }

        private Glide.Tween _shaderAnim;

        private void MouseEnteredContainer(MouseEventArgs e) {
            this.DrawEffect = GetScrollEffect();

            this.ShaderRoller = 0f;
            _shaderAnim = GameService.Animation.Tweener
                                     .Tween(
                                            this,
                                            new { ShaderRoller = 1.0f },
                                            0.6f
                                           );
        }

        private void MouseLeftContainer(MouseEventArgs e) {
            this.DrawEffect = null;

            _shaderAnim?.Cancel();
            _shaderAnim = null;
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);

            if (this.RelativeMousePosition.Y > this.Height * GetVerticalDrawPercent())
                MouseLeftContainer(e);
            else if (!this.MouseOver)
                MouseEnteredContainer(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            MouseLeftContainer(e);
        }

        protected virtual float GetVerticalDrawPercent() {
            return 1f;
        }

    }

}