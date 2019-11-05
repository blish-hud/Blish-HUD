using System;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Common.UI.Presenters {
    public class AchievementPresenter : Presenter<ControlView<DetailsButton>, Achievement>, IControlPresenter<DetailsButton> {

        /// <inheritdoc />
        public AchievementPresenter(ControlView<DetailsButton> view, Achievement model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override async Task<bool> Load(IProgress<string> progress) {
            return await base.Load(progress);
        }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.Control.Text = Model.Name;
            this.View.Control.Icon = GameService.Content.GetRenderServiceTexture(Model.Icon);
        }

    }
}
