using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;

namespace Blish_HUD.Controls {

    public class TabbedWindow : WindowBase {

        private const int TAB_HEIGHT    = 52;
        private const int TAB_WIDTH     = 104;
        private const int TAB_ICON_SIZE = 32;

        private const int TAB_SECTION_WIDTH = 46;

        private const int WINDOWCONTENT_WIDTH  = 1024;
        private const int WINDOWCONTENT_HEIGHT = 700;

        #region Load Static

        private static readonly Texture2D _textureDefaultBackround;
        private static readonly Texture2D _textureSplitLine;
        private static readonly Texture2D _textureBlackFade;
        private static readonly Texture2D _textureTabActive;

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
                    OnTabChanged(EventArgs.Empty);
                }
            }
        }

        public WindowTab SelectedTab => _tabs.Count > _selectedTabIndex ? _tabs[_selectedTabIndex] : null;

        private int _hoveredTabIndex = 0;
        private int HoveredTabIndex {
            get => _hoveredTabIndex;
            set => SetProperty(ref _hoveredTabIndex, value);
        }

        private readonly Dictionary<WindowTab, Rectangle> _tabRegions = new Dictionary<WindowTab, Rectangle>();
        private readonly Dictionary<WindowTab, Panel>     _panels     = new Dictionary<WindowTab, Panel>();
        private          List<WindowTab>                  _tabs       = new List<WindowTab>();

        // TODO: Remove public access to _panels - only kept currently as it is used by KillProof.me module (need more robust "Navigate()" call for panel history)
        public Dictionary<WindowTab, Panel> Panels => _panels;

        public TabbedWindow() {
            var tabWindowTexture = _textureDefaultBackround;
            tabWindowTexture = tabWindowTexture.Duplicate().SetRegion(0, 0, 64, _textureDefaultBackround.Height, Color.Transparent);   

            this.ConstructWindow(tabWindowTexture, new Vector2(25, 33), new Rectangle(0, 0, 1100, 745), new Thickness(60, 75, 45, 25), 40);

            _contentRegion = new Rectangle(TAB_WIDTH / 2, 48, WINDOWCONTENT_WIDTH, WINDOWCONTENT_HEIGHT);
        }

        protected virtual void OnTabChanged(EventArgs e) {
            if (_visible) {
                Content.PlaySoundEffectByName($"audio\\tab-swap-{RandomUtil.GetRandom(1, 5)}");
            }

            this.Subtitle = SelectedTab.Name;

            Navigate(_panels[this.SelectedTab], false);

            this.TabChanged?.Invoke(this, e);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            this.HoveredTabIndex = -1;

            base.OnMouseLeft(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            bool newSet = false;

            if (RelativeMousePosition.X < StandardTabBounds.Right && RelativeMousePosition.Y > StandardTabBounds.Y) {
                var tabList = _tabRegions.ToList();
                for (int tabIndex = 0; tabIndex < _tabs.Count; tabIndex++) {
                    var tab = _tabs[tabIndex];
                    if (_tabRegions[tab].Contains(RelativeMousePosition)) {
                        HoveredTabIndex       = tabIndex;
                        newSet                = true;
                        this.BasicTooltipText = tab.Name;

                        break;
                    }
                }
                tabList.Clear();
            }

            if (!newSet) {
                this.HoveredTabIndex  = -1;
                this.BasicTooltipText = null;
            }

            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            if (RelativeMousePosition.X < StandardTabBounds.Right && RelativeMousePosition.Y > StandardTabBounds.Y) {
                var tabList = _tabs.ToList();
                for (int tabIndex = 0; tabIndex < _tabs.Count; tabIndex++) {
                    var tab = tabList[tabIndex];
                    if (_tabRegions[tab].Contains(RelativeMousePosition)) {
                        SelectedTabIndex = tabIndex;

                        break;
                    }
                }
                tabList.Clear();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        #region Tab Handling

        public WindowTab AddTab(string name, AsyncTexture2D icon, Panel panel, int priority) {
            var tab = new WindowTab(name, icon, priority);
            AddTab(tab, panel);
            return tab;
        }

        public WindowTab AddTab(string name, AsyncTexture2D icon, Panel panel) {
            var tab = new WindowTab(name, icon);
            AddTab(tab, panel);
            return tab;
        }

        public void AddTab(WindowTab tab, Panel panel) {
            if (!_tabs.Contains(tab)) {
                var prevTab = _tabs.Count > 0 ? _tabs[this.SelectedTabIndex] : tab;

                panel.Visible = false;
                panel.Parent  = this;

                _tabs.Add(tab);
                _tabRegions.Add(tab, TabBoundsFromIndex(_tabRegions.Count));
                _panels.Add(tab, panel);

                _tabs = _tabs.OrderBy(t => t.Priority).ToList();

                for (int i = 0; i < _tabs.Count; i++) {
                    _tabRegions[_tabs[i]] = TabBoundsFromIndex(i);
                }

                if (_selectedTabIndex == -1) {
                    this.SelectedTabIndex = _tabs.IndexOf(prevTab);
                }

                Invalidate();
            }
        }

        public void RemoveTab(WindowTab tab) {
            // TODO: If the last tab is for some reason removed, this will crash the application
            var prevTab = _tabs.Count > 0 ? _tabs[this.SelectedTabIndex] : _tabs[0];

            if (_tabs.Contains(tab)) {
                _tabs.Remove(tab);
                _tabRegions.Remove(tab);
                _panels.Remove(tab);
            }

            _tabs = _tabs.OrderBy(t => t.Priority).ToList();

            for (var tabIndex = 0; tabIndex < _tabRegions.Count; tabIndex++) {
                var curTab = _tabs[tabIndex];
                _tabRegions[curTab] = TabBoundsFromIndex(tabIndex);
            }

            if (_tabs.Contains(prevTab)) {
                _selectedTabIndex = _tabs.IndexOf(prevTab);
            }

            Invalidate();
        }
        private Rectangle TabBoundsFromIndex(int index) {
            return StandardTabBounds.OffsetBy(-TAB_WIDTH, ContentRegion.Y + index * TAB_HEIGHT);
        }

        #endregion
        
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

            if (_tabs.Count == 0) return;

            var firstTabBounds    = TabBoundsFromIndex(0);
            var selectedTabBounds = _tabRegions[this.SelectedTab];
            var lastTabBounds     = TabBoundsFromIndex(_tabRegions.Count - 1);

            _layoutTopTabBarBounds    = new Rectangle(0, 0,                    TAB_SECTION_WIDTH, firstTabBounds.Top);
            _layoutBottomTabBarBounds = new Rectangle(0, lastTabBounds.Bottom, TAB_SECTION_WIDTH, _size.Y - lastTabBounds.Bottom);

            int topSplitHeight    = selectedTabBounds.Top - ContentRegion.Top;
            int bottomSplitHeight = ContentRegion.Bottom  - selectedTabBounds.Bottom;

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
            foreach (var tab in _tabs) {
                bool active = (i == this.SelectedTabIndex);
                bool hovered = (i == this.HoveredTabIndex);

                var tabBounds = _tabRegions[tab];
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
