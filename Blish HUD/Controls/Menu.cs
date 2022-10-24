using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Menu : Container, IMenuItem {

        private const int DEFAULT_ITEM_HEIGHT = 32;

        #region Textures

        private readonly AsyncTexture2D _textureMenuItemFade = AsyncTexture2D.FromAssetId(156044);

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

            e.ChangedChild.Width = this.Width;

            // We'll bind the top of the control to the bottom of the last control we added
            var lastItem = _children.LastOrDefault();
            if (lastItem != null) {
                // Handler will be removed again when the underlying object is being disposed
                lastItem.PropertyChanged += (_, args) => {
                    if (args.PropertyName == "Bottom") {
                        e.ChangedChild.Top = lastItem.Bottom;
                    }
                };
                
                e.ChangedChild.Top = lastItem.Bottom;
            }

            ShouldShift = e.ResultingChildren.Any(mi => {
                                                      MenuItem cmi = (MenuItem) mi;

                                                      return cmi.CanCheck || cmi.Icon != null || cmi.Children.Any();
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
            int totalItemHeight = 0;

            foreach (var child in _children) {
                totalItemHeight = Math.Max(child.Bottom, totalItemHeight);
            }

            this.Height = totalItemHeight;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw items dark every other one
            for (int sec = 0; sec < _size.Y / MenuItemHeight; sec += 2) {
                spriteBatch.DrawOnCtrl(this,
                                       _textureMenuItemFade.Texture,
                                       new Rectangle(0,
                                                     MenuItemHeight * sec - VerticalScrollOffset,
                                                     _size.X,
                                                     MenuItemHeight),
                                       Color.Black * 0.7f);
            }
        }

        public override void RecalculateLayout() {
            int lastBottom = 0;

            foreach (var child in _children.Where(c => c.Visible)) {
                child.Location = new Point(0, lastBottom);
                child.Width = this.Width;

                lastBottom = child.Bottom;
            }
        }
    }
}