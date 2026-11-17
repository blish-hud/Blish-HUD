using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Overlay.UI.Views {

    /// <summary>
    /// View used to list the registered <see cref="CornerIcon"/>s so that they
    /// can be rearranged and decluttered.
    /// </summary>
    public class CornerIconsView : View {

        private const int MENU_ITEM_HEIGHT   = 40;
        private const int ROW_PADDING        = 12;
        private const int CHECKBOX_SPACE     = 42;
        private const int MOVE_BUTTONS_SPACE = 68;

        // Set to true to place the move arrows between the checkbox and the icon
        // instead of on the far right of the row.
        private const bool MOVE_BUTTONS_ON_LEFT = false;

        private Menu _cornerIconsList;

        private readonly List<CornerIconMenuItem> _cornerIconMenuItems = new List<CornerIconMenuItem>();

        protected override void Build(Container buildPanel) {
            var cornerIconsPanel = new Panel() {
                ShowBorder = true,
                Width      = buildPanel.Width,
                Height     = buildPanel.Height,
                Title      = "Corner Icons",
                CanScroll  = true,
                Parent     = buildPanel
            };

            int contentWidth = cornerIconsPanel.ContentRegion.Width - (ROW_PADDING * 2);

            var hintLabel = new Label() {
                Text      = "Check an icon to declutter it. Use the arrows to move it.",
                Width     = contentWidth,
                Height    = 28,
                TextColor = Color.LightGray,
                Parent    = cornerIconsPanel
            };

            _cornerIconsList = new Menu() {
                MenuItemHeight = MENU_ITEM_HEIGHT,
                Width          = contentWidth,
                Top            = hintLabel.Bottom,
                Parent         = cornerIconsPanel
            };

            BuildCornerIconMenuItems();
        }

        private void BuildCornerIconMenuItems() {
            List<CornerIcon> orderedIcons = GetOrderedIcons();

            foreach (CornerIcon cornerIcon in CornerIcon.All) {
                CornerIcon currentCornerIcon = cornerIcon;
                string     name              = cornerIcon.Name;

                int index = cornerIcon.Exempt
                                ? -1
                                : orderedIcons.IndexOf(cornerIcon);

                var menuItem = new CornerIconMenuItem(string.IsNullOrWhiteSpace(name) ? "(Unnamed)" : name,
                                                      cornerIcon,
                                                      cornerIcon.Exempt
                                                          ? null
                                                          : new Action<int>(offset => MoveCornerIcon(currentCornerIcon, offset))) {
                    CanCheck         = !cornerIcon.Exempt,
                    Checked          = cornerIcon.Decluttered,
                    LeftPadding      = cornerIcon.Exempt
                                           ? CHECKBOX_SPACE + (MOVE_BUTTONS_ON_LEFT ? MOVE_BUTTONS_SPACE : 0)
                                           : 0,
                    CanMoveUp        = index > 0,
                    CanMoveDown      = index >= 0 && index < orderedIcons.Count - 1,
                    BasicTooltipText = cornerIcon.Exempt
                                           ? $"{name}\n\nThis icon is exempt and can not be moved or decluttered."
                                           : name,
                    Parent           = _cornerIconsList
                };

                menuItem.CheckedChanged += (sender, e) => currentCornerIcon.Decluttered = e.Checked;

                _cornerIconMenuItems.Add(menuItem);
            }
        }

        private void RefreshCornerIconMenuItems() {
            if (_cornerIconsList?.Parent == null) return;

            foreach (CornerIconMenuItem menuItem in _cornerIconMenuItems) {
                menuItem.Dispose();
            }

            _cornerIconMenuItems.Clear();

            BuildCornerIconMenuItems();
        }

        private void MoveCornerIcon(CornerIcon cornerIcon, int offset) {
            List<CornerIcon> orderedIcons = GetOrderedIcons();
            int              index        = orderedIcons.IndexOf(cornerIcon);
            int              newIndex     = index + offset;

            if (index < 0 || newIndex < 0 || newIndex >= orderedIcons.Count) return;

            CornerIcon swappedIcon  = orderedIcons[newIndex];
            orderedIcons[newIndex]  = orderedIcons[index];
            orderedIcons[index]     = swappedIcon;

            // Renumber priorities so that the icon order is stable in the current session.
            int priority = int.MaxValue - 100;

            foreach (CornerIcon orderedIcon in orderedIcons) {
                orderedIcon.Priority = priority;
                priority            -= 10;
            }

            // Let the click finish before rebuilding the list.
            GameService.Overlay.QueueMainThreadUpdate(_ => RefreshCornerIconMenuItems());
        }

        private static List<CornerIcon> GetOrderedIcons() {
            return CornerIcon.All.Where(cornerIcon => !cornerIcon.Exempt).ToList();
        }

        protected override void Unload() {
            foreach (CornerIconMenuItem menuItem in _cornerIconMenuItems) {
                menuItem.Dispose();
            }

            _cornerIconMenuItems.Clear();

            base.Unload();
        }

        /// <summary>
        /// A <see cref="MenuItem"/> with move up/down arrow buttons on its right side.
        /// </summary>
        private sealed class CornerIconMenuItem : MenuItem {

            private const int ARROW_SIZE    = 24;
            private const int ARROW_MARGIN  = 8;
            private const int ARROW_SPACING = 4;

            private static readonly AsyncTexture2D _textureArrow = AsyncTexture2D.FromAssetId(156057);

            private readonly Action<int> _moveAction;

            public bool CanMoveUp   { get; set; }
            public bool CanMoveDown { get; set; }

            public CornerIconMenuItem(string text, CornerIcon cornerIcon, Action<int> moveAction) : base(text, cornerIcon.Icon) {
                _moveAction = moveAction;

                if (_moveAction == null) return;

                if (CornerIconsView.MOVE_BUTTONS_ON_LEFT) {
                    this.IconPadding = CornerIconsView.MOVE_BUTTONS_SPACE;
                } else {
                    this.RightPadding = CornerIconsView.MOVE_BUTTONS_SPACE;
                }
            }

            private Rectangle UpButtonBounds {
                get {
                    int left = CornerIconsView.MOVE_BUTTONS_ON_LEFT
                                   ? this.LeftSidePadding + ICON_SIZE + ICON_PADDING + ARROW_MARGIN
                                   : this.Width - ARROW_MARGIN - ARROW_SIZE - ARROW_SPACING - ARROW_SIZE;

                    return new Rectangle(left,
                                         this.Height / 2 - ARROW_SIZE / 2,
                                         ARROW_SIZE,
                                         ARROW_SIZE);
                }
            }

            private Rectangle DownButtonBounds => new Rectangle(this.UpButtonBounds.Right + ARROW_SPACING,
                                                                this.Height / 2 - ARROW_SIZE / 2,
                                                                ARROW_SIZE,
                                                                ARROW_SIZE);

            public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
                base.PaintBeforeChildren(spriteBatch, bounds);

                if (_moveAction == null) return;

                DrawMoveArrow(spriteBatch, this.UpButtonBounds,    MathHelper.Pi, this.CanMoveUp);
                DrawMoveArrow(spriteBatch, this.DownButtonBounds,  0f,            this.CanMoveDown);
            }

            private void DrawMoveArrow(SpriteBatch spriteBatch, Rectangle bounds, float rotation, bool enabled) {
                // MonoGame treats the destination rectangle's location as the rotation pivot, so
                // pivot the draw around the center of the clickable bounds and rotate around the
                // center of the actual texture.
                var drawBounds = new Rectangle(bounds.Center.X, bounds.Center.Y, bounds.Width, bounds.Height);
                var origin     = new Vector2(_textureArrow.Width / 2f, _textureArrow.Height / 2f);

                spriteBatch.DrawOnCtrl(this,
                                       _textureArrow,
                                       drawBounds,
                                       null,
                                       Color.White * (enabled ? 0.85f : 0.2f),
                                       rotation,
                                       origin);
            }

            protected override void OnClick(MouseEventArgs e) {
                if (_moveAction != null) {
                    if (this.UpButtonBounds.Contains(this.RelativeMousePosition)) {
                        if (this.CanMoveUp) {
                            Content.PlaySoundEffectByName("button-click");
                            _moveAction(-1);
                        }

                        return;
                    }

                    if (this.DownButtonBounds.Contains(this.RelativeMousePosition)) {
                        if (this.CanMoveDown) {
                            Content.PlaySoundEffectByName("button-click");
                            _moveAction(1);
                        }

                        return;
                    }
                }

                base.OnClick(e);
            }

        }

    }
}
