using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities.Effects {
    public class TrailEffect : Graphics.SharedEffect {

        // Per-effect parameters
        private const string PARAMETER_WORLDVIEWPROJECTION  = "WorldViewProjection";
        private const string PARAMETER_PLAYERVIEW           = "PlayerView";
        private const string PARAMETER_PLAYERPOSITION       = "PlayerPosition";
        private const string PARAMETER_CAMERAPOSITION       = "CameraPosition";
        private const string PARAMETER_TOTALMILLISECONDS    = "TotalMilliseconds";

        private Matrix  _worldViewProjection;
        private Matrix  _playerView;
        private Vector3 _playerPosition;
        private Vector3 _cameraPosition;
        private float   _totalMilliseconds;

        public Matrix WorldViewProjection {
            get => _worldViewProjection;
            set => SetParameter(PARAMETER_WORLDVIEWPROJECTION, ref _worldViewProjection, value);
        }

        public Matrix PlayerView {
            get => _playerView;
            set => SetParameter(PARAMETER_PLAYERVIEW, ref _playerView, value);
        }

        public Vector3 PlayerPosition {
            get => _playerPosition;
            set => SetParameter(PARAMETER_PLAYERPOSITION, ref _playerPosition, value);
        }

        public Vector3 CameraPosition {
            get => _cameraPosition;
            set => SetParameter(PARAMETER_CAMERAPOSITION, ref _cameraPosition, value);
        }

        public float TotalMilliseconds {
            get => _totalMilliseconds;
            set => SetParameter(PARAMETER_TOTALMILLISECONDS, ref _totalMilliseconds, value);
        }

        // Entity-unique parameters
        private const string PARAMETER_TEXTURE          = "Texture";
        private const string PARAMETER_FADETEXTURE      = "FadeTexture";
        private const string PARAMETER_FLOWSPEED        = "FlowSpeed";
        private const string PARAMETER_FADENEAR         = "FadeNear";
        private const string PARAMETER_FADEFAR          = "FadeFar";
        private const string PARAMETER_OPACITY          = "Opacity";
        private const string PARAMETER_TINTCOLOR        = "TintColor";
        private const string PARAMETER_PLAYERFADERADIUS = "PlayerFadeRadius";
        private const string PARAMETER_FADECENTER       = "FadeCenter";

        private Texture2D _texture;
        private Texture2D _fadeTexture;
        private float     _flowSpeed;
        private float     _fadeNear, _fadeFar;
        private float     _opacity;
        private float     _playerFadeRadius;
        private bool      _fadeCenter;
        private Color     _tintColor;

        public Texture2D Texture {
            get => _texture;
            set => SetParameter(PARAMETER_TEXTURE, ref _texture, value);
        }

        public Texture2D FadeTexture {
            get => _texture;
            set => SetParameter(PARAMETER_FADETEXTURE, ref _fadeTexture, value);
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

        public float PlayerFadeRadius {
            get => _playerFadeRadius;
            set => SetParameter(PARAMETER_PLAYERFADERADIUS, ref _playerFadeRadius, value);
        }

        public bool FadeCenter {
            get => _fadeCenter;
            set => SetParameter(PARAMETER_FADECENTER, ref _fadeCenter, value);
        }

        public Color TintColor {
            get => _tintColor;
            set => SetParameter(PARAMETER_TINTCOLOR, ref _tintColor, value);
        }

        #region ctors

        public TrailEffect(Effect cloneSource) : base(cloneSource) { /* NOOP */ }

        public TrailEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { /* NOOP */ }

        public TrailEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { /* NOOP */ }

        #endregion

        public void SetEntityState(Texture2D texture, float flowSpeed, float fadeNear, float fadeFar, float opacity, float playerFadeRadius, bool fadeCenter, Texture2D fadeTexture, Color tintColor) {
            this.Texture          = texture;
            this.FlowSpeed        = flowSpeed;
            this.FadeNear         = fadeNear;
            this.FadeFar          = fadeFar;
            this.Opacity          = opacity;
            this.PlayerFadeRadius = playerFadeRadius;
            this.FadeCenter       = fadeCenter;
            this.TintColor        = tintColor;
            this.FadeTexture      = fadeTexture;
        }

        protected override void Update(GameTime gameTime) {
            this.TotalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;
            this.PlayerPosition    = GameService.Gw2Mumble.PlayerCharacter.Position;
            this.CameraPosition    = GameService.Gw2Mumble.PlayerCamera.Position;

            // TODO: Move to Graphics pipeline
            this.WorldViewProjection = GameService.Gw2Mumble.PlayerCamera.WorldViewProjection;
            this.PlayerView          = GameService.Gw2Mumble.PlayerCamera.PlayerView;
        }

    }
}
