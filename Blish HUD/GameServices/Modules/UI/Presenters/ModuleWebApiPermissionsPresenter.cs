using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleWebApiPermissionsPresenter : Presenter<ModuleWebApiPermissionsView, ModuleManager> {

        public ModuleWebApiPermissionsPresenter(ModuleWebApiPermissionsView view, ModuleManager module) : base(view, module) { /* NOOP */ }

        /// <inheritdoc />
        protected override void UpdateView() {
            var savedConsent = this.Model.State.UserEnabledPermissions ?? new TokenPermission[0];

            foreach (var requestedPermission in this.Model.Manifest.ApiPermissions) {
                this.View.SetPermissionCheckboxState(requestedPermission.Key,
                                                     new ModuleWebApiPermissionsView.PermissionState(!requestedPermission.Value.Optional,
                                                                                                     savedConsent.Contains(requestedPermission.Key)));
            }
        }

    }
}
