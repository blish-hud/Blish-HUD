using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Represents a right-click shortcut menu.  Can be assigned to <see cref="Control.Menu"/>.
    /// </summary>
    public class ContextMenuStrip : Container {

        private const int BORDER_PADDING = 2;

        private const int ITEM_WIDTH          = 135;
        private const int ITEM_HEIGHT         = 22;
        private const int ITEM_VERTICALMARGIN = 6;

        private const int CONTROL_WIDTH = BORDER_PADDING + ITEM_WIDTH + BORDER_PADDING;

        #region Load Static

        private static readonly Texture2D _textureMenuEdge;

        static ContextMenuStrip() {
            _textureMenuEdge = Content.GetTexture("scrollbar-track");
        }

        #endregion

        public ContextMenuStrip() {
            this.Visible = false;
            this.Width   = CONTROL_WIDTH;
            this.ZIndex  = Screen.CONTEXTMENU_BASEINDEX;

            Input.Mouse.LeftMouseButtonPressed  += MouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += MouseButtonPressed;
        }

        protected override void OnShown(EventArgs e) {
            this.Parent = GameService.Graphics.SpriteScreen;

            // If we have no children, don't display (and don't even call 'Shown' event)
            if (!_children.Any()) {
                this.Visible = false;
                return;
            }
            
            base.OnShown(e);
        }

        /// <inheritdoc />
        protected override void OnHidden(EventArgs e) {
            this.Parent = null;

            base.OnHidden(e);
        }

        private int GetVerticalOffset(int yStart, int downOffset = 0, int upOffset = 0) {
            int yUnderDef = Graphics.SpriteScreen.Bottom - (yStart + _size.Y);
            int yAboveDef = Graphics.SpriteScreen.Top    + (yStart - _size.Y);

            return yUnderDef > 0 || yUnderDef > yAboveDef
                       // flip down
                       ? yStart + upOffset
                       // flip up
                       : yStart - _size.Y + downOffset;
        }

        public void Show(Point position) {
            this.Location = new Point(position.X, GetVerticalOffset(position.Y));
            
            base.Show();
        }

        public void Show(Control activeControl) {
            if (activeControl is ContextMenuStripItem parentMenu) {
                this.Location = new Point(parentMenu.AbsoluteBounds.Right - 3, GetVerticalOffset(parentMenu.AbsoluteBounds.Top, 19));
                this.ZIndex = parentMenu.Parent.ZIndex + 1;
            } else {
                (int x, int y) = activeControl.AbsoluteBounds.Location;

                this.Location = new Point(x, GetVerticalOffset(y, 0, activeControl.Height));
            }

            base.Show();
        }

        public override void Hide() {
            this.Visible = false;

            foreach (var cmsiChild in this.Children.Select(otherChild => otherChild as ContextMenuStripItem)) {
                cmsiChild?.Submenu?.Hide();
            }
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);
            OnChildMembershipChanged(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);
            OnChildMembershipChanged(e);
        }

        private void MouseButtonPressed(object sender, MouseEventArgs e) {
            if (!this.Visible) return;
            if (Input.Mouse.ActiveControl is ContextMenuStripItem menuStrip && menuStrip.CanCheck) return;

            if (!this.MouseOver)
                this.Hide();
        }

        public ContextMenuStripItem AddMenuItem(string text) {
            return new ContextMenuStripItem() {
                Text   = text,
                Parent = this
            };
        }

        public ContextMenuStripItem AddMenuItem(ContextMenuStripItem item) {
            item.Parent = this;

            return item;
        }

        public void AddMenuItems(IEnumerable<ContextMenuStripItem> items) {
            foreach (var item in items) {
                item.Parent = this;
            }
        }

        private void OnChildMembershipChanged(ChildChangedEventArgs e) {
            if (e.Added) {
                if (!(e.ChangedChild is ContextMenuStripItem newChild)) {
                    e.Cancel = true;
                    return;
                }

                newChild.Height = ITEM_HEIGHT;

                newChild.MouseEntered += ChildOnMouseEntered;
                newChild.Resized      += ChildOnResized;
            } else {
                e.ChangedChild.MouseEntered -= ChildOnMouseEntered;
                e.ChangedChild.Resized      -= ChildOnResized;
            }

            this.Invalidate();
        }

        private void ChildOnMouseEntered(object sender, MouseEventArgs e) {
            // Stop showing submenus if adjacent menu items are moused over
            foreach (var ocCmsi in _children.Except(new[] { sender }).Select(otherChild => otherChild as ContextMenuStripItem)) {
                ocCmsi?.Submenu?.Hide();
            }
        }

        private void ChildOnResized(object sender, ResizedEventArgs e) {
            this.Invalidate();
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Filter;
        }

        public override void RecalculateLayout() {
            if (_children.Any()) {
                int maxChildWidth = CONTROL_WIDTH;

                int lastChildBottom = BORDER_PADDING - ITEM_VERTICALMARGIN;

                foreach (var menuItem in _children.Where(c => c.Visible)) {
                    maxChildWidth = Math.Max(menuItem.Width, maxChildWidth);

                    menuItem.Location = new Point(BORDER_PADDING, lastChildBottom + ITEM_VERTICALMARGIN);

                    lastChildBottom = menuItem.Bottom;
                }

                _size = new Point(maxChildWidth   + BORDER_PADDING * 2,
                                  lastChildBottom + BORDER_PADDING);

                foreach (var childItem in this.Children) {
                    childItem.Width = maxChildWidth;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(BORDER_PADDING,
                                                 BORDER_PADDING,
                                                 _size.X - BORDER_PADDING * 2,
                                                 _size.Y - BORDER_PADDING * 2),
                                   Color.FromNonPremultiplied(33, 32, 33, 255));

            // Left line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(0, 1, _textureMenuEdge.Width, _size.Y - BORDER_PADDING),
                                   new Rectangle(0, 1, _textureMenuEdge.Width, _size.Y - BORDER_PADDING),
                                   Color.White * 0.8f);

            // Top line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(1, BORDER_PADDING, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   new Rectangle(1, BORDER_PADDING, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   Color.White * 0.8f,
                                   -MathHelper.PiOver2,
                                   Vector2.Zero);

            // Bottom line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(1, _size.Y, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   new Rectangle(1, BORDER_PADDING, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   Color.White * 0.8f,
                                   -MathHelper.PiOver2,
                                   Vector2.Zero);

            // Right line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(_size.X - _textureMenuEdge.Width, 1, _textureMenuEdge.Width, _size.Y - 2),
                                   new Rectangle(0,                           1, _textureMenuEdge.Width, _size.Y - 2),
                                   Color.White * 0.8f);
        }
    }

}
