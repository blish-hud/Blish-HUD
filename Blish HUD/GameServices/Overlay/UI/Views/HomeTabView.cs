using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Presenters;

namespace Blish_HUD.Overlay.UI.Views {
    public class HomeTabView : View<HomeTabPresenter> {

        public HomeTabView() {
            this.Presenter = new HomeTabPresenter(this, GameService.Overlay);
        }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            return Task.FromResult(true);
        }

    }
}
