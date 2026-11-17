using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Overlay.UI.Views {

    /// <summary>
    /// View used to list the registered <see cref="CornerIcon"/>s so that they
    /// can be sorted and hidden.
    /// </summary>
    public class CornerIconsView : View {

        protected override void Build(Container buildPanel) {
            var cornerIconsPanel = new Panel() {
                ShowBorder = true,
                Width      = buildPanel.Width,
                Height     = buildPanel.Height,
                Title      = "Corner Icons",
                CanScroll  = true,
                Parent     = buildPanel
            };

            var cornerIconsList = new Menu() {
                MenuItemHeight = 40,
                Width          = cornerIconsPanel.ContentRegion.Width,
                Parent         = cornerIconsPanel
            };

            foreach (CornerIcon cornerIcon in CornerIcon.All) {
                string name = GetName(cornerIcon);

                _ = new MenuItem(GetDisplayName(name), cornerIcon.Icon) {
                    BasicTooltipText = name,
                    Parent           = cornerIconsList
                };
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
