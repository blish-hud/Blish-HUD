using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Views;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleRepoPresenter : Presenter<ModuleRepoView, IPkgRepoProvider> {

        public ModuleRepoPresenter(ModuleRepoView view, IPkgRepoProvider model) : base(view, model) { /* NOOP */ }
        
        protected override async Task<bool> Load(IProgress<string> progress) {
            return await this.Model.Load(progress);
        }

        protected override void UpdateView() {
            bool s = true;

            foreach (var pkgManifest in this.Model.GetPkgManifests().GroupBy(m => m.Namespace)) {
                var nPanel = new ViewContainer() {
                    Size     = new Point(this.View.RepoFlowPanel.Width - 25, 64),
                    ShowTint = (s = !s)
                };

                var pkgView = new ManagePkgView();

                nPanel.Parent = this.View.RepoFlowPanel;

                nPanel.Show(pkgView.WithPresenter(new ManagePkgPresenter(pkgView, pkgManifest)));
            }

            foreach (var option in this.Model.GetExtraOptions()) {
                this.View.SettingsMenu.AddMenuItem(option.OptionName).Click += delegate {
                    option.OptionAction();
                };
            }
        }

    }
}
