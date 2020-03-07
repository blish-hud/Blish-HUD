using System;
using System.Collections.Concurrent;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            Small  = 0, // 47 x 47
            Normal = 1, // 52 x 52
            Large  = 2, // 58 x 58
            Larger = 3  // 64 x 64
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
                this.SpriteScreen.Size = new Point((int)(BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferWidth / _uiScaleMultiplier), (int)(BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferHeight / _uiScaleMultiplier));

                _uiScaleTransform = Matrix.CreateScale(_uiScaleMultiplier);
            }
        }

        private Matrix _uiScaleTransform = Matrix.Identity;
        public Matrix UIScaleTransform => _uiScaleTransform;

        private float _uiScaleMultiplier = 1f;
        public float UIScaleMultiplier => _uiScaleMultiplier;

        public Screen SpriteScreen => _spriteScreen;

        public World World => _world;

        public GraphicsDeviceManager GraphicsDeviceManager => BlishHud.ActiveGraphicsDeviceManager;

        public GraphicsDevice GraphicsDevice => BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice;

        public int WindowWidth => this.GraphicsDevice.Viewport.Width;
        public int WindowHeight => this.GraphicsDevice.Viewport.Height;

        private float _aspectRatio;
        public  float AspectRatio => _aspectRatio;

        public Point Resolution {
            get => new Point(BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferWidth, BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferHeight);
            set {
                try {
                    BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferWidth  = value.X;
                    BlishHud.ActiveGraphicsDeviceManager.PreferredBackBufferHeight = value.Y;

                    BlishHud.ActiveGraphicsDeviceManager.ApplyChanges();

                    // Exception would be from the code above, but don't update our
                    // scaling if there is an exception
                    ScreenSizeUpdated(value);
                } catch (SharpDX.SharpDXException sdxe) {
                    // If device lost, we should hopefully handle in device lost event below
                }
            }
        }

        private readonly ConcurrentQueue<Action<GraphicsDevice>> _queuedRenders = new ConcurrentQueue<Action<GraphicsDevice>>();

        /// <summary>
        /// Allows you to enqueue a call that will occur during the next time the update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see="GameTime" /> as a parameter.</param>
        public void QueueMainThreadRender(Action<GraphicsDevice> call) {
            _queuedRenders.Enqueue(call);
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
            ActiveBlishHud.GraphicsDevice.DeviceLost += delegate { System.Windows.Forms.Application.Restart(); };

            _uiScaleMultiplier = GetScaleRatio(this.UIScale);
            _uiScaleTransform  = Matrix.CreateScale(Graphics.UIScaleMultiplier);
        }

        internal void Render(GameTime gameTime, SpriteBatch spriteBatch) {
            this.GraphicsDevice.Clear(Color.Transparent);

            GameService.Debug.StartTimeFunc("3D objects");
            // Only draw 3D elements if we are in game
            if (GameService.GameIntegration.IsInGame && (!GameService.ArcDps.RenderPresent || GameService.ArcDps.HudIsActive))
                this.World.DoDraw(this.GraphicsDevice);
            GameService.Debug.StopTimeFunc("3D objects");

            // Slightly better scaling (text is a bit more legible)
            this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            GameService.Debug.StartTimeFunc("UI Elements");
            if (this.SpriteScreen != null && this.SpriteScreen.Visible) {
                this.SpriteScreen.Draw(spriteBatch, this.SpriteScreen.LocalBounds, this.SpriteScreen.LocalBounds);
            }
            GameService.Debug.StopTimeFunc("UI Elements");

            GameService.Debug.StartTimeFunc("Render Queue");
            if (this._queuedRenders.TryDequeue(out var renderCall)) {
                renderCall.Invoke(this.GraphicsDevice);
            }
            GameService.Debug.StopTimeFunc("Render Queue");
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            this.World.DoUpdate(gameTime);
            Entities.Effects.EntityEffect.UpdateEffects(gameTime);
            this.SpriteScreen.Update(gameTime);
        }
    }
}
