using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Menu : Container, IMenuItem {

        private const int DEFAULT_ITEM_HEIGHT = 32;

        public event EventHandler<EventArgs> ItemSelected;

        protected int _menuItemHeight = DEFAULT_ITEM_HEIGHT;
        public int MenuItemHeight {
            get => _menuItemHeight;
            set {
                if (SetProperty(ref _menuItemHeight, value)) {
                    foreach (var control in _children) {
                        var childMenuItem = (IMenuItem) control;

                        childMenuItem.MenuItemHeight = value;
                    }
                }
            }
        }

        protected bool _shouldShift = false;
        public bool ShouldShift {
            get => _shouldShift;
            set => SetProperty(ref _shouldShift, value, true);
        }

        public bool Selected { get; set; }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            if (!(e.ChangedChild is IMenuItem newChild)) {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;

            // Ensure child items remains the same width as us
            Adhesive.Binding.CreateOneWayBinding(() => e.ChangedChild.Width,
                                                 () => this.Width, applyLeft: true);

            // We'll bind the top of the control to the bottom of the last control we added
            var lastItem = _children.LastOrDefault();
            if (lastItem != null) {
                Adhesive.Binding.CreateOneWayBinding(() => e.ChangedChild.Top,
                                                     () => lastItem.Bottom, applyLeft: true);
            }

            ShouldShift = e.ResultingChildren.Any(mi => {
                                                      MenuItem cmi = (MenuItem) mi;

                                                      return cmi.CanCheck || cmi.Icon != null || cmi.Children.Any() ;
                                                  });
        }

        public MenuItem AddMenuItem(string text, Texture2D icon = null) {
            return new MenuItem(text) {
                Icon   = icon,
                Parent = this
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw items dark every other one
            int totalItemHeight = _children.Where(c => c.Visible).Max(c => c.Bottom);

            for (int sec = 0; sec < totalItemHeight / MenuItemHeight; sec += 2) {
                spriteBatch.DrawOnCtrl(
                                       this,
                                       Content.GetTexture("156044"),
                                       new Rectangle(
                                                     0,
                                                     MenuItemHeight * sec - VerticalScrollOffset,
                                                     _size.X,
                                                     MenuItemHeight
                                                    ),
                                       Color.Black * 0.7f
                                      );
            }
        }

    }
}