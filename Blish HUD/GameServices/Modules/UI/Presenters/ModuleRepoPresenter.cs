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

        private bool _viewBuiltOnce = false;

        public ModuleRepoPresenter(ModuleRepoView view, IPkgRepoProvider model) : base(view, model) { /* NOOP */ }

        private void OnModuleRegistered(object sender, ValueEventArgs<ModuleManager> e) {
            this.View.DirtyAssemblyStateExists = this.View.DirtyAssemblyStateExists || e.Value.IsModuleAssemblyStateDirty;
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            return await this.Model.Load(progress);
        }

        protected override void Unload() {
            GameService.Module.ModuleRegistered -= OnModuleRegistered;
        }

        protected override void UpdateView() {
            UpdateAssemblyDirtiedState();
            UpdateExtraOptionsView();
            UpdatePackagesView();

            _viewBuiltOnce = true;
        }

        private void UpdateAssemblyDirtiedState() {
            if (_viewBuiltOnce) return;

            GameService.Module.ModuleRegistered += OnModuleRegistered;

            foreach (var module in GameService.Module.Modules) {
                this.View.DirtyAssemblyStateExists = this.View.DirtyAssemblyStateExists || module.IsModuleAssemblyStateDirty;
            }
        }

        private void UpdateExtraOptionsView() {
            this.View.SettingsMenu.ClearChildren();

            foreach (var option in this.Model.GetExtraOptions()) {
                var menuItem = this.View.SettingsMenu.AddMenuItem(option.OptionName);
                menuItem.CanCheck = option.IsToggle;
                menuItem.Checked  = option.IsChecked;
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
