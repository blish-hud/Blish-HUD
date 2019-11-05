using System;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Presenters;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Common.UI.Views {
    public class CenteredView : View<CenterPresenter> {

        private readonly Panel _centeredPanel;

        /// <inheritdoc />
        public CenteredView(IView view) {
            _centeredPanel         =  new Panel();
            _centeredPanel.Resized += CenteredPanelOnResized;
            _centeredPanel.Moved   += CenteredPanelOnMoved;

            this.Presenter = new CenterPresenter(this, view);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            buildPanel.Resized += BuildPanelOnResized;

            buildPanel.Size = buildPanel.ContentRegion.Size;

            this.Presenter.Model.DoBuild(_centeredPanel);

            _centeredPanel.Parent = buildPanel;

            //UpdateViewLocation();
        }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            return this.Presenter.Model.DoLoad(progress);
        }

        /// <inheritdoc />
        protected override void Unload() {
            this.Presenter.Model.DoUnload();
        }

        private void UpdateViewLocation() {
            _centeredPanel.Location = new Point(this.ViewTarget.Width  / 2 - _centeredPanel.Width  / 2,
                                                this.ViewTarget.Height / 2 - _centeredPanel.Height / 2);
        }

        private void BuildPanelOnResized(object sender, ResizedEventArgs e) {
            UpdateViewLocation();
        }

        private void CenteredPanelOnMoved(object sender, MovedEventArgs e) {
            UpdateViewLocation();
        }

        private void CenteredPanelOnResized(object sender, ResizedEventArgs e) {
            UpdateViewLocation();
        }

    }
}
