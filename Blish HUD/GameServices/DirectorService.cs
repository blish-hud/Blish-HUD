using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {

    public class DirectorService:GameService {

        public TabbedWindow BlishHudWindow { get; protected set; }
        public CornerIcon BlishMenuIcon { get; protected set; }
        public ContextMenuStrip BlishContextMenu { get; protected set; }

        private readonly ConcurrentQueue<Action<GameTime>> _queuedUpdates = new ConcurrentQueue<Action<GameTime>>();

        /// <summary>
        /// Allows you to enqueue a call that will occur during the next time the update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see="GameTime" /> as a parameter.</param>
        public void QueueMainThreadUpdate(Action<GameTime> call) {
            _queuedUpdates.Enqueue(call);
        }

        protected override void Initialize() {
            //Texture2D mask = Content.GetTexture("157357");
            //Texture2D background = Content.GetTexture("157356");

            //var coverageEffect = Overlay.cm.Load<Effect>(@"effects\gw2master");
            //coverageEffect.Parameters["Mask"].SetValue(mask);
            //coverageEffect.Parameters["directionIn"].SetValue(false);
            //coverageEffect.Parameters["Roller"].SetValue(0f);

            //Controls.Image shaderTest = new Image(background) {
            //    BackgroundColor = Color.Magenta,
            //    Parent = Graphics.SpriteScreen,
            //    Size = new Point(128, 128),
            //    Location = new Point(512, 100),
            //    //SpriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Immediate,
            //    //BlendState.NonPremultiplied,
            //    //SamplerState.LinearClamp,
            //    //null,
            //    //null,
            //    //coverageEffect,
            //    //GameService.Graphics.UIScaleTransform)
            //};

            //Controls.TrackBar rollbarTest = new TrackBar() {
            //    Parent = Graphics.SpriteScreen,
            //    MinValue = 0,
            //    MaxValue = 100,
            //    Value = 0,
            //    Location = new Point(512, shaderTest.Bottom - shaderTest.Height / 2),
            //};

            //rollbarTest.Left = shaderTest.Right + 10;

            //rollbarTest.ValueChanged += delegate {
            //    coverageEffect.Parameters["Roller"].SetValue(rollbarTest.Value / 100f);
            //};
        }

        protected override void Load() {
            this.BlishMenuIcon = new CornerIcon() {
                Icon             = Content.GetTexture("logo"),
                HoverIcon        = Content.GetTexture("logo-big"),
                Menu             = new ContextMenuStrip(),
                BasicTooltipText = Properties.Strings.General_BlishHUD,
                Priority         = int.MaxValue,
                Parent           = Graphics.SpriteScreen,
            };

            this.BlishContextMenu = this.BlishMenuIcon.Menu;
            this.BlishContextMenu.AddMenuItem($"{Properties.Strings.General_Close} {Properties.Strings.General_BlishHUD}").Click += delegate { ActiveOverlay.Exit(); };

            this.BlishHudWindow = new TabbedWindow() {
                Parent = Graphics.SpriteScreen,
                Title  = Properties.Strings.General_BlishHUD,
                Emblem = Content.GetTexture("test-window-icon9")
            };

            this.BlishMenuIcon.LeftMouseButtonReleased += delegate {
                this.BlishHudWindow.ToggleWindow();
            };

            // Center the window so that you don't have to drag it over every single time (which is really annoying)
            // TODO: Save window positions to settings so that they remember where they were last
            Graphics.SpriteScreen.Resized += delegate {
                if (!this.BlishHudWindow.Visible)
                    this.BlishHudWindow.Location = new Point(Graphics.WindowWidth / 2 - this.BlishHudWindow.Width / 2, Graphics.WindowHeight / 2 - this.BlishHudWindow.Height / 2);
            };

            this.BlishHudWindow.AddTab(Properties.Strings.Service_DirectorService_Tab_Home, Content.GetTexture("255369"), BuildHomePanel(this.BlishHudWindow), int.MinValue);
        }

        private Panel BuildHomePanel(WindowBase wndw) {
            var hPanel = new Panel() {
                Size = wndw.ContentRegion.Size
            };

            var bttn = new StandardButton() {
                Size       = new Point(160, 26),
                Icon       = Content.GetTexture("1228909"),
                ResizeIcon = true,
                Parent     = hPanel,
                Text       = "Announcements",
                Location   = new Point(100, 100)
            };

            var bttn2 = new StandardButton() {
                Size       = new Point(160, 26),
                Icon       = Content.GetTexture("1770706"),
                ResizeIcon = false,
                Parent     = hPanel,
                Text       = "Celebrate",
                Location   = new Point(bttn.Left, bttn.Bottom + 5)
            };

            var bttn3 = new StandardButton() {
                Size       = new Point(160, 26),
                Icon       = Content.GetTexture("156384"),
                ResizeIcon = true,
                Parent     = hPanel,
                Text       = "ArenaNet is Cool",
                Location   = new Point(bttn2.Left, bttn2.Bottom + 5)
            };

            var bttn4 = new StandardButton() {
                Size       = new Point(32, 32),
                Icon       = Content.GetTexture("156627"),
                Parent     = hPanel,
                ResizeIcon = true,
                Location   = new Point(bttn3.Right + 5, bttn3.Top - 4),
                Visible = false
            };

            var bttn5 = new StandardButton() {
                Size       = new Point(160, 26),
                Icon       = Content.GetTexture("156356"),
                ResizeIcon = true,
                Parent     = hPanel,
                Text       = "I Like This",
                Location   = new Point(bttn3.Left, bttn3.Bottom + 5)
            };

            //var rsreader = new Content.RenderServiceReader();

            //var textureStream = rsreader.GetFileStream("18CE5D78317265000CF3C23ED76AB3CEE86BA60E/65941");

            //rsreader.GetFileStream("4F19A8B4E309C3042358FB194F7190331DEF27EB/631494");
            //rsreader.GetFileStream("027D1D382447933D074BE45F405EA1F379471DEB/63127");
            //rsreader.GetFileStream("9D94B96446F269662F6ACC2531394A06C0E03951/947657");
            //rsreader.GetFileStream("18CE5D78317265000CF3C23ED76AB3CEE86BA60E/65941");

            //Texture2D ectoPic = Texture2D.FromStream(Graphics.GraphicsDevice, textureStream);

            //var img = new Image() {
            //    Size     = new Point(64, 64),
            //    Parent   = hPanel,
            //    Texture = ectoPic,
            //    Location = new Point(256, 256)
            //};

            //var hi = new Label() {
            //    Text = Utils.DrawUtil.WrapText(Content.DefaultFont14, "Thanks for trying Blish HUD!  More to come soon!  :)  -- FreeSnow (LandersXanders.1235)", 50),
            //    Parent = hPanel,
            //    Location = Point.Zero,
            //    Height = 128,
            //    AutoSizeWidth = true,
            //    StrokeText = true,
            //    HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
            //    BackgroundColor = Color.Magenta
            //};

            //hi.Location = new Point(hPanel.Width / 2 - hi.Width / 2, hPanel.Height / 2 - hi.Height / 2);

            return hPanel;
        }

        protected override void Unload() {
            this.BlishMenuIcon.Dispose();
            this.BlishHudWindow.Dispose();
        }

        // TODO: Move into a TacO compatibility module
        private double lastTacoCheckTime = 5;

        private void HandleEnqueuedUpdates(GameTime gameTime) {
            while (_queuedUpdates.TryDequeue(out Action<GameTime> updateCall)) {
                updateCall.Invoke(gameTime);
            }
        }

        protected override void Update(GameTime gameTime) {
            HandleEnqueuedUpdates(gameTime);

            if (GameService.GameIntegration.IsInGame) {
                CornerIcon.Alignment = CornerIcon.CornerIconAlignment.Left;

                lastTacoCheckTime += gameTime.ElapsedGameTime.TotalSeconds;

                // TODO: Move some of this into the TacO related module
                if (lastTacoCheckTime > 3) {
                    Process[] tacoApp = Process.GetProcessesByName("GW2TacO");

                    if (tacoApp.Length > 0)
                        CornerIcon.LeftOffset = 32 + 4;
                    else
                        CornerIcon.LeftOffset = 0;

                    lastTacoCheckTime = 0;
                }
            } else {
                    // We are either at a loading screen, or on the character selection screen

                    /* Stick the icons in the middle of the screen because it looks like the character
                       select screen uses a different icon size and I don't want it to look weird next to
                       different sized icons */

                    CornerIcon.Alignment = CornerIcon.CornerIconAlignment.Center;
            }
        }

    }
}
