using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = Microsoft.Xna.Framework.Point;

namespace Blish_HUD {
    public class GraphicsService:GameService {

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
                this.SpriteScreen.Size = new Point((int)(Overlay.graphics.PreferredBackBufferWidth / _uiScaleMultiplier), (int)(Overlay.graphics.PreferredBackBufferHeight / _uiScaleMultiplier));

                _uiScaleTransform = Matrix.CreateScale(_uiScaleMultiplier);

            }
        }

        private Matrix _uiScaleTransform = Matrix.Identity;
        public Matrix UIScaleTransform => _uiScaleTransform;

        private float _uiScaleMultiplier = 1f;
        public float UIScaleMultiplier => _uiScaleMultiplier;

        private Controls.Screen _screen;
        public Controls.Screen SpriteScreen => _screen;

        private Entities.World _world;
        public Entities.World World => _world;

        public GraphicsDevice GraphicsDevice => Overlay.GraphicsDevice;
        public GraphicsDeviceManager GraphicsDeviceManager => Overlay.graphics;

        public int WindowWidth => this.GraphicsDevice.Viewport.Width;
        public int WindowHeight => this.GraphicsDevice.Viewport.Height;

        public Point Resolution {
            get => new Point(Overlay.graphics.PreferredBackBufferWidth, Overlay.graphics.PreferredBackBufferHeight);
            set {
                try {
                    Overlay.graphics.PreferredBackBufferWidth  = value.X;
                    Overlay.graphics.PreferredBackBufferHeight = value.Y;

                    Overlay.graphics.ApplyChanges();

                    // Exception would be from the code above, but don't update our scaling if there is an exception
                    this.SpriteScreen.Size = new Point((int) (value.X / this.UIScaleMultiplier), (int) (value.Y / this.UIScaleMultiplier));
                } catch (SharpDXException sdxe) {
                    // If device lost, we should hopefully handle in device lost event below
                }
            }
        }

        protected override void Initialize() {
            _screen = new Controls.Screen();
            _world  = new Entities.World();

            // If for some reason we lose the rendering device, just restart the application
            // Might do better error handling later on
            Overlay.GraphicsDevice.DeviceLost += delegate { Application.Restart(); };

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
