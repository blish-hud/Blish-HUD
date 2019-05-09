using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls.Effects {
    public class GlowEffect : ControlEffect {

        private const string SPARAM_TEXTUREWIDTH = "TextureWidth";
        private const string SPARAM_GLOWCOLOR = "GlowColor";

        #region Static Persistant Effect

        private static Effect _glowEffectReference;

        static GlowEffect() {
            _glowEffectReference = Overlay.cm.Load<Effect>(@"effects\glow");
            _glowEffectReference.Parameters[SPARAM_GLOWCOLOR].SetValue(Color.White.ToVector4());
        }

        #endregion

        private Effect _glowEffect;

        private Color _glowColor = Color.White;
        public Color GlowColor {
            get => _glowColor;
            set {
                if (_glowColor == value) return;

                _glowColor = value;

                _glowEffect?.Parameters["GlowColor"].SetValue(_glowColor.ToVector4());
            }
        }


        public GlowEffect(Control assignedControl) : base(assignedControl) {
            _glowEffect = _glowEffectReference.Clone();
        }

        public override SpriteBatchParameters GetSpriteBatchParameters() {
            return new SpriteBatchParameters(SpriteSortMode.Deferred,
                                             BlendState.AlphaBlend,
                                             SamplerState.LinearWrap,
                                             null,
                                             null,
                                             _glowEffect,
                                             GameService.Graphics.UIScaleTransform);
        }

        public override void PaintEffect(SpriteBatch spriteBatch, Rectangle bounds) {
            
        }

    }
}
