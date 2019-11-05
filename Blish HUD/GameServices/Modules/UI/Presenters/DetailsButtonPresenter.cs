using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Presenters;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules.UI.Presenters {
    public class DetailsButtonPresenter : Presenter<ControlView<DetailsButton>, TokenPermission>, IControlPresenter<DetailsButton> {

        /// <inheritdoc />
        public DetailsButtonPresenter(ControlView<DetailsButton> view, TokenPermission model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.Control.Text = this.Model.ToString();
            this.View.Control.Icon = GameService.Content.GetTexture("1914835");
        }

    }
}
