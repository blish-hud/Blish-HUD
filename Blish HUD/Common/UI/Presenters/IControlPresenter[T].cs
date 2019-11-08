using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Presenters {

    public interface IControlPresenter<TControl> : IPresenter<ControlView<TControl>>, IControlPresenter
        where TControl : Control {

    }

}