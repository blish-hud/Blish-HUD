using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Compatibility.TacO;
using Blish_HUD;
using Humanizer;
using Microsoft.Xna.Framework.Content;

namespace Blish_HUD {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
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

            font_def12 = this.Content.Load<SpriteFont>("common\\menomonia");
            font_consolas = this.Content.Load<BitmapFont>("fonts\\menomonia\\menomonia-11-regular");

            WinHandle = this.Window.Handle;
            var ctrl = System.Windows.Forms.Control.FromHandle(WinHandle);
            Form = ctrl.FindForm();

            // TODO: Move this into the "Textbox" control class as a lazy-loaded static var
            // This is needed to ensure that the textbox is *actually* unfocused
            UnfocusLabel = new System.Windows.Forms.Label {
                Location = new System.Drawing.Point(-200, 0),
                Parent   = Form
            };

            this.Window.IsBorderless = true;
            this.Window.AllowAltF4 = false;

            //Form.ShowInTaskbar = false;

            //var js = Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings { Formatting = Newtonsoft.Json.Formatting.Indented });
            //var sw = new Newtonsoft.Json.JsonTextWriter()
            //js.Serialize(sw, Blish_HUD.Services.Services.UI.Screen);

            //Newtonsoft.Json.JsonSerializerSettings d = new Newtonsoft.Json.JsonSerializerSettings() {
            //    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            //    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
            //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            //};

            //var j = Newtonsoft.Json.JsonConvert.SerializeObject(Blish_HUD.Services.Services.UI.Screen.GetDescendants(), Newtonsoft.Json.Formatting.Indented, d);
            //File.WriteAllText("views\\screen.json", j);

            //var s = File.ReadAllText("views\\screen.json");
            //Blish_HUD.Services.Services.UI.Screen.Children.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Control>>(s, d));

#if DEBUG
            graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
#endif
            //TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);

            //QueensdaleRoute = Markers.AugTyrRoute.FromFile("15.json");

            //List<VertexPositionColor> tempAug = new List<VertexPositionColor>();

            //foreach (Markers.AugTyrNode node in QueensdaleRoute.Nodes) {
            //    tempAug.Add(new VertexPositionColor(new Vector3(node.X, node.Z, node.Y), Color.Magenta));
            //}

            //routeTest = tempAug.ToArray();

            //qdbuff = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, routeTest.Length, BufferUsage.WriteOnly);
            //qdbuff.SetData(routeTest);

            // Initialize all game services
            foreach (var service in GameService.All)
                service.DoInitialize(this);


            base.Initialize();
        }

        private List<OverlayData> testData;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {

            BHGw2Api.Settings.Load();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(this.GraphicsDevice);

            // TODO: Move to TacO module
            // TODO: Add to markers & pathing module and ensure directory exists before attempting to get files
            //testData = new List<OverlayData>();
            //foreach (string markerFile in Directory.GetFiles("taco")) {
            //    testData.Add(Modules.Compatibility.TacO.OverlayData.FromFile(markerFile));
            //}

            //SunkenChestMarkers = Modules.Compatibility.TacO.OverlayData.FromFile("tw_core_sunkenchests.xml");

            

            b.DepthBufferEnable = true;
            b.DepthBufferWriteEnable = false;
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

        public static SpriteFont font_def12;

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

        private DepthStencilState b = new DepthStencilState();

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            if (!GameService.GameIntegration.Gw2IsRunning) return;

            //var rasterizerState = new RasterizerState();
            ////rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            //// TODO: We need to be culling in production builds
            //// Actually, on second thought, some things need no culling (like trails) so they can be seen from both sides
            //rasterizerState.CullMode = CullMode.None;
            ////rasterizerState.FillMode = FillMode.WireFrame;
            //this.GraphicsDevice.RasterizerState = rasterizerState;

            this.GraphicsDevice.BlendState = BlendState.Opaque;
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


            this.GraphicsDevice.Clear(Color.Transparent);



            //GameService.Debug.StartTimeFunc("Trails");
            //TrailEffect.Parameters["WorldViewProjection"].SetValue(GameService.Camera.View * GameService.Camera.Projection * Matrix.Identity);

            //foreach (OverlayData data in testData) {
            //    if (data == null || data.Trails == null) continue;
            //    foreach (Trail trail in data.Trails) {
            //        if (trail.MapId != GameService.Player.MapId) continue;

            //        TrailEffect.Parameters["Texture"].SetValue(trail.Texture);
            //        TrailEffect.Parameters["TotalMilliseconds"]
            //            .SetValue((float)gameTime.TotalGameTime.TotalMilliseconds);
            //        TrailEffect.Parameters["FlowSpeed"].SetValue(10);
            //        TrailEffect.Parameters["PlayerPosition"]
            //            .SetValue(GameService.Player.Position);
            //        TrailEffect.Parameters["FadeOutDistance"].SetValue((float)trail.FadeNear);
            //        TrailEffect.Parameters["FullClip"].SetValue((float)trail.FadeFar);
            //        TrailEffect.Parameters["FadeDistance"].SetValue((float)trail.Texture.Height / 256.0f / 2f);

            //        foreach (TrailSection trlSection in trail.Sections) {
            //            // TODO: See if we can remove this - it's not currently in use by the shader
            //            TrailEffect.Parameters["TotalLength"].SetValue(trlSection.Distance / trail.Texture.Height);

            //            foreach (EffectPass pass in TrailEffect.CurrentTechnique.Passes) {
            //                pass.Apply();

            //                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, trlSection.VertexData, 0,
            //                    trlSection.VertexData.Length - 2);
            //            }
            //        }
            //    }
            //}
            //GameService.Debug.StopTimeFunc("Trails"); 

            GameService.Debug.StartTimeFunc("UI Elements");
            if (GameService.Graphics.SpriteScreen != null && GameService.Graphics.SpriteScreen.Visible)
                GameService.Graphics.SpriteScreen.Draw(this.GraphicsDevice, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            GameService.Debug.StopTimeFunc("UI Elements");

            GameService.Debug.StartTimeFunc("3D objects");
            // Only draw 3D elements if we are in game
            if (GameService.GameIntegration.IsInGame)
                GameService.Graphics.World.Draw(this.GraphicsDevice);
            GameService.Debug.StopTimeFunc("3D objects");
            
            spriteBatch.Begin();

            Texture2D outRender;
            if (GameService.Graphics.SpriteScreen != null && GameService.Graphics.SpriteScreen.Visible) {
                if ((outRender = GameService.Graphics.SpriteScreen.GetRender()) != null)
                    spriteBatch.Draw(outRender, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
            }


#if DEBUG


            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameCounter.Update(deltaTime);

            var fps = string.Format("FPS: {0}", Math.Round(_frameCounter.AverageFramesPerSecond, 0));

            int debugLeft = GameService.Graphics.WindowWidth - 750;

            spriteBatch.DrawString(font_def12, fps, new Vector2(debugLeft, 25), Color.Red);

            int i = 0;
            foreach (KeyValuePair<string, DebugService.FuncClock> timedFuncPair in GameService.Debug.FuncTimes.Where(ft => ft.Value.AverageRuntime > 1).OrderByDescending(ft => ft.Value.AverageRuntime)) {
                spriteBatch.DrawString(font_def12, $"{timedFuncPair.Key} {timedFuncPair.Value.AverageRuntime} ms", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
                i++;
            }

            spriteBatch.DrawString(font_def12, $"Pathables Available: {GameService.Pathing.Pathables.Count}", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            i++;
            spriteBatch.DrawString(font_def12, $"3D Entities Displayed: {GameService.Graphics.World.Entities.Count}", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            i++;
            spriteBatch.DrawString(font_def12, $"Controls Displayed: {GameService.Graphics.SpriteScreen.GetDescendants().Count}", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            i++;
            spriteBatch.DrawString(font_def12, "Render Late: " + (gameTime.IsRunningSlowly ? "Yes" : "No"), new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);
            //i++;
            //spriteBatch.DrawString(font_def12, $"UI Tick: {(deltaTime * GameService.Gw2Mumble.AverageFramesPerUITick / 60)}%", new Vector2(debugLeft, 50 + (i * 25)), Color.Yellow);

#endif

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
