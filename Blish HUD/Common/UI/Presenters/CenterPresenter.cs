using Blish_HUD.Common.UI.Views;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Presenters {
    public class CenterPresenter : Presenter<CenteredView, IView> {

        /// <inheritdoc />
        public CenterPresenter(CenteredView view, IView model) : base(view, model) { /* NOOP */ }

    }
}
