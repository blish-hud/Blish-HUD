using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    public struct WindowTab2 {
        public string Name { get; set; }
        public Texture2D Icon { get; set; }
        public int Priority { get; set; }

        public WindowTab2(string name, string icon) {
            this.Name = name;
            this.Icon = GameService.Content.GetTexture(icon);
            // Something arbitrary that will be consistent
            this.Priority = name.Length;
        }

        public WindowTab2(string name, Texture2D icon) {
            this.Name = name;
            this.Icon = icon;
            // Something arbitrary that will be consistent
            this.Priority = name.Length;
        }

        public WindowTab2(string name, string icon, int priority) {
            this.Name = name;
            this.Icon = GameService.Content.GetTexture(icon);
            this.Priority = priority;
        }

        public WindowTab2(string name, Texture2D icon, int priority) {
            this.Name = name;
            this.Icon = icon;
            this.Priority = priority;
        }

        public static bool operator ==(WindowTab2 tab1, WindowTab2 tab2) {
            return tab1.Name == tab2.Name && tab1.Icon == tab2.Icon;
        }

        public static bool operator !=(WindowTab2 tab1, WindowTab2 tab2) {
            return !(tab1 == tab2);
        }
    }

    public class TabbedWindow : Window {

        private const int TAB_HEIGHT = 52;
        private const int TAB_WIDTH = 104;
        private const int TAB_ICON_SIZE = 32;

        private const int TOP_PADDING = 16;
        private const int LEFT_PADDING = TAB_WIDTH;

        private const int WINDOW_WIDTH = 1024;
        private const int WINDOW_HEIGHT = 780;

        public event EventHandler<EventArgs> TabChanged;

        private int _selectedTabIndex = -1;
        public int SelectedTabIndex {
            get { return _selectedTabIndex; }
            set {
                if (_selectedTabIndex != value) {
                    _selectedTabIndex = value;
                    Invalidate();
                    this.TabChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public WindowTab2 SelectedTab { get { return Tabs[_selectedTabIndex]; } }

        private int _hoveredTabIndex = 0;
        private int HoveredTabIndex {
            get { return _hoveredTabIndex; }
            set {
                if (_hoveredTabIndex != value) {
                    _hoveredTabIndex = value;
                    Invalidate();
                }
            }
        }

        private Tooltip TabTooltip;
        private Label TabTooltipLabel;

        private Point Padding2 = new Point(LEFT_PADDING, TOP_PADDING);

        private Rectangle BackgroundBounds;
        private Rectangle WindowBounds;
        private Rectangle LeftTitleBarBounds;
        private Rectangle RightTitleBarBounds;

        public TabbedWindow() {
            TitleBarHeight = 64;
            //this.Size = new Point(988, 761);
            //this.Size = new Point(630 + LEFT_PADDING, 761 + TOP_PADDING);
            this.Size = new Point(WINDOW_WIDTH + LEFT_PADDING, WINDOW_HEIGHT + TOP_PADDING);
            ExitBounds = new Rectangle(this.Width - 32 - 32, 16, 32, 32).OffsetBy(0, Padding2.Y);

            WindowBounds = new Rectangle(Padding2, this.Size - Padding2);
            BackgroundBounds = WindowBounds.OffsetBy(46, 64 - 36);
            TitleBarBounds = new Rectangle(0, 0, 1024, 64).OffsetBy(WindowBounds.Location);
            LeftTitleBarBounds = new Rectangle(TitleBarBounds.X, TitleBarBounds.Y, Math.Min(TitleBarBounds.Width - 128, 1024), 64);
            RightTitleBarBounds = new Rectangle(TitleBarBounds.Right - 128, TitleBarBounds.Y, 128, 64);

            this.ContentRegion = WindowBounds.Add(50, 74, -88, -92);


            TabTooltip = new Tooltip() {
                Parent = GameServices.GetService<GraphicsService>().SpriteScreen,
                Visible = false,
            };
            TabTooltipLabel = new Label() {
                Text = "unknown",
                Font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular),
                Location = new Point(10, 10),
                TextColor = Color.White,
                ShadowColor = Color.Black,
                ShowShadow = true,
                Height = 20,
                AutoSizeWidth = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent = TabTooltip,
            };

            this.MouseMoved += TabbedWindow_MouseMoved;
            this.LeftMouseButtonPressed += TabbedWindow_LeftMouseButtonPressed;

            this.MouseLeft += delegate { Invalidate(); };

            this.TabChanged += delegate {
                Content.PlaySoundEffectByName($"audio\\tab-swap-{Utils.Calc.GetRandom(1, 5)}");

                Navigate(Panels[this.SelectedTab], false);
            };
        }

        // TODO: Cleanup window bounds
        // Stop-gap until window padding can be tightened up to match actual bounds better
        private bool inPaddingZone = false;
        protected override CaptureType CapturesInput() {
            // Stop-gap until window padding can be tightened up to match actual bounds better
            if (!inPaddingZone)
                return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;

            return CaptureType.None;
        }

        private void TabbedWindow_MouseMoved(object sender, MouseEventArgs e) {
            var relMousePos = e.MouseState.Position - this.AbsoluteBounds.Location;
            bool newSet = false;

            inPaddingZone = relMousePos.X < 105 || relMousePos.X > 1100;
            inPaddingZone = inPaddingZone || relMousePos.Y < 30 || relMousePos.Y > 785;
            if (inPaddingZone) return;

            if (relMousePos.X < standardTabBounds.Right && relMousePos.Y > standardTabBounds.Y) {
                var tabList = TabRegions.ToList();
                for (var tabIndex = 0; tabIndex < Tabs.Count; tabIndex++) {
                    var tab = Tabs[tabIndex];
                    if (TabRegions[tab].Contains(relMousePos)) {
                        this.HoveredTabIndex = tabIndex;
                        newSet = true;
                        TabTooltipLabel.Text = tab.Name;
                        this.Tooltip = TabTooltip;
                        break;
                    }
                }
                tabList.Clear();
            }

            if (!newSet) {
                this.HoveredTabIndex = -1;
                if (this.Tooltip != null)
                    this.Tooltip.Visible = false;
                this.Tooltip = null;
            }
        }

        private void TabbedWindow_LeftMouseButtonPressed(object sender, MouseEventArgs e) {
            var relMousePos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (relMousePos.X < standardTabBounds.Right && relMousePos.Y > standardTabBounds.Y) {
                var tabList = Tabs.ToList();
                for (var tabIndex = 0; tabIndex < Tabs.Count; tabIndex++) {
                    var tab = tabList[tabIndex];
                    if (TabRegions[tab].Contains(relMousePos)) {
                        this.SelectedTabIndex = tabIndex;
                        break;
                    }
                }
                tabList.Clear();
            }
        }

        public Dictionary<WindowTab2, Rectangle> TabRegions = new Dictionary<WindowTab2, Rectangle>();
        public Dictionary<WindowTab2, Panel> Panels = new Dictionary<WindowTab2, Panel>();
        public List<WindowTab2> Tabs = new List<WindowTab2>();

        public WindowTab2 AddTab(string name, string icon, Panel panel, int priority) {
            var tab = new WindowTab2(name, icon, priority);
            AddTab(tab, panel);
            return tab;
        }

        public WindowTab2 AddTab(string name, Texture2D icon, Panel panel, int priority) {
            var tab = new WindowTab2(name, icon, priority);
            AddTab(tab, panel);
            return tab;
        }

        public WindowTab2 AddTab(string name, string icon, Panel panel) {
            var tab = new WindowTab2(name, icon);
            AddTab(tab, panel);
            return tab;
        }

        public WindowTab2 AddTab(string name, Texture2D icon, Panel panel) {
            var tab = new WindowTab2(name, icon);
            AddTab(tab, panel);
            return tab;
        }

        public void AddTab(WindowTab2 tab, Panel panel) {
            if (!Tabs.Contains(tab)) {
                var prevTab = Tabs.Count > 0 ? Tabs[this.SelectedTabIndex] : tab;

                panel.Visible = false;
                panel.Parent = this;

                Tabs.Add(tab);
                TabRegions.Add(tab, TabBoundsFromIndex(TabRegions.Count));
                Panels.Add(tab, panel);

                Tabs = Tabs.OrderBy(t => t.Priority).ToList();

                var i = 0;
                foreach (var etab in Tabs.ToList()) {
                    TabRegions[etab] = TabBoundsFromIndex(i);
                    i++;
                }

                if (this._selectedTabIndex > -1)
                    this._selectedTabIndex = Tabs.IndexOf(prevTab);
                else
                    this.SelectedTabIndex = Tabs.IndexOf(prevTab);

                Invalidate();
            }
        }

        public void RemoveTab(WindowTab2 tab) {
            // TODO: If the last tab is for some reason removed, this will crash the application
            var prevTab = Tabs.Count > 0 ? Tabs[this.SelectedTabIndex] : Tabs[0];

            if (Tabs.Contains(tab)) {
                Tabs.Remove(tab);
                TabRegions.Remove(tab);
                Panels.Remove(tab);
            }

            Tabs = Tabs.OrderBy(t => t.Priority).ToList();

            for (var tabIndex = 0; tabIndex < TabRegions.Count; tabIndex++) {
                var curTab = Tabs[tabIndex];
                TabRegions[curTab] = TabBoundsFromIndex(tabIndex);
            }

            if (Tabs.Contains(prevTab)) {
                _selectedTabIndex = Tabs.IndexOf(prevTab);
            }

            Invalidate();
        }
        
        private static Rectangle standardTabBounds = new Rectangle(TAB_WIDTH / 2 - 6, 88, TAB_WIDTH, TAB_HEIGHT);

        private Rectangle TabBoundsFromIndex(int index) {
            return standardTabBounds.OffsetBy(WindowBounds.Location.X - TAB_WIDTH, WindowBounds.Location.Y + index * TAB_HEIGHT);
        }

        public override void Invalidate() {
            base.Invalidate();

            // TODO: Cleanup -- too many magic numbers
            //this.ContentRegion = WindowBounds.Add(60, 74, -88, -92); // new Rectangle(60 + Padding2.X, this.TitleBarHeight + 10 + Padding.Y, Width - 20 - Padding.X, Height - 20 - 64 - Padding.Y);
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            string bkgtxt = "502049";

            //spriteBatch.Draw(Content.GetTexture("hero-background2"), bounds.OffsetBy(43, 0).Add(0, 0, -43, 0), new Rectangle(43, 0, Content.GetTexture("hero-background2").Width - 43, Content.GetTexture("hero-background2").Height), Color.White);
            //spriteBatch.Draw(Content.GetTexture(bkgtxt), bounds.OffsetBy(0, 0).Add(0, 0, Content.GetTexture(bkgtxt).Width - Width, 0), Color.White);
            // Draw window background
            var srcrec = Content.GetTexture(bkgtxt).Bounds.Add(50, 0, -50, 0);
            spriteBatch.Draw(Content.GetTexture(bkgtxt), BackgroundBounds, srcrec, Color.White);

            // Draw black block for tab bar
            spriteBatch.Draw(ContentService.Textures.Pixel, new Rectangle(0, 38, 44, 52).OffsetBy(Padding2), Color.Black);

            if (this.MouseOver && Input.MouseState.Position.Y < this.Top + TitleBarHeight && Input.MouseState.Position.Y > TOP_PADDING && !this.HoverClose) {
                spriteBatch.Draw(Content.GetTexture("titlebar-active"), LeftTitleBarBounds, Color.White);
                spriteBatch.Draw(Content.GetTexture("titlebar-active"), LeftTitleBarBounds, Color.White);
                spriteBatch.Draw(Content.GetTexture("window-topright-active"), RightTitleBarBounds, Color.White);
                spriteBatch.Draw(Content.GetTexture("window-topright-active"), RightTitleBarBounds, Color.White);
            } else {
                spriteBatch.Draw(Content.GetTexture("titlebar-inactive"), LeftTitleBarBounds, Color.White);
                spriteBatch.Draw(Content.GetTexture("titlebar-inactive"), LeftTitleBarBounds, Color.White);
                spriteBatch.Draw(Content.GetTexture("window-topright"), RightTitleBarBounds, Color.White);
                spriteBatch.Draw(Content.GetTexture("window-topright"), RightTitleBarBounds, Color.White);
            }

            var windowBadge = Content.GetTexture("test-window-icon9");
            spriteBatch.Draw(windowBadge, windowBadge.Bounds.OffsetBy(Padding2).OffsetBy(-16, -16), Color.White);
            
            var fadeTexture = Content.GetTexture("fade-down-46");
            spriteBatch.Draw(fadeTexture, new Rectangle(WindowBounds.X, WindowBounds.Y + 52 * TabRegions.Count + 60 + 28, fadeTexture.Width, this.Height - 52 * TabRegions.Count - WindowBounds.Y - 64 - 24), Color.White);

            var splitTexture = Content.GetTexture("605024");
            spriteBatch.Draw(splitTexture, new Rectangle(WindowBounds.X + 52 - 16, WindowBounds.Y + 70, splitTexture.Width, splitTexture.Height), Color.White);
            spriteBatch.Draw(splitTexture, new Rectangle(WindowBounds.X + 52 - 16, WindowBounds.Y + 200, splitTexture.Width, splitTexture.Height), Color.White);

            // Draw tabs

            int i = 0;
            foreach (var tab in Tabs) {
                bool active = (i == this.SelectedTabIndex);
                bool hovered = (i == this.HoveredTabIndex);

                var destBounds = TabRegions[tab];
                var subBounds = new Rectangle(destBounds.X + destBounds.Width / 2, destBounds.Y, TAB_WIDTH / 2, destBounds.Height).OffsetBy(bounds.Location);

                if (active) {
                    spriteBatch.Draw(Content.GetTexture(bkgtxt), destBounds.OffsetBy(bounds.Location), destBounds.Add(-TAB_WIDTH, 0, 110, 0).OffsetBy(bounds.Location), Color.White);
                    spriteBatch.Draw(Content.GetTexture("window-tab-active"), destBounds.OffsetBy(bounds.Location), Color.White);
                } else {
                    spriteBatch.Draw(Content.GetTexture("black-46x52"), subBounds.OffsetBy(6, 0).Add(0, 0, -6, 0), Color.White);
                }

                spriteBatch.Draw(tab.Icon, new Rectangle(TAB_WIDTH / 4 - TAB_ICON_SIZE / 2 + 2, TAB_HEIGHT / 2 - TAB_ICON_SIZE / 2, TAB_ICON_SIZE, TAB_ICON_SIZE).OffsetBy(subBounds.Location), active || hovered ? Color.White : ContentService.Colors.DullColor);
                //spriteBatch.Draw(tab.Icon, subBounds, active ? Color.White : ContentService.Colors.DullColor);

                i++;
            }

            // End drawing tabs

            var cornerTexture = Content.GetTexture("156008");
            spriteBatch.Draw(cornerTexture, new Rectangle(this.Width - cornerTexture.Width, this.Height - cornerTexture.Height, cornerTexture.Width, cornerTexture.Height).OffsetBy(Padding2.X - 15, 0), Color.White);
            
            base.PaintContainer(spriteBatch, bounds);
        }

    }
}
