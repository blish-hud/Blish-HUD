using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Entities.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities.Effects {
    public class ScrollingTrailEffect : EntityEffect {

        // Per-effect parameters
        private const string PARAMETER_WORLDVIEWPROJECTION  = "WorldViewProjection";
        private const string PARAMETER_PLAYERVIEWPROJECTION = "PlayerViewProjection";
        private const string PARAMETER_PLAYERPOSITION       = "PlayerPosition";
        private const string PARAMETER_TOTALMILLISECONDS    = "TotalMilliseconds";

        private Matrix  _worldViewProjection;
        private Matrix  _playerViewProjection;
        private Vector3 _playerPosition;
        private float   _totalMilliseconds;

        public Matrix WorldViewProjection {
            get => _worldViewProjection;
            set => SetParameter(PARAMETER_WORLDVIEWPROJECTION, ref _worldViewProjection, value);
        }

        public Matrix PlayerViewProjection {
            get => _playerViewProjection;
            set => SetParameter(PARAMETER_PLAYERVIEWPROJECTION, ref _playerViewProjection, value);
        }

        public Vector3 PlayerPosition {
            get => _playerPosition;
            set => SetParameter(PARAMETER_PLAYERPOSITION, ref _playerPosition, value);
        }

        public float TotalMilliseconds {
            get => _totalMilliseconds;
            set => SetParameter(PARAMETER_TOTALMILLISECONDS, ref _totalLength, value);
        }

        // Entity-unique parameters
        private const string PARAMETER_TEXTURE     = "Texture";
        private const string PARAMETER_FLOWSPEED   = "FlowSpeed";
        private const string PARAMETER_FADENEAR    = "FadeNear";
        private const string PARAMETER_FADEFAR     = "FadeFar";
        private const string PARAMETER_OPACITY     = "Opacity";
        private const string PARAMETER_TOTALLENGTH = "TotalLength";

        private Texture2D _texture;
        private float     _flowSpeed;
        private float     _fadeNear, _fadeFar;
        private float     _opacity;
        private float     _totalLength;

        public Texture2D Texture {
            get => _texture;
            set => SetParameter(PARAMETER_TEXTURE, ref _texture, value);
        }

        public float FlowSpeed {
            get => _flowSpeed;
            set => SetParameter(PARAMETER_FLOWSPEED, ref _flowSpeed, value);
        }

        public float FadeNear {
            get => _fadeNear;
            set => SetParameter(PARAMETER_FADENEAR, ref _fadeNear, value);
        }

        public float FadeFar {
            get => _fadeFar;
            set => SetParameter(PARAMETER_FADEFAR, ref _fadeFar, value);
        }

        public float Opacity {
            get => _opacity;
            set => SetParameter(PARAMETER_OPACITY, ref _opacity, value);
        }

        public float TotalLength {
            get => _totalLength;
            set => SetParameter(PARAMETER_TOTALLENGTH, ref _totalLength, value);
        }

        #region ctors

        /// <inheritdoc />
        public ScrollingTrailEffect(Effect cloneSource) : base(cloneSource) { /* NOOP */ }

        /// <inheritdoc />
        public ScrollingTrailEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { /* NOOP */ }

        /// <inheritdoc />
        public ScrollingTrailEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { /* NOOP */ }

        #endregion

        public void SetEntityState(Texture2D texture, float flowSpeed, float fadeNear, float fadeFar, float opacity, float totalLength) {
            this.Texture     = texture;
            this.FlowSpeed   = flowSpeed;
            this.FadeNear    = fadeNear;
            this.FadeFar     = fadeFar;
            this.Opacity     = opacity;
            this.TotalLength = totalLength;
        }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime) {
            this.WorldViewProjection  = GameService.Camera.WorldViewProjection;
            this.PlayerViewProjection = GameService.Camera.PlayerView * GameService.Camera.Projection;
            this.PlayerPosition       = GameService.Player.Position;
            this.TotalMilliseconds    = (float)gameTime.TotalGameTime.TotalMilliseconds;
        }

    }
}
