using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {
    public class Screen:Container {

        public const int MENUUI_BASEINDEX = 30; // Skillbox
        public const int TOOLTIP3D_BASEINDEX = 40;
        public const int WINDOW_BASEZINDEX = 41;
        public const int TOOLWINDOW_BASEZINDEX = 45;
        public const int TOOLTIP_BASEZINDEX = 55;
        public const int CONTEXTMENU_BASEINDEX = 50;

        /// <inheritdoc />
        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        //public override Control TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
        //    List<Control> ZSortedChildren = _children.OrderByDescending(i => i.ZIndex).ToList();

        //    foreach (var childControl in ZSortedChildren) {
        //        if (childControl.AbsoluteBounds.Contains(ms.Position) && childControl.Visible)
        //            return childControl.TriggerMouseInput(mouseEventType, ms);
        //    }

        //    return null;
        //}

    }
}
