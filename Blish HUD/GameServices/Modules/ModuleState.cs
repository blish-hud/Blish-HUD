using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules {

    public class ModuleState {

        public bool Enabled { get; set; }

        public TokenPermission[] UserEnabledPermissions { get; set; }

        public bool IgnoreDependencies { get; set; }

        public SettingCollection Settings { get; set; }

    }

}
