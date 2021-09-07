using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    /// <summary>
    /// The StandardWindow is a control meant to replicate the standard Guild Wars 2 windows.
    /// </summary>
    public class StandardWindow : WindowBase2 {

        public StandardWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) {
            this.ConstructWindow(background, windowRegion, contentRegion);
        }

        /// <summary>
        /// Shows the window with the provided view.
        /// </summary>
        public void Show(IView view) {
            this.ShowView(view);
            base.Show();
        }

        /// <summary>
        /// Shows the window with the provided view if it is hidden.
        /// Hides the window if it is currently showing.
        /// </summary>
        public void ToggleWindow(IView view) {
            if (this.Visible) {
                Show(view);
            } else {
                Hide();
            }
        }

    }
}
