using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Screen = Blish_HUD.Controls.Screen;

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

                float uiScale = GetScaleRatio(value);
                this.SpriteScreen.Size = new Point((int)(Overlay.graphics.PreferredBackBufferWidth / uiScale), (int)(Overlay.graphics.PreferredBackBufferHeight / uiScale));
            }
        }

        private Controls.Screen _screen;
        public Controls.Screen SpriteScreen => _screen;

        // TODO: This needs separated out to a different service specific to 3D entities
        // Or maybe not?
        public Entities.World World { get; private set; }

        public GraphicsDevice GraphicsDevice => Overlay.GraphicsDevice;
        public GraphicsDeviceManager GraphicsDeviceManager => Overlay.graphics;

        public int WindowWidth => this.GraphicsDevice.Viewport.Width;
        public int WindowHeight => this.GraphicsDevice.Viewport.Height;

        public Point Resolution {
            get => new Point(Overlay.graphics.PreferredBackBufferWidth, Overlay.graphics.PreferredBackBufferHeight);
            set {
                Overlay.graphics.PreferredBackBufferWidth = value.X;
                Overlay.graphics.PreferredBackBufferHeight = value.Y;
                
                Overlay.graphics.ApplyChanges();

                float uiScale = GetScaleRatio(this.UIScale);
                this.SpriteScreen.Size = new Point((int)(value.X / uiScale), (int)(value.Y / uiScale));
            }
        }

        protected override void Initialize() {
            _screen = new Controls.Screen();
            this.World = new Entities.World();

            // If for some reason we lose the rendering device, just restart the application
            // Might do better error handling later on
            Overlay.GraphicsDevice.DeviceLost += delegate { Application.Restart(); };
            //MainLoop.Form.Resize += delegate { this.Resolution = new Point(MainLoop.Form.Size.Width, MainLoop.Form.Size.Height); };
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            this.SpriteScreen.Update(gameTime);
            this.World.Update(gameTime);
        }
    }
}
