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

        private const int ITEM_WIDTH = 135;
        private const int ITEM_HEIGHT = 22;
        private const int ITEM_VERTICALMARGIN = 6;

        private const int CONTROL_WIDTH = BORDER_PADDING + ITEM_WIDTH + BORDER_PADDING;

        private const int CLICK_DEBOUNCE = 100;

        #region Load Static

        private static readonly List<WeakReference<ContextMenuStrip>> _contextMenuStrips = new List<WeakReference<ContextMenuStrip>>();

        private static readonly Texture2D _textureMenuEdge = Content.GetTexture("scrollbar-track");

        private static double _lastOpenTime;

        static ContextMenuStrip() {
            Input.Mouse.LeftMouseButtonPressed += HandleMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += HandleMouseButtonPressed;
        }

        private static void RegisterContextMenuStrip(ContextMenuStrip contextMenuStrip) {
            lock (_contextMenuStrips) {
                _contextMenuStrips.Add(new WeakReference<ContextMenuStrip>(contextMenuStrip));
            }
        }

        private static void HandleMouseButtonPressed(object sender, MouseEventArgs e) {
            // Debounce to prevent mistakenly closing the menu immediatley after opening (or when
            // this event is triggered after the same event that triggered it to open)
            if (GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _lastOpenTime < CLICK_DEBOUNCE) return;

            lock (_contextMenuStrips) {
                WeakReference<ContextMenuStrip>[] allMenuStrips = _contextMenuStrips.ToArray();

                if (Input.Mouse.ActiveControl is ContextMenuStripItem { CanCheck: true } || Input.Mouse.ActiveControl is ContextMenuStrip) return;

                foreach (var cmsRef in allMenuStrips) {
                    if (!cmsRef.TryGetTarget(out var cms)) {
                        _contextMenuStrips.Remove(cmsRef);
                        continue;
                    }

                    if (!cms.Visible) continue;

                    if (!cms.MouseOver) cms.Hide();
                }
            }
        }

        #endregion

        private (Point Position, int DownOffset, int UpOffset) _targetOffset;

        protected Func<IEnumerable<ContextMenuStripItem>> GetItemsDelegate { get; private set; }

        public ContextMenuStrip() {
            this.Visible = false;
            this.Width = CONTROL_WIDTH;
            this.ZIndex = Screen.CONTEXTMENU_BASEINDEX;

            RegisterContextMenuStrip(this);
        }

        public ContextMenuStrip(Func<IEnumerable<ContextMenuStripItem>> getItemsDelegate) : this() {
            this.GetItemsDelegate = getItemsDelegate;
        }

        protected override void OnShown(EventArgs e) {
            // Keep track of when we opened for debounce
            _lastOpenTime = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

            if (this.GetItemsDelegate != null) {
                AddMenuItems(this.GetItemsDelegate());
            }

            this.Parent = GameService.Graphics.SpriteScreen;

            // If we have no children, don't display (and don't even call 'Shown' event)
            if (_children.IsEmpty) {
                this.Visible = false;
                return;
            }

            base.OnShown(e);
        }

        protected override void OnHidden(EventArgs e) {
            this.Parent = null;

            if (this.GetItemsDelegate != null) {
                ClearChildren();
            }

            base.OnHidden(e);
        }

        private int GetVerticalOffset(int yStart, int downOffset = 0, int upOffset = 0) {
            int yUnderDef = Graphics.SpriteScreen.Bottom - (yStart + _size.Y);
            int yAboveDef = Graphics.SpriteScreen.Top + (yStart - _size.Y);

            return yUnderDef > 0 || yUnderDef > yAboveDef
                       // flip down
                       ? yStart + upOffset
                       // flip up
                       : yStart - _size.Y + downOffset;
        }

        private void SetPositionFromOffset((Point Position, int DownOffset, int UpOffset) offset) {
            this.Location = new Point(offset.Position.X, GetVerticalOffset(offset.Position.Y, offset.DownOffset, offset.UpOffset));
        }

        public void Show(Point position) {
            SetPositionFromOffset(_targetOffset = (position, 0, 0));

            base.Show();
        }

        public void Show(Control activeControl) {
            if (activeControl is ContextMenuStripItem parentMenu) {
                SetPositionFromOffset(_targetOffset = (new Point(parentMenu.AbsoluteBounds.Right - 3, parentMenu.AbsoluteBounds.Top), 19, 0));

                this.ZIndex = parentMenu.Parent.ZIndex + 1;
            } else {
                SetPositionFromOffset(_targetOffset = (activeControl.AbsoluteBounds.Location, 0, activeControl.Height));
            }

            base.Show();
        }

        public override void Hide() {
            var children = _children.ToArray();
            foreach (var cmsiChild in children.Select(otherChild => otherChild as ContextMenuStripItem)) {
				if (cmsiChild is { Submenu: { MouseOver: false } }) {
                    cmsiChild.Submenu.Hide();
                }
            }

            this.Visible = false;
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);
            OnChildMembershipChanged(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);
            OnChildMembershipChanged(e);
        }

        public ContextMenuStripItem AddMenuItem(string text) {
            return new ContextMenuStripItem() {
                Text = text,
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
                newChild.Resized += ChildOnResized;
            } else {
                e.ChangedChild.MouseEntered -= ChildOnMouseEntered;
                e.ChangedChild.Resized -= ChildOnResized;
            }

            if (this.Visible) {
                SetPositionFromOffset(_targetOffset);
            }

            this.Invalidate();
        }

        private void ChildOnMouseEntered(object sender, MouseEventArgs e) {
            // Stop showing submenus if adjacent menu items are moused over
            var children = _children.ToArray();
            foreach (var ocCmsi in children.Select(otherChild => otherChild as ContextMenuStripItem)) {
                ocCmsi?.Submenu?.Hide();
            }

            // And then make sure we're showing just the first level of the moused-over submenu
            if (sender is ContextMenuStripItem cmsi) {
                cmsi.Submenu?.Show();
            }
        }

        private void ChildOnResized(object sender, ResizedEventArgs e) {
            this.Invalidate();
        }

        public override void RecalculateLayout() {
            if (!_children.IsEmpty) {
                int maxChildWidth = CONTROL_WIDTH;

                int lastChildBottom = BORDER_PADDING - ITEM_VERTICALMARGIN;

                foreach (var menuItem in _children.Where(c => c.Visible)) {
                    maxChildWidth = Math.Max(menuItem.Width, maxChildWidth);

                    menuItem.Location = new Point(BORDER_PADDING, lastChildBottom + ITEM_VERTICALMARGIN);

                    lastChildBottom = menuItem.Bottom;
                }

                _size = new Point(maxChildWidth + BORDER_PADDING * 2,
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
                                   new Rectangle(0, 1, _textureMenuEdge.Width, _size.Y - 2),
                                   Color.White * 0.8f);
        }
    }

}
