using System;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    /// <summary>
    /// A tab displayed by a <see cref="ITabOwner"/> control.
    /// </summary>
    public class Tab {

        /// <summary>
        /// The icon displayed in the tab.
        /// </summary>
        public AsyncTexture2D Icon { get; set; }

        /// <summary>
        /// The order used to determine where in order the tab will be placed.  Tabs are sorted by descending OrderPriority.
        /// </summary>
        public int OrderPriority { get; set; }

        /// <summary>
        /// The name of the tab to be displayed as a tooltip.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A function which returns an <see cref="IView"/> to be displayed when the tab is clicked.
        /// </summary>
        public Func<IView> View { get; set; }

        /// <summary>
        /// Indicates if the tab is enabled.  If not enabled, the tab will not be clickable.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <param name="icon">The icon displayed in the tab.</param>
        /// <param name="view">A function which returns an <see cref="IView"/> to be displayed when the tab is clicked.</param>
        /// <param name="name">The name of the tab to be displayed as a tooltip.</param>
        /// <param name="priority">The order used to determine where in order the tab will be placed.  Tabs are sorted by descending OrderPriority.</param>
        public Tab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null) {
            this.Icon          = icon;
            this.Name          = name;
            this.OrderPriority = priority ?? 0;
            this.View          = view;
        }

        public void Draw(Control tabbedControl, SpriteBatch spriteBatch, Rectangle bounds, bool selected, bool hovered) {
            if (!this.Icon.HasTexture) return;

            // TODO: If not enabled, draw darker to indicate it is disabled

            spriteBatch.DrawOnCtrl(tabbedControl,
                                   Icon,
                                   new Rectangle(bounds.Right  - bounds.Width  / 2 - this.Icon.Texture.Width  / 2,
                                                 bounds.Bottom - bounds.Height / 2 - this.Icon.Texture.Height / 2,
                                                 this.Icon.Texture.Width,
                                                 this.Icon.Texture.Height),
                                   selected || hovered
                                        ? Color.White
                                        : ContentService.Colors.DullColor);
        }

    }
}
