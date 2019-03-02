using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Praeclarum.Bind;

namespace Blish_HUD.Controls {
    public class Menu : Container, IMenuItem {

        private const int DEFAULT_ITEM_HEIGHT = 32;

        public event EventHandler<EventArgs> ItemSelected;

        private int _menuItemHeight = DEFAULT_ITEM_HEIGHT;
        public int MenuItemHeight {
            get => _menuItemHeight;
            set {
                if (_menuItemHeight == value) return;

                _menuItemHeight = value;

                foreach (var control in this.Children) {
                    var childMenuItem = (IMenuItem)control;

                    childMenuItem.MenuItemHeight = value;
                }

                OnPropertyChanged();
            }
        }

        public bool Selected { get; set; }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            if (!(e.ChangedChild is IMenuItem newChild)) {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;

            var allChildItems = new List<IMenuItem>(this.Children.Select(c => (IMenuItem)c).ToList<IMenuItem>()) {
                newChild
            };

            // Ensure child items remains the same width as us
            Binding.Create(() => e.ChangedChild.Width == this.Width);

            // We'll bind the top of the control to the bottom of the last control we added
            var lastItem = this.Children.LastOrDefault();
            if (lastItem != null) {
                Binding.Create(() => e.ChangedChild.Top == lastItem.Top + lastItem.Height /* complex binding to break 2-way bind */);
            }
            //this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this.Width, allChildItems.Max(c => ((Control)c).Bottom));
        }

        public MenuItem AddMenuItem(string text) {
            return new MenuItem(text) {
                Parent = this
            };
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            // No back tints to draw if we have no items
            if (!this.Children.Any()) return;

            // Draw items dark every other one
            int totalItemHeight = this.Children.Max(c => c.Bottom);

            for (int sec = 0; sec < totalItemHeight / this.MenuItemHeight; sec += 2) {
                spriteBatch.Draw(Content.GetTexture("156044"), new Rectangle(this.ContentRegion.Left, this.ContentRegion.Top + this.MenuItemHeight * sec - this.VerticalScrollOffset, this.ContentRegion.Width, this.MenuItemHeight), Color.Black * 0.7f);
            }
        }

    }
}