using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Overlay.UI.Views {

    /// <summary>
    /// View used to list the registered <see cref="CornerIcon"/>s so that they
    /// can be sorted and decluttered.
    /// </summary>
    public class CornerIconsView : View {

        private const int MENU_ITEM_HEIGHT = 40;
        private const int ROW_PADDING      = 12;

        // Width of a MenuItem checkbox plus its padding, used to line up exempt icons.
        private const int CHECKBOX_SPACE = 42;

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
                Text      = "Check an icon to declutter it from the corner icon bar.",
                Width     = contentWidth,
                Height    = 28,
                TextColor = Color.LightGray,
                Parent    = cornerIconsPanel
            };

            var cornerIconsList = new Menu() {
                MenuItemHeight = MENU_ITEM_HEIGHT,
                Width          = contentWidth,
                Top            = hintLabel.Bottom,
                Parent         = cornerIconsPanel
            };

            foreach (CornerIcon cornerIcon in CornerIcon.All) {
                CornerIcon currentCornerIcon = cornerIcon;
                string     name              = cornerIcon.Name;

                var menuItem = new MenuItem(string.IsNullOrWhiteSpace(name) ? "(Unnamed)" : name, cornerIcon.Icon) {
                    CanCheck         = !cornerIcon.Exempt,
                    Checked          = cornerIcon.Decluttered,
                    LeftPadding      = cornerIcon.Exempt ? CHECKBOX_SPACE : 0,
                    BasicTooltipText = cornerIcon.Exempt
                                           ? $"{name}\n\nThis icon is exempt and can not be decluttered."
                                           : name,
                    Parent           = cornerIconsList
                };

                menuItem.CheckedChanged += (sender, e) => currentCornerIcon.Decluttered = e.Checked;
            }
        }

    }
}
