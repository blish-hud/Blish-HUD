using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Menu : Container, IMenuItem {

        private const int DEFAULT_ITEM_HEIGHT = 32;

        #region Load Static

        private static readonly Texture2D _textureMenuItemFade;

        static Menu() {
            _textureMenuItemFade = Content.GetTexture("156044");
        }

        #endregion

        #region Events

        public event EventHandler<ControlActivatedEventArgs> ItemSelected;
        protected virtual void OnItemSelected(ControlActivatedEventArgs e) {
            this.ItemSelected?.Invoke(this, e);
        }

        #endregion

        protected int _menuItemHeight = DEFAULT_ITEM_HEIGHT;
        public int MenuItemHeight {
            get => _menuItemHeight;
            set {
                if (!SetProperty(ref _menuItemHeight, value)) return;

                foreach (var childMenuItem in _children.Cast<IMenuItem>()) {
                    childMenuItem.MenuItemHeight = value;
                }
            }
        }

        protected bool _shouldShift = false;
        public bool ShouldShift {
            get => _shouldShift;
            set => SetProperty(ref _shouldShift, value, true);
        }

        private bool _canSelect;
        public bool CanSelect {
            get => _canSelect;
            set => SetProperty(ref _canSelect, value);
        }

        bool IMenuItem.Selected => false;

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem => _selectedMenuItem;

        void IMenuItem.Select() {
            throw new InvalidOperationException($"The root {nameof(Menu)} instance can not be selected.");
        }

        public void Select(MenuItem menuItem, List<IMenuItem> itemPath) {
            if (!_canSelect) {
                itemPath.ForEach(i => i.Deselect());
                return;
            }

            foreach (var item in this.GetDescendants().Cast<IMenuItem>().Except(itemPath)) {
                item.Deselect();
            }

            _selectedMenuItem = menuItem;

            OnItemSelected(new ControlActivatedEventArgs(menuItem));
        }

        public void Select(MenuItem menuItem) {
            menuItem.Select();
        }

        void IMenuItem.Deselect() {
            Select(null, null);
        }

        protected override void OnResized(ResizedEventArgs e) {
            foreach (var childMenuItem in _children) {
                childMenuItem.Width = e.CurrentSize.X;
            }

            base.OnResized(e);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            if (!(e.ChangedChild is IMenuItem newChild)) {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;

            // Ensure child items remains the same width as us
            //Adhesive.Binding.CreateOneWayBinding(() => e.ChangedChild.Width,
            //                                     () => this.Width, applyLeft: true);

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

            base.OnChildAdded(e);
        }

        public MenuItem AddMenuItem(string text, Texture2D icon = null) {
            return new MenuItem(text) {
                Icon   = icon,
                Parent = this
            };
        }

        public override void UpdateContainer(GameTime gameTime) {
            int totalItemHeight = _children.Where(c => c.Visible).Max(c => c.Bottom);

            this.Height = totalItemHeight;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw items dark every other one
            int totalItemHeight = _children.Where(c => c.Visible).Max(c => c.Bottom);

            for (int sec = 0; sec < totalItemHeight / MenuItemHeight; sec += 2) {
                spriteBatch.DrawOnCtrl(this,
                                       _textureMenuItemFade,
                                       new Rectangle(
                                                     0,
                                                     MenuItemHeight * sec - VerticalScrollOffset,
                                                     _size.X,
                                                     MenuItemHeight
                                                    ),
                                       Color.Black * 0.7f);
            }
        }

    }
}