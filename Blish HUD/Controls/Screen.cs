using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {
    public class Screen:Container {

        public const int MENUUI_BASEINDEX = 30; // Skillbox
        public const int TOOLTIP3D_BASEINDEX = 40;
        public const int WINDOW_BASEZINDEX = 41;
        public const int TOOLWINDOW_BASEZINDEX = 45;
        public const int TOOLTIP_BASEZINDEX = 50;
        public const int CONTEXTMENU_BASEINDEX = 52;

        public Screen() : base() {
            this.Location = new Point(0, 0);
            this.Size = new Point(GameService.Graphics.GraphicsDevice.Viewport.Width, GameService.Graphics.GraphicsDevice.Viewport.Height);
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            // NOOP
        }

        public override bool TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
            List<Control> ZSortedChildren = this.Children.OrderByDescending(i => i.ZIndex).ToList();

            foreach (var childControl in ZSortedChildren) {
                if (childControl.AbsoluteBounds.Contains(ms.Position) && childControl.Visible && childControl.TriggerMouseInput(mouseEventType, ms)) return true;
            }

            return false;
        }

    }
}
