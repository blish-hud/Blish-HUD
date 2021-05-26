using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Threading;

namespace Blish_HUD {

    public class BlishHud : Game {

        private static readonly Logger Logger = Logger.GetLogger<BlishHud>();

        #region Internal Members for Services

        /// <summary>
        /// Exposed through the <see cref="GraphicsService"/>'s <see cref="GraphicsService.GraphicsDeviceManager"/>.
        /// </summary>
        internal GraphicsDeviceManager ActiveGraphicsDeviceManager { get; }

        /// <summary>
        /// Exposed through the <see cref="ContentService"/>'s <see cref="ContentService.ContentManager"/>.
        /// </summary>
        internal Microsoft.Xna.Framework.Content.ContentManager ActiveContentManager { get; }

        internal static BlishHud Instance;

        #endregion

        public IntPtr FormHandle { get; private set; }

        public System.Windows.Forms.Form Form { get; private set; }

        // TODO: Move this into GraphicsService
        public RasterizerState UiRasterizer { get; private set; }

        // Primarily used to draw debug text
        private SpriteBatch _basicSpriteBatch;

        public BlishHud() {
            BlishHud.Instance = this;

            this.ActiveGraphicsDeviceManager = new GraphicsDeviceManager(this);
            this.ActiveGraphicsDeviceManager.PreparingDeviceSettings += delegate(object sender, PreparingDeviceSettingsEventArgs args) {
                args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
            };

            this.ActiveGraphicsDeviceManager.GraphicsProfile     = GraphicsProfile.HiDef;
            this.ActiveGraphicsDeviceManager.PreferMultiSampling = true;

            this.ActiveContentManager = this.Content;

            this.Content.RootDirectory = "Content";

            this.IsMouseVisible = true;
        }
        
        protected override void Initialize() {
            FormHandle = this.Window.Handle;
            Form       = System.Windows.Forms.Control.FromHandle(FormHandle).FindForm();

            this.Window.IsBorderless = true;
            this.Window.AllowAltF4   = false;

            // Initialize all game services
            foreach (var service in GameService.All) {
                service.DoInitialize(this);
            }

            base.Initialize();
        }

        protected override void LoadContent() {
            UiRasterizer = new RasterizerState() {
                ScissorTestEnable = true
            };

            // Create a new SpriteBatch, which can be used to draw debug information
            _basicSpriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        protected override void BeginRun() {
            base.BeginRun();

            Logger.Debug("Loading services.");

            // Let all of the game services have a chance to load
            foreach (var service in GameService.All) {
                service.DoLoad();
            }
        }

        protected override void UnloadContent() {
            base.UnloadContent();

            Logger.Debug("Unloading services.");
            
            // Let all of the game services have a chance to unload
            foreach (var service in GameService.All) {
                service.DoUnload();
            }
        }

        protected override void Update(GameTime gameTime) {
            if (gameTime.TotalGameTime.TotalSeconds == 0) {
                Logger.Trace("Skipping first update.");
                // Update is called before the first render
                // Skip to get to the first render as fast as possible
                return;
            }

            // If gw2 isn't open - only update the most important things:
            if (!GameService.GameIntegration.Gw2IsRunning) {
                GameService.Debug.DoUpdate(gameTime);
                GameService.GameIntegration.DoUpdate(gameTime);

                return;
            }

            // Update all game services
            foreach (var service in GameService.All) {
                GameService.Debug.StartTimeFunc($"Service: {service.GetType().Name}");
                service.DoUpdate(gameTime);
                GameService.Debug.StopTimeFunc($"Service: {service.GetType().Name}");
            }

            base.Update(gameTime);

            _drawLag += gameTime.ElapsedGameTime.TotalSeconds;
        }

        private double _drawLag;

        protected override void Draw(GameTime gameTime) {
            GameService.Debug.TickFrameCounter(_drawLag);
            _drawLag = 0;

            if (!GameService.GameIntegration.Gw2IsRunning) return;

            GameService.Graphics.Render(gameTime, _basicSpriteBatch);

            if (ApplicationSettings.Instance.DebugEnabled) {
                _basicSpriteBatch.Begin();

                GameService.Debug.DrawDebugOverlay(_basicSpriteBatch, gameTime);

                _basicSpriteBatch.End();
            }
            
            base.Draw(gameTime);
        }
    }
}
