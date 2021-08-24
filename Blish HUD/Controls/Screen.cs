namespace Blish_HUD.Controls {
    public class Screen:Container {

        public const int MENUUI_BASEINDEX      = 30; // Skillbox
        public const int TOOLTIP3D_BASEINDEX   = 40;
        public const int WINDOW_BASEZINDEX     = 41;
        public const int TOOLWINDOW_BASEZINDEX = 45;
        public const int CONTEXTMENU_BASEINDEX = 50;
        public const int DROPDOWN_BASEINDEX    = int.MaxValue - 64;
        public const int TOOLTIP_BASEZINDEX    = int.MaxValue - 32;
        
        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

    }
}
