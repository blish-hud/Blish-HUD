using System.Linq;
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
                string     name              = GetName(cornerIcon);

                var menuItem = new MenuItem(GetDisplayName(name), cornerIcon.Icon) {
                    CanCheck         = true,
                    Checked          = cornerIcon.Decluttered,
                    BasicTooltipText = name,
                    Parent           = cornerIconsList
                };

                menuItem.CheckedChanged += (sender, e) => currentCornerIcon.Decluttered = e.Checked;
            }
        }

        /// <summary>
        /// Corner icons may be named through <see cref="CornerIcon.IconName"/> or,
        /// as some modules do, through <see cref="Control.BasicTooltipText"/>.
        /// </summary>
        private static string GetName(CornerIcon cornerIcon) {
            if (!string.IsNullOrWhiteSpace(cornerIcon.IconName)) {
                return cornerIcon.IconName;
            }

            return !string.IsNullOrWhiteSpace(cornerIcon.BasicTooltipText)
                       ? cornerIcon.BasicTooltipText
                       : null;
        }

        private static string GetDisplayName(string name) {
            // Names may contain additional lines (such as hint text), so use the first non-empty one.
            string firstLine = name?.Split('\n')
                                    .Select(line => line.Trim())
                                    .FirstOrDefault(line => line.Length > 0);

            return string.IsNullOrWhiteSpace(firstLine)
                       ? "(Unnamed)"
                       : firstLine;
        }

    }
}
