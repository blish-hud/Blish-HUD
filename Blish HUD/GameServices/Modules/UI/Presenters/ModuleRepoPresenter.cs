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
            UpdateExtraOptionsView();
            UpdatePackagesView();
        }

        private void UpdateExtraOptionsView() {
            foreach (var option in this.Model.GetExtraOptions()) {
                var menuItem = this.View.SettingsMenu.AddMenuItem(option.OptionName);
                menuItem.CanCheck = option.IsToggle;
                menuItem.Click += delegate {
                    option.OptionAction(menuItem.Checked);

                    UpdatePackagesView();
                };
            }
        }

        private void UpdatePackagesView() {
            this.View.RepoFlowPanel.ClearChildren();

            bool s = true;

            foreach (var pkgManifest in this.Model.GetPkgManifests().GroupBy(m => m.Namespace)) {
                var nPanel = new ViewContainer {
                    Size     = new Point(this.View.RepoFlowPanel.Width - 25, 64),
                    ShowTint = (s = !s),
                    Parent   = this.View.RepoFlowPanel
                };

                nPanel.Show(new ManagePkgView(pkgManifest));
            }
        }

    }
}
