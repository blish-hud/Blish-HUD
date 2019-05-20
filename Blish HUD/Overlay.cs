using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace Blish_HUD {

    public class Overlay : Game {

        public readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // TODO: Move this (FrameCounter) into the debug service or something
        private FrameCounter _frameCounter = new FrameCounter();

        public static ContentManager cm;

        public Overlay() {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += delegate(object sender, PreparingDeviceSettingsEventArgs args) {
                args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
            };

            this.Content.RootDirectory = "Content";

            cm = this.Content;

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferMultiSampling = true;

            this.IsMouseVisible = true;
        }

        public static System.Windows.Forms.Form Form;
        public static System.Windows.Forms.Label UnfocusLabel;

        public static IntPtr WinHandle;

        public static BitmapFont font_consolas;

        //private GameService[] gameServices;
        
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            System.Windows.Forms.Application.EnableVisualStyles();

            ///  ///

            WinHandle = this.Window.Handle;
            var ctrl = System.Windows.Forms.Control.FromHandle(WinHandle);
            Form = ctrl.FindForm();
            //Form.ShowInTaskbar = false;

            // TODO: Move this into the "Textbox" control class as a lazy-loaded static var
            // This is needed to ensure that the textbox is *actually* unfocused
            UnfocusLabel = new System.Windows.Forms.Label {
                Location = new System.Drawing.Point(-200, 0),
                Parent   = Form
            };

            this.Window.IsBorderless = true;
            this.Window.AllowAltF4 = false;

#if DEBUG
            graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep                    = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
#endif

            // Initialize all game services
            foreach (var service in GameService.All)
                service.DoInitialize(this);

            base.Initialize();
        }

        protected override void LoadContent() {
            _uiRasterizer = new RasterizerState() {
                ScissorTestEnable = true
            };

            BHGw2Api.Settings.Load();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // Let all of the game services have a chance to unload
            foreach (var service in GameService.All)
                service.DoUnload();
        }

        protected override void BeginRun() {
            base.BeginRun();

            // Let all of the game services have a chance to load
            foreach (var service in GameService.All)
                service.DoLoad();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
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
        }

        public static RasterizerState _uiRasterizer;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            if (!GameService.GameIntegration.Gw2IsRunning) return;
            
            this.GraphicsDevice.BlendState = BlendState.Opaque;
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            this.GraphicsDevice.Clear(Color.Transparent);

            GameService.Debug.StartTimeFunc("3D objects");
            // Only draw 3D elements if we are in game
            if (GameService.GameIntegration.IsInGame)
                GameService.Graphics.World.Draw(this.GraphicsDevice);
            GameService.Debug.StopTimeFunc("3D objects");

            GameService.Debug.StartTimeFunc("UI Elements");
            if (GameService.Graphics.SpriteScreen != null && GameService.Graphics.SpriteScreen.Visible) {
                GameService.Graphics.SpriteScreen.Draw(spriteBatch, GameService.Graphics.SpriteScreen.LocalBounds, GameService.Graphics.SpriteScreen.LocalBounds);
            }
            GameService.Debug.StopTimeFunc("UI Elements");


#if DEBUG
            spriteBatch.Begin();

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameCounter.Update(deltaTime);

            var fps = string.Format("FPS: {0}", Math.Round(_frameCounter.AverageFramesPerSecond, 0));

            int debugLeft = GameService.Graphics.WindowWidth - 750;

            spriteBatch.DrawString(GameService.Content.DefaultFont14, fps, new Vector2(debugLeft, 25), Color.Red);

            int i = 0;
            foreach (KeyValuePair<string, DebugService.FuncClock> timedFuncPair in GameService.Debug.FuncTimes.Where(ft => ft.Value.AverageRuntime > 1).OrderByDescending(ft => ft.Value.AverageRuntime)) {
                spriteBatch.DrawString(GameService.Content.DefaultFont14, $"{timedFuncPair.Key} {Math.Round(timedFuncPair.Value.AverageRuntime)} ms", new Vector2(debugLeft, 50 + (i * 25)), Color.Orange);
                i++;
            }

            spriteBatch.DrawString(GameService.Content.DefaultFont14, $"Pathables Available: {GameService.Pathing.Pathables.Count}", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            i++;
            spriteBatch.DrawString(GameService.Content.DefaultFont14, $"3D Entities Displayed: {GameService.Graphics.World.Entities.Count}", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            //i++;
            //spriteBatch.DrawString(GameService.Content.DefaultFont14, $"Controls Displayed: {GameService.Graphics.SpriteScreen.GetDescendants().Count}", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            i++;
            spriteBatch.DrawString(GameService.Content.DefaultFont14, "Render Late: " + (gameTime.IsRunningSlowly ? "Yes" : "No"), new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);

#endif

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
