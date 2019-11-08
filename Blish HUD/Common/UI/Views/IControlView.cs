using Blish_HUD.Controls;

namespace Blish_HUD.Common.UI.Views {

    public interface IControlView<out TControl> where TControl : Control {

        TControl Control { get; }

    }

}