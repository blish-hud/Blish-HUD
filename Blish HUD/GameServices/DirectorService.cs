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

namespace Blish_HUD {

    public class DirectorService:GameService {

        public TabbedWindow BlishHudWindow { get; protected set; }
        public CornerIcon BlishMenuIcon { get; protected set; }
        public ContextMenuStrip BlishContextMenu { get; protected set; }

        private ConcurrentQueue<Action<GameTime>> _queuedUpdates = new ConcurrentQueue<Action<GameTime>>();

        /// <summary>
        /// Allows you to enqueue a call that will occur during the next time the Update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see="GameTime" /> as a parameter.</param>
        public void QueueAdHocUpdate(Action<GameTime> call) {
            _queuedUpdates.Enqueue(call);
        }

        protected override void Initialize() {
        }

        protected override void Load() {
            this.BlishMenuIcon = new CornerIcon() {
                Icon             = Content.GetTexture("logo"),
                HoverIcon        = Content.GetTexture("logo-big"),
                Menu             = new ContextMenuStrip(),
                BasicTooltipText = Properties.Strings.General_BlishHUD,
                Priority         = int.MinValue,
            };

            this.BlishContextMenu = this.BlishMenuIcon.Menu;
            this.BlishContextMenu.AddMenuItem($"{Properties.Strings.General_Close} {Properties.Strings.General_BlishHUD}").Click += delegate { Overlay.Exit(); };

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

            var hi = new Label() {
                Text = Utils.DrawUtil.WrapText(Content.DefaultFont14, "Thanks for trying Blish HUD!  More to come soon!  :)  -- FreeSnow (LandersXanders.1235)", 50),
                Parent = hPanel,
                Location = Point.Zero,
                Height = 128,
                AutoSizeWidth = true,
                StrokeText = true,
                HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
                BackgroundColor = Color.Magenta
            };

            hi.Location = new Point(hPanel.Width / 2 - hi.Width / 2, hPanel.Height / 2 - hi.Height / 2);

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
