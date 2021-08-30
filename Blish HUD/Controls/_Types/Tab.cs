using System;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Tab {

        public AsyncTexture2D Icon { get; set; }

        public int OrderPriority { get; set; }

        public string Name { get; set; }

        public Func<IView> View { get; set; }

        public bool Enabled { get; set; } = true;

        public Tab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null) {
            this.Icon          = icon;
            this.Name          = name;
            this.OrderPriority = priority ?? icon.GetHashCode();
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
