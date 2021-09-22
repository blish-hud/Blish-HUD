using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

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

        public Form Form { get; private set; }

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
            Form       = Control.FromHandle(FormHandle).FindForm();

            // Avoid the flash the window shows when the application launches
            Form.Size     = new Size(1, 1);
            Form.Location = new System.Drawing.Point(-Form.Width * 2, -Form.Height * 2);

            this.Window.IsBorderless = true;
            this.Window.AllowAltF4   = false;
            this.InactiveSleepTime   = TimeSpan.Zero;

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
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning) {
                // If gw2 isn't open so only run the essentials
                GameService.Debug.DoUpdate(gameTime);
                GameService.GameIntegration.DoUpdate(gameTime);

                for (int i = 0; i < 200; i++) { // Wait ~10 seconds between checks
                    if (GameService.GameIntegration.Gw2Instance.Gw2IsRunning || GameService.Overlay.Exiting) break;
                    Thread.Sleep(50);
                    Application.DoEvents();
                }

                return;
            }

            // Update all game services
            foreach (var service in GameService.All) {
                GameService.Debug.StartTimeFunc($"Service: {service.GetType().Name}");
                service.DoUpdate(gameTime);
                GameService.Debug.StopTimeFunc($"Service: {service.GetType().Name}");
            }

            base.Update(gameTime);

            _drawLag += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private float _drawLag;

        private bool _skipDraw = false;

        internal void SkipDraw() {
            _skipDraw = true;
        }

        protected override void Draw(GameTime gameTime) {
            if (_skipDraw) {
                Thread.Sleep(1);
                _skipDraw = false;
                return;
            }

            GameService.Debug.TickFrameCounter(_drawLag);
            _drawLag = 0;

            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning) return;

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
