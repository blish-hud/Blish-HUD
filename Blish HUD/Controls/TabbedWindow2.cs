using System;
using Blish_HUD.Controls._Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class TabbedWindow2 : WindowBase2 {

        private const int TAB_HEIGHT    = 52;
        private const int TAB_WIDTH     = 104;
        private const int TAB_ICON_SIZE = 32;

        public event EventHandler<ValueChangedEventArgs<WindowTab>> TabChanged;

        private int _selectedTabIndex = -1;
        public int SelectedTabIndex {
            get => _selectedTabIndex;
            set {
                int lastTab = Tabs[_selectedTabIndex];

                if (SetProperty(ref _selectedTabIndex, value, true)) {
                    OnTabChanged(_tabs);
                }
            }
        }

        public TabCollection Tabs { get; } = new TabCollection();

        protected virtual void OnTabChanged(ValueChangedEventArgs<int> e) {

            this.TabChanged?.Invoke(this, e);
        }

        public TabbedWindow2(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) {
            this.ConstructWindow(background, windowRegion, contentRegion);
        }

    }
}
