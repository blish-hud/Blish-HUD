using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    /// <summary>
    /// The TabbedWindow2 is a control meant to replicate the standard Guild Wars 2 windows with tabs.
    /// </summary>
    public class TabbedWindow2 : WindowBase2, ITabOwner {

        private const int TAB_VERTICALOFFSET = 40;

        private const int TAB_HEIGHT = 50;
        private const int TAB_WIDTH  = 84;

        #region Load Static

        private static readonly Texture2D _textureTabActive;

        static TabbedWindow2() {
            _textureTabActive = Content.GetTexture("window-tab-active");
        }

        #endregion

        /// <summary>
        /// Fires when a <see cref="TabbedWindow2"/> Tab changes.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<Tab>> TabChanged;

        /// <summary>
        /// A collection of <see cref="Tab"/> controls, in the order they will be displayed in the window.
        /// </summary>
        public TabCollection Tabs { get; }

        private Tab _selectedTab = null;
        /// <summary>
        /// The actively selected tab.
        /// </summary>
        public Tab SelectedTab {
            get => _selectedTab;
            set {
                var currentTab = _selectedTab;

                if (value != null && !this.Tabs.Contains(value)) return;

                if (SetProperty(ref _selectedTab, value, true)) {
                    OnTabChanged(new ValueChangedEventArgs<Tab>(currentTab, value));
                }
            }
        }

        private Tab HoveredTab { get; set; }

        protected  virtual void OnTabChanged(ValueChangedEventArgs<Tab> e) {
            ShowView(e.NewValue?.View());

            TabChanged?.Invoke(this, e);
        }

        public TabbedWindow2(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) {
            this.Tabs        = new TabCollection(this);
            this.ShowSideBar = true;

            this.ConstructWindow(background, windowRegion, contentRegion);
        }

        protected override void OnClick(MouseEventArgs e) {
            if (this.HoveredTab != null && this.HoveredTab.Enabled) {
                this.SelectedTab = this.HoveredTab;
            }

            base.OnClick(e);
        }

        private void UpdateTabStates() {
            this.SideBarHeight = TAB_VERTICALOFFSET + TAB_HEIGHT * this.Tabs.Count;

            this.HoveredTab = this.MouseOver && this.SidebarActiveBounds.Contains(this.RelativeMousePosition)
                                  ? this.Tabs.FromIndex((this.RelativeMousePosition.Y - this.SidebarActiveBounds.Y - TAB_VERTICALOFFSET) / TAB_HEIGHT)
                                  : null;

            this.BasicTooltipText = this.HoveredTab?.Name;
        }

        public override void UpdateContainer(GameTime gameTime) {
            UpdateTabStates();

            base.UpdateContainer(gameTime);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintAfterChildren(spriteBatch, bounds);

            int tabIndex = 0;
            foreach (var tab in this.Tabs) {
                int tabTop = this.SidebarActiveBounds.Top + TAB_VERTICALOFFSET + tabIndex * TAB_HEIGHT;

                bool selected = tab == this.SelectedTab;
                bool hovered  = tab == this.HoveredTab;

                if (selected) {
                    var tabBounds = new Rectangle(this.SidebarActiveBounds.Left - (TAB_WIDTH - this.SidebarActiveBounds.Width) + 2,
                                                  tabTop,
                                                  TAB_WIDTH,
                                                  TAB_HEIGHT);

                    spriteBatch.DrawOnCtrl(this,
                                           this.WindowBackground,
                                           tabBounds.OffsetBy(2, 0),
                                           new Rectangle(this.WindowRegion.Left + tabBounds.X + 2,
                                                         tabBounds.Y - (int)this.Padding.Top,
                                                         tabBounds.Width,
                                                         tabBounds.Height));

                    spriteBatch.DrawOnCtrl(this, _textureTabActive, tabBounds);
                }

                tab.Draw(this,
                         spriteBatch,
                         new Rectangle(this.SidebarActiveBounds.X,
                                       tabTop,
                                       this.SidebarActiveBounds.Width,
                                       TAB_HEIGHT),
                         selected,
                         hovered);

                tabIndex++;
            }
        }

    }
}
