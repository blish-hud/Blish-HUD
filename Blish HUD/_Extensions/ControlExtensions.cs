using Blish_HUD.Controls;

namespace Blish_HUD {
    public static class ControlExtensions {

        public static bool IsChildOf(this Control control, Container container) {
            return control.Parent != null && IsChildOf(control.Parent, container);
        }

    }
}
