using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    public class TabbedWindow : WindowBase {

        private const int TAB_HEIGHT = 52;
        private const int TAB_WIDTH = 104;
        private const int TAB_ICON_SIZE = 32;

        private const int TAB_SECTION_WIDTH = 46;

        private const int TOP_PADDING = 0;
        private const int LEFT_PADDING = 0;

        private const int WINDOW_WIDTH = 1024;
        private const int WINDOW_HEIGHT = 780;

        private const int WINDOWCONTENT_WIDTH = 1024;
        private const int WINDOWCONTENT_HEIGHT = 700;

        #region Load Static

        private static Texture2D _textureDefaultBackround;
        private static Texture2D _textureSplitLine;
        private static Texture2D _textureBlackFade;
        private static Texture2D _textureTabActive;

        static TabbedWindow() {
            _textureDefaultBackround = Content.GetTexture("502049");
            _textureSplitLine        = Content.GetTexture("605024");
            _textureBlackFade        = Content.GetTexture("fade-down-46");
            _textureTabActive        = Content.GetTexture("window-tab-active");
        }

        #endregion

        private static readonly Rectangle StandardTabBounds = new Rectangle(TAB_SECTION_WIDTH, 24, TAB_WIDTH, TAB_HEIGHT);

        public event EventHandler<EventArgs> TabChanged;

        protected int _selectedTabIndex = -1;
        public int SelectedTabIndex {
            get => _selectedTabIndex;
            set {
                if (SetProperty(ref _selectedTabIndex, value)) {
                    this.Subtitle = SelectedTab.Name;
                    this.TabChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public WindowTab SelectedTab => Tabs[_selectedTabIndex];

        private int _hoveredTabIndex = 0;
        private int HoveredTabIndex {
            get => _hoveredTabIndex;
            set => SetProperty(ref _hoveredTabIndex, value);
        }

        private Point Padding2 = new Point(LEFT_PADDING, TOP_PADDING);

        private Rectangle BackgroundBounds;
        private Rectangle WindowBounds;
        private Rectangle LeftTitleBarBounds;
        private Rectangle RightTitleBarBounds;

        public TabbedWindow() {
            TitleBarHeight = 64;

            Size = new Point(WINDOW_WIDTH + LEFT_PADDING, WINDOW_HEIGHT + TOP_PADDING);
            ExitBounds = new Rectangle(this.Width - 32 - 32, 16, 32, 32).OffsetBy(0, Padding2.Y);

            var tabWindowTexture = _textureDefaultBackround;
            tabWindowTexture = tabWindowTexture.Duplicate().SetRegion(0, 0, 64, _textureDefaultBackround.Height, Color.Transparent);   

            ConstructWindow(tabWindowTexture, new Vector2(25, 35), new Rectangle(0, 0, 1100, 700), new Thickness(60, 75, 45, 25), 40);

            WindowBounds = new Rectangle(Padding2, _size - Padding2);
            //BackgroundBounds = WindowBounds.OffsetBy(44, 64 - 36);
            //TitleBarBounds = new Rectangle(0, 0, 1024, 64).OffsetBy(WindowBounds.Location);
            //LeftTitleBarBounds = new Rectangle(TitleBarBounds.X, TitleBarBounds.Y, Math.Min(TitleBarBounds.Width - 128, 1024), 64);
            //RightTitleBarBounds = new Rectangle(TitleBarBounds.Right - 128, TitleBarBounds.Y, 128, 64);

            //ContentRegion = new Rectangle(TAB_WIDTH / 2, 48, Width - TAB_WIDTH / 2, Height - 128);

            ContentRegion = new Rectangle(TAB_WIDTH / 2, 48, WINDOWCONTENT_WIDTH, WINDOWCONTENT_HEIGHT);

            this.MouseMoved += TabbedWindow_MouseMoved;
            this.LeftMouseButtonPressed += TabbedWindow_LeftMouseButtonPressed;

            this.MouseLeft += delegate { Invalidate(); };

            this.TabChanged += delegate {
                if (_visible)
                    Content.PlaySoundEffectByName($"audio\\tab-swap-{Utils.Calc.GetRandom(1, 5)}");

                Navigate(Panels[this.SelectedTab], false);
            };
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        private void TabbedWindow_MouseMoved(object sender, MouseEventArgs e) {
            bool newSet = false;

            if (RelativeMousePosition.X < StandardTabBounds.Right && RelativeMousePosition.Y > StandardTabBounds.Y) {
                var tabList = TabRegions.ToList();
                for (int tabIndex = 0; tabIndex < Tabs.Count; tabIndex++) {
                    var tab = Tabs[tabIndex];
                    if (TabRegions[tab].Contains(RelativeMousePosition)) {
                        HoveredTabIndex       = tabIndex;
                        newSet                = true;
                        this.BasicTooltipText = tab.Name;

                        break;
                    }
                }
                tabList.Clear();
            }

            if (!newSet) {
                this.HoveredTabIndex = -1;
                this.BasicTooltipText = null;
            }
        }

        private void TabbedWindow_LeftMouseButtonPressed(object sender, MouseEventArgs e) {
            if (RelativeMousePosition.X < StandardTabBounds.Right && RelativeMousePosition.Y > StandardTabBounds.Y) {
                var tabList = Tabs.ToList();
                for (int tabIndex = 0; tabIndex < Tabs.Count; tabIndex++) {
                    var tab = tabList[tabIndex];
                    if (TabRegions[tab].Contains(RelativeMousePosition)) {
                        SelectedTabIndex = tabIndex;

                        break;
                    }
                }
                tabList.Clear();
            }
        }

        #region Tab Handling

        public Dictionary<WindowTab, Rectangle> TabRegions = new Dictionary<WindowTab, Rectangle>();
        public Dictionary<WindowTab, Panel> Panels = new Dictionary<WindowTab, Panel>();
        public List<WindowTab> Tabs = new List<WindowTab>();

        public WindowTab AddTab(string name, Texture2D icon, Panel panel, int priority) {
            var tab = new WindowTab(name, icon, priority);
            AddTab(tab, panel);
            return tab;
        }

        public WindowTab AddTab(string name, Texture2D icon, Panel panel) {
            var tab = new WindowTab(name, icon);
            AddTab(tab, panel);
            return tab;
        }

        public void AddTab(WindowTab tab, Panel panel) {
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

                if (_selectedTabIndex > -1)
                    _selectedTabIndex = Tabs.IndexOf(prevTab);
                else
                    SelectedTabIndex = Tabs.IndexOf(prevTab);

                Invalidate();
            }
        }

        public void RemoveTab(WindowTab tab) {
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

        #endregion
        

        private Rectangle TabBoundsFromIndex(int index) {
            return StandardTabBounds.OffsetBy(-TAB_WIDTH, ContentRegion.Y + index * TAB_HEIGHT);
        }

        #region Calculated Layout

        private Rectangle _layoutTopTabBarBounds;
        private Rectangle _layoutBottomTabBarBounds;

        private Rectangle _layoutTopSplitLineBounds;
        private Rectangle _layoutBottomSplitLineBounds;

        private Rectangle _layoutTopSplitLineSourceBounds;
        private Rectangle _layoutBottomSplitLineSourceBounds;

        #endregion

        public override void RecalculateLayout() {
            base.RecalculateLayout();

            var firstTabBounds = TabBoundsFromIndex(0);
            var selectedTabBounds = TabRegions[this.SelectedTab];
            var lastTabBounds = TabBoundsFromIndex(TabRegions.Count - 1);

            _layoutTopTabBarBounds = new Rectangle(0, 0, TAB_SECTION_WIDTH, firstTabBounds.Top);
            _layoutBottomTabBarBounds = new Rectangle(0, lastTabBounds.Bottom, TAB_SECTION_WIDTH, _size.Y - lastTabBounds.Bottom);

            int topSplitHeight = selectedTabBounds.Top - ContentRegion.Top;
            int bottomSplitHeight = ContentRegion.Bottom - selectedTabBounds.Bottom;

            _layoutTopSplitLineBounds = new Rectangle(ContentRegion.X - _textureSplitLine.Width + 1,
                                                      ContentRegion.Y,
                                                      _textureSplitLine.Width,
                                                      topSplitHeight);
            _layoutTopSplitLineSourceBounds = new Rectangle(0, 0, _textureSplitLine.Width, topSplitHeight);


            _layoutBottomSplitLineBounds = new Rectangle(ContentRegion.X - _textureSplitLine.Width + 1,
                                                         selectedTabBounds.Bottom,
                                                         _textureSplitLine.Width,
                                                         bottomSplitHeight);
            _layoutBottomSplitLineSourceBounds = new Rectangle(0, _textureSplitLine.Height - bottomSplitHeight, _textureSplitLine.Width, bottomSplitHeight);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            // Draw black block for tab bar
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                                   _layoutTopTabBarBounds,
                                   Color.Black);

            // Draw black fade for tab bar
            spriteBatch.DrawOnCtrl(this, _textureBlackFade, _layoutBottomTabBarBounds);

            // Draw tabs
            int i = 0;
            foreach (var tab in Tabs) {
                bool active = (i == this.SelectedTabIndex);
                bool hovered = (i == this.HoveredTabIndex);

                var tabBounds = TabRegions[tab];
                var subBounds = new Rectangle(tabBounds.X + tabBounds.Width / 2, tabBounds.Y, TAB_WIDTH / 2, tabBounds.Height);

                if (active) {
                    spriteBatch.DrawOnCtrl(this, _textureDefaultBackround,
                                           tabBounds,
                                           tabBounds.OffsetBy(_windowBackgroundOrigin.ToPoint()).Add(0, -35, 0, 0).Add(tabBounds.Width / 3, 0, -tabBounds.Width / 3, 0),
                                     Color.White);

                    spriteBatch.DrawOnCtrl(this, _textureTabActive, tabBounds);
                } else {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, tabBounds.Y, TAB_SECTION_WIDTH, tabBounds.Height), Color.Black);
                }

                spriteBatch.DrawOnCtrl(this, tab.Icon,
                                 new Rectangle(TAB_WIDTH / 4 - TAB_ICON_SIZE / 2 + 2,
                                               TAB_HEIGHT / 2 - TAB_ICON_SIZE / 2,
                                               TAB_ICON_SIZE,
                                               TAB_ICON_SIZE).OffsetBy(subBounds.Location),
                                 active || hovered
                                     ? Color.White
                                     : ContentService.Colors.DullColor);

                i++;
            }

            // Draw top of split
            spriteBatch.DrawOnCtrl(this, _textureSplitLine,
                                   _layoutTopSplitLineBounds,
                                   _layoutTopSplitLineSourceBounds);

            // Draw bottom of split
            spriteBatch.DrawOnCtrl(this, _textureSplitLine,
                                   _layoutBottomSplitLineBounds,
                                   _layoutBottomSplitLineSourceBounds);
        }

    }
}
