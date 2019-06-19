using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = Microsoft.Xna.Framework.Point;

namespace Blish_HUD {
    public class GraphicsService:GameService {

        #region Load Static

        private static readonly Screen _spriteScreen;
        private static readonly World  _world;

        static GraphicsService() {
            _spriteScreen = new Screen();
            _world        = new World();
        }

        #endregion

        public enum UiScale {
            Small,  // 47 x 47
            Normal, // 52 x 52
            Large,  // 58 x 58
            Larger  // 64 x 64
        }

        public float GetScaleRatio(UiScale currScale) {
            switch (currScale) {
                case UiScale.Small:
                    return 0.810f;
                case UiScale.Normal:
                    return 0.897f;
                case UiScale.Large:
                    return 1f;
                case UiScale.Larger:
                    return 1.103f;
            }

            return 1f;
        }

        private UiScale _uiScale = UiScale.Normal;
        public UiScale UIScale {
            get => _uiScale;
            set {
                if (_uiScale == value) return;

                _uiScale = value;

                _uiScaleMultiplier = GetScaleRatio(value);
                this.SpriteScreen.Size = new Point((int)(Overlay.ActiveGraphicsDeviceManager.PreferredBackBufferWidth / _uiScaleMultiplier), (int)(Overlay.ActiveGraphicsDeviceManager.PreferredBackBufferHeight / _uiScaleMultiplier));

                _uiScaleTransform = Matrix.CreateScale(_uiScaleMultiplier);
            }
        }

        private Matrix _uiScaleTransform = Matrix.Identity;
        public Matrix UIScaleTransform => _uiScaleTransform;

        private float _uiScaleMultiplier = 1f;
        public float UIScaleMultiplier => _uiScaleMultiplier;

        public Controls.Screen SpriteScreen => _spriteScreen;

        public Entities.World World => _world;

        public GraphicsDeviceManager GraphicsDeviceManager => Overlay.ActiveGraphicsDeviceManager;

        public GraphicsDevice GraphicsDevice => Overlay.ActiveGraphicsDeviceManager.GraphicsDevice;

        public int WindowWidth => this.GraphicsDevice.Viewport.Width;
        public int WindowHeight => this.GraphicsDevice.Viewport.Height;

        private float _aspectRatio;
        public  float AspectRatio => _aspectRatio;

        public Point Resolution {
            get => new Point(Overlay.ActiveGraphicsDeviceManager.PreferredBackBufferWidth, Overlay.ActiveGraphicsDeviceManager.PreferredBackBufferHeight);
            set {
                try {
                    Overlay.ActiveGraphicsDeviceManager.PreferredBackBufferWidth  = value.X;
                    Overlay.ActiveGraphicsDeviceManager.PreferredBackBufferHeight = value.Y;

                    Overlay.ActiveGraphicsDeviceManager.ApplyChanges();

                    // Exception would be from the code above, but don't update our
                    // scaling if there is an exception
                    ScreenSizeUpdated(value);
                } catch (SharpDXException sdxe) {
                    // If device lost, we should hopefully handle in device lost event below
                }
            }
        }

        private void ScreenSizeUpdated(Point newSize) {
            // Update the SpriteScreen
            this.SpriteScreen.Size = new Point((int)(newSize.X / this.UIScaleMultiplier), (int)(newSize.Y / this.UIScaleMultiplier));

            // Update the aspect ratio
            _aspectRatio = (float)Graphics.WindowWidth / (float)Graphics.WindowHeight;
        }

        protected override void Initialize() {
            // If for some reason we lose the rendering device, just restart the application
            // Might do better error handling later on
            ActiveOverlay.GraphicsDevice.DeviceLost += delegate { System.Windows.Forms.Application.Restart(); };

            _uiScaleMultiplier = GetScaleRatio(this.UIScale);
            _uiScaleTransform  = Matrix.CreateScale(Graphics.UIScaleMultiplier);
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            this.World.Update(gameTime);
            this.SpriteScreen.Update(gameTime);
        }
    }
}
