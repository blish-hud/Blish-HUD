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
                _ = new MenuItem(GetDisplayName(cornerIcon), cornerIcon.Icon) {
                    BasicTooltipText = cornerIcon.IconName,
                    Parent           = cornerIconsList
                };
            }
        }

        private static string GetDisplayName(CornerIcon cornerIcon) {
            if (string.IsNullOrWhiteSpace(cornerIcon.IconName)) {
                return "(Unnamed)";
            }

            // Corner icon names may contain additional lines (such as hint text).
            return cornerIcon.IconName.Split('\n')[0].Trim();
        }

    }
}
