using Blish_HUD.Common.UI.Presenters;
using Blish_HUD.Controls;

namespace Blish_HUD.Common.UI.Views {

    public static class ControlView {

        public static ControlView<TControl> FromControl<TControl>(TControl control) where TControl : Control {
            return new ControlView<TControl>(control);
        }

        public static ControlView<TControl> FromControl<TControl>(TControl control, IControlPresenter presenter) where TControl : Control {
            return new ControlView<TControl>(control, presenter);
        }

    }

}
