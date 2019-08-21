using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities.Effects {
    public class MarkerEffect : Blish_HUD.Entities.Effects.EntityEffect {

        // Per-effect parameters
        private const string PARAMETER_PLAYERPOSITION = "PlayerPosition";

        // Entity-unique parameters
        private const string PARAMETER_TEXTURE = "Texture";
        private const string PARAMETER_OPACITY = "Opacity";

        private const string PARAMETER_FADENEAR = "FadeNear";
        private const string PARAMETER_FADEFAR  = "FadeFar";

        // Per-entity parameter
        private Texture2D _texture;
        private float     _opacity;
        private float     _fadeNear, _fadeFar;

        public Texture2D Texture {
            set {
                if (SetProperty(ref _texture, value)) {
                    this.Parameters[PARAMETER_TEXTURE].SetValue(_texture);
                }
            }
        }

        public float Opacity {
            set {
                if (SetProperty(ref _opacity, value)) {
                    this.Parameters[PARAMETER_OPACITY].SetValue(_opacity);
                }
            }
        }

        public float FadeNear {
            set {
                if (SetProperty(ref _fadeNear, value)) {
                    this.Parameters[PARAMETER_FADENEAR].SetValue(_fadeNear);
                }
            }
        }

        public float FadeFar {
            set {
                if (SetProperty(ref _fadeFar, value)) {
                    this.Parameters[PARAMETER_FADEFAR].SetValue(_fadeFar);
                }
            }
        }

        #region ctors

        /// <inheritdoc />
        public MarkerEffect(Effect baseEffect) : base(baseEffect) { }

        /// <inheritdoc />
        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { }

        /// <inheritdoc />
        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { }

        #endregion

        public void SetEntityState(Matrix world, Texture2D texture, float opacity, float fadeNear, float fadeFar) {
            this.World    = world;
            this.Texture  = texture;
            this.Opacity  = opacity;
            this.FadeNear = fadeNear;
            this.FadeFar  = fadeFar;
        }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime) {
            this.Parameters[PARAMETER_PLAYERPOSITION].SetValue(GameService.Player.Position);
        }

    }
}
