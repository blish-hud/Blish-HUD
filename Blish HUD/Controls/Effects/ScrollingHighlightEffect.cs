using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls.Effects {

    /// <summary>
    /// Used to show the "scrolling" highlight used by many menu items and buttons throughout the game.
    /// Should be applied as <see cref="Control.EffectBehind"/>.
    /// </summary>
    public class ScrollingHighlightEffect : ControlEffect {

        private const string SPARAM_MASK        = "Mask";
        private const string SPARAM_OVERLAY     = "Overlay";
        private const string SPARAM_ROLLER      = "Roller";
        private const float  ANIMATION_DURATION = 0.5f;

        #region Static Persistant Effect

        private static Effect _scrollingEffect;

        static ScrollingHighlightEffect() {
            _scrollingEffect = Overlay.cm.Load<Effect>(@"effects\menuitem");
            _scrollingEffect.Parameters[SPARAM_MASK].SetValue(GameService.Content.GetTexture("156072"));
            _scrollingEffect.Parameters[SPARAM_OVERLAY].SetValue(GameService.Content.GetTexture("156071"));
        }

        #endregion

        private float _scrollRoller = 0;
        public float ScrollRoller {
            get => _scrollRoller;
            set {
                _scrollRoller = value;
                _scrollingEffect.Parameters[SPARAM_ROLLER].SetValue(_scrollRoller);
            }
        }

        private Glide.Tween _shaderAnim;
        private bool _mouseOver = false;

        public ScrollingHighlightEffect(Control assignedControl) : base(assignedControl) {
            assignedControl.MouseEntered += AssignedControlOnMouseEntered;
            assignedControl.MouseLeft    += AssignedControlOnMouseLeft;
        }

        public override SpriteBatchParameters GetSpriteBatchParameters() {
            return new SpriteBatchParameters(SpriteSortMode.Immediate,
                                             BlendState.AlphaBlend,
                                             SamplerState.LinearWrap,
                                             null,
                                             null,
                                             _scrollingEffect,
                                             GameService.Graphics.UIScaleTransform);
        }

        private void AssignedControlOnMouseEntered(object sender, MouseEventArgs e) {
            if (!this.Enabled) return;

            this.ScrollRoller = 0;

            _shaderAnim = GameService.Animation
                                     .Tweener
                                     .Tween(this,
                                            new {ScrollRoller = 1.0f},
                                            ANIMATION_DURATION);

            _mouseOver = true;
        }

        private void AssignedControlOnMouseLeft(object sender, MouseEventArgs e) {
            _shaderAnim?.Cancel();
            _shaderAnim = null;

            this.ScrollRoller = 0;

            _mouseOver = false;
        }

        protected override void OnEnable() {
            if (this.AssignedControl.MouseOver)
                AssignedControlOnMouseEntered(this.AssignedControl, null);
        }


        protected override void OnDisable() {
            AssignedControlOnMouseLeft(this.AssignedControl, null);
        }


        public override void PaintEffect(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_mouseOver)
                spriteBatch.DrawOnCtrl(this.AssignedControl, ContentService.Textures.Pixel, bounds, Color.Transparent);
        }

    }
}
