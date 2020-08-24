using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities.Effects {
    public class MarkerEffect : Blish_HUD.Entities.Effects.EntityEffect {

        // Per-effect parameters
        private const string PARAMETER_VIEW           = "View";
        private const string PARAMETER_PROJECTION     = "Projection";
        private const string PARAMETER_PLAYERPOSITION = "PlayerPosition";

        private Matrix  _view, _projection;
        private Vector3 _playerPosition;

        public Matrix View {
            get => _view;
            set => SetParameter(PARAMETER_VIEW, ref _view, value);
        }

        public Matrix Projection {
            get => _projection;
            set => SetParameter(PARAMETER_PROJECTION, ref _projection, value);
        }

        public Vector3 PlayerPosition {
            get => _playerPosition;
            set => SetParameter(PARAMETER_PLAYERPOSITION, ref _playerPosition, value);
        }

        // Entity-unique parameters
        private const string PARAMETER_WORLD   = "World";
        private const string PARAMETER_TEXTURE = "Texture";
        private const string PARAMETER_OPACITY = "Opacity";

        private const string PARAMETER_FADENEAR = "FadeNear";
        private const string PARAMETER_FADEFAR  = "FadeFar";

        private const string PARAMETER_TINTCOLOR = "TintColor";

        private Matrix    _world;
        private Texture2D _texture;
        private float     _opacity;
        private float     _fadeNear, _fadeFar;
        private Color     _tintColor;

        public Matrix World {
            get => _world;
            set => SetParameter(PARAMETER_WORLD, ref _world, value);
        }

        public Texture2D Texture {
            get => _texture;
            set => SetParameter(PARAMETER_TEXTURE, ref _texture, value);
        }

        public float Opacity {
            get => _opacity;
            set => SetParameter(PARAMETER_OPACITY, ref _opacity, value);
        }

        public float FadeNear {
            get => _fadeNear;
            set => SetParameter(PARAMETER_FADENEAR, ref _fadeNear, value);
        }

        public float FadeFar {
            get => _fadeFar;
            set => SetParameter(PARAMETER_FADEFAR, ref _fadeFar, value);
        }

        public Color TintColor {
            get => _tintColor;
            set => SetParameter(PARAMETER_TINTCOLOR, ref _tintColor, value);
        }

        #region ctors

        public MarkerEffect(Effect baseEffect) : base(baseEffect) { }

        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { }

        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { }

        #endregion

        public void SetEntityState(Matrix world, Texture2D texture, float opacity, float fadeNear, float fadeFar, Color tintColor) {
            this.World     = world;
            this.Texture   = texture;
            this.Opacity   = opacity;
            this.FadeNear  = fadeNear;
            this.FadeFar   = fadeFar;
            this.TintColor = tintColor;
        }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime) {
            this.PlayerPosition = GameService.Gw2Mumble.PlayerCharacter.Position;

            // TODO: Move to Graphics pipeline
            this.View       = GameService.Gw2Mumble.PlayerCamera.View;
            this.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
        }

    }
}
