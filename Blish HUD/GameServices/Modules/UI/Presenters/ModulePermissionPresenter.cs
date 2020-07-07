using System;
using System.Collections.Generic;
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

            return base.Load(progress);
        }

        private void ViewOnPermissionStateChanged(object sender, KeyedValueChangedEventArgs<TokenPermission, bool> e) {
            var newPermissionList = this.Model.State.UserEnabledPermissions?.ToList() ?? new List<TokenPermission>(1);

            if (e.Value) {
                if (!newPermissionList.Contains(e.Key)) {
                    newPermissionList.Add(e.Key);
                }
            } else {
                newPermissionList.Remove(e.Key);
            }

            this.Model.State.UserEnabledPermissions = newPermissionList.ToArray();

            GameService.Settings.Save();
        }

        protected override void UpdateView() {
            UpdatePermissionList();
            UpdateStatus();
        }

        private void UpdatePermissionList() {
            this.View.SetPermissions(this.Model.Manifest.ApiPermissions.Select(p => (p.Key,
                                                                                     p.Value.Optional,
                                                                                     p.Value.Details ?? "",
                                                                                     this.Model.State.UserEnabledPermissions?.Contains(p.Key) ?? false))
                                         .OrderBy(p => p.Optional));
        }

        private void UpdateStatus() {
            // Unused at this time
        }

    }
}
