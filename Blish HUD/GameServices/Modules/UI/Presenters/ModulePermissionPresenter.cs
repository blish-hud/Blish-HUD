using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModulePermissionPresenter : Presenter<ModulePermissionView, ModuleManager> {

        public ModulePermissionPresenter(ModulePermissionView view, ModuleManager model) : base(view, model) { /* NOOP */ }

        protected override Task<bool> Load(IProgress<string> progress) {
            this.View.PermissionStateChanged += ViewOnPermissionStateChanged;

            this.Model.ModuleEnabled += ModelOnModuleEnabled;
            this.Model.ModuleDisabled += ModelOnModuleDisabled;

            this.View.Editable = !this.Model.Enabled;

            return base.Load(progress);
        }

        private void ViewOnPermissionStateChanged(object sender, KeyedValueChangedEventArgs<TokenPermission, bool> e) {
            // When nothing has been saved yet, all requested permissions are enabled by default.
            var newPermissionList = this.Model.State.UserEnabledPermissions?.ToList()
                                 ?? this.Model.Manifest.ApiPermissions.Keys.ToList();

            if (e.Value) {
                if (!newPermissionList.Contains(e.Key)) {
                    newPermissionList.Add(e.Key);
                }
            } else {
                newPermissionList.Remove(e.Key);
            }

            this.Model.State.UserEnabledPermissions = newPermissionList.ToArray();

            GameService.Settings.Save();

            UpdateStatus();
        }

        private void ModelOnModuleEnabled(object sender, EventArgs e) {
            this.View.Editable = false;
            UpdateStatus();
        }

        private void ModelOnModuleDisabled(object sender, EventArgs e) {
            this.View.Editable = true;
            UpdateStatus();
        }

        protected override void UpdateView() {
            UpdatePermissionList();
            UpdateStatus();
        }

        private void UpdatePermissionList() {
            this.View.SetPermissions(this.Model.Manifest.ApiPermissions.Select(p => (p.Key,
                                                                                     p.Value.Optional,
                                                                                     p.Value.Details ?? "",
                                                                                     this.Model.State.UserEnabledPermissions?.Contains(p.Key) ?? true))
                                         .OrderBy(p => p.Optional));
        }

        private void UpdateStatus() {
            var missingPermissions = this.Model.GetMissingRequiredApiPermissions();

            if (missingPermissions.Length > 0) {
                this.View.SetDetails(
                    $"{Strings.GameServices.ModulesService.ApiPermission_ApprovalRequired} {string.Join(", ", missingPermissions)}",
                    TitledDetailView.DetailLevel.Warning);
            } else if (this.Model.Enabled && this.Model.Manifest.ApiPermissions.Any()) {
                this.View.SetDetails(Strings.GameServices.ModulesService.ApiPermission_NotEditable,
                                     TitledDetailView.DetailLevel.Info);
            } else {
                this.View.ClearDetails();
            }
        }

        protected override void Unload() {
            this.View.PermissionStateChanged -= ViewOnPermissionStateChanged;

            this.Model.ModuleEnabled -= ModelOnModuleEnabled;
            this.Model.ModuleDisabled -= ModelOnModuleDisabled;
        }
    }
}
