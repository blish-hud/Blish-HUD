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
            _ = new Panel() {
                ShowBorder = true,
                Width      = buildPanel.Width,
                Height     = buildPanel.Height,
                Title      = "Corner Icons",
                CanScroll  = true,
                Parent     = buildPanel
            };
        }

    }
}
