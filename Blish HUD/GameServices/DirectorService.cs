using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using ContextMenuStrip = Blish_HUD.Controls.ContextMenuStrip;
using Label = Blish_HUD.Controls.Label;
using Panel = Blish_HUD.Controls.Panel;

namespace Blish_HUD {

    public class DirectorService:GameService {

        public TabbedWindow BlishHudWindow { get; protected set; }
        public CornerIcon BlishMenuIcon { get; protected set; }
        public ContextMenuStrip BlishContextMenu { get; protected set; }

        protected override void Initialize() {
        }

        protected override void Load() {
            this.BlishMenuIcon = new CornerIcon() {
                Icon = GameService.Content.GetTexture("logo"),
                HoverIcon = GameService.Content.GetTexture("logo-big"),
                Menu = new ContextMenuStrip(),
                Tooltip = new Tooltip(),
                Priority = int.MinValue,
            };

            this.BlishMenuIcon.Tooltip.AddChild(new Label() {
                Text = "Blish HUD",
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular),
                Location = new Point(10, 8),
                Height = 12,
                TextColor = Color.White,
                ShadowColor = Color.Black,
                ShowShadow = true,
                AutoSizeWidth = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
            });

            this.BlishContextMenu = this.BlishMenuIcon.Menu;
            this.BlishContextMenu.AddMenuItem("Close Blish HUD").Click += delegate { Overlay.Exit(); };

            this.BlishHudWindow = new TabbedWindow() {
                Parent = Graphics.SpriteScreen,
                Title = "Blish HUD"
            };

            //var iconHoverRegion = new MouseZone() {
            //    Size = new Point(32 * 10, 32),
            //    Location = new Point(0, 0),
            //    Parent = GameService.Graphics.SpriteScreen,
            //};

            //iconHoverRegion.MouseEntered += delegate { this.BlishMenuIcon.MouseInHouse = true; };
            //iconHoverRegion.MouseLeft += delegate { this.BlishMenuIcon.MouseInHouse = false; };

            this.BlishMenuIcon.LeftMouseButtonReleased += delegate {
                this.BlishHudWindow.ToggleWindow();
            };

            // Center the window so that you don't have to drag it over every single time (which is really annoying)
            // TODO: Save window positions to settings so that they remember where they were last
            Graphics.SpriteScreen.Resized += delegate {
                if (!this.BlishHudWindow.Visible)
                    this.BlishHudWindow.Location = new Point(Graphics.WindowWidth / 2 - this.BlishHudWindow.Width / 2, Graphics.WindowHeight / 2 - this.BlishHudWindow.Height / 2);
            };

            this.BlishHudWindow.AddTab("Home", "255369", BuildHomePanel(this.BlishHudWindow), int.MinValue);
        }

        private Panel BuildHomePanel(Window wndw) {
            var hPanel = new Panel() {
                Size = wndw.ContentRegion.Size
            };

            var hi = new Label() {
                Text = "Thanks for trying Blish HUD!  More to come soon!  :)  -- FreeSnow (LandersXanders.1235)",
                Parent = hPanel,
                Location = Point.Zero,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeShadow = true
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

        protected override void Update(GameTime gameTime) {
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
