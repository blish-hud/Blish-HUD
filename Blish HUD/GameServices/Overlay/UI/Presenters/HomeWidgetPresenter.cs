using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Views.Widgets;

namespace Blish_HUD.Overlay.UI.Presenters {

    public class HomeWidgetPresenter : Presenter<RepeatedView<IEnumerable<IHomeWidgetView>>, IEnumerable<IHomeWidgetView>> {

        /// <inheritdoc />
        public HomeWidgetPresenter(RepeatedView<IEnumerable<IHomeWidgetView>> view, IEnumerable<IHomeWidgetView> model) : base(view, model) { /* NOOP */ }
        
        /// <inheritdoc />
        protected override void UpdateView() {
            foreach (var view in this.Model) {
                this.View.Views.Add(view);
            }
        }

    }

}
