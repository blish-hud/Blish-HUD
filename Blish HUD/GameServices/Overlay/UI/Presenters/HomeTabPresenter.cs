using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Views;

namespace Blish_HUD.Overlay.UI.Presenters {
    public class HomeTabPresenter : Presenter<HomeTabView, OverlayService> {

        /// <inheritdoc />
        public HomeTabPresenter(HomeTabView view, OverlayService model) : base(view, model) { /* NOOP */ }

    }
}
