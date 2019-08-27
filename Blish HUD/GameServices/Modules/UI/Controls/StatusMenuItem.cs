using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.UI.Controls {
    public class StatusMenuItem : MenuItem {

        private string _statusText;
        private Color  _statusTextColor = Color.White;

        public string StatusText {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public Color StatusTextColor {
            get => _statusTextColor;
            set => SetProperty(ref _statusTextColor, value);
        }

        /// <inheritdoc />
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, _statusText, Content.DefaultFont12, new Rectangle(bounds.X, bounds.Y, bounds.Width - 20, bounds.Height), _statusTextColor, false, true, 1, HorizontalAlignment.Right);
        }

    }
}
