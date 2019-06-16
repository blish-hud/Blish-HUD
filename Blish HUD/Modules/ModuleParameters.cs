using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules {

    public class ModuleParameters {

        private Manifest           _manifest;
        private Managers.SettingsManager    _settingsManager;
        private ContentsManager    _contentsManager;
        private DirectoriesManager _directoriesManager;
        private Gw2ApiManager      _gw2ApiManager;

        public Manifest Manifest => _manifest;

        public Managers.SettingsManager SettingsManager => _settingsManager;

        public ContentsManager ContentsManager => _contentsManager;

        public DirectoriesManager DirectoriesManager => _directoriesManager;

        public Gw2ApiManager GW2ApiManager => _gw2ApiManager;

        private ModuleParameters() {
            
        }

        internal static ModuleParameters BuildFromManifest(Manifest manifest, ModuleManager module) {
            switch (manifest.ManifestVersion) {
                case SupportedModuleManifestVersion.V1:
                    return BuildFromManifest(manifest as ManifestV1, module);
                    break;

                default:
                    GameService.Debug.WriteErrorLine($"Unsupported manifest version '{manifest.ManifestVersion}'.");
                    break;
            }

            return null;
        }

        private static ModuleParameters BuildFromManifest(ManifestV1 manifest, ModuleManager module) {
            var builtModuleParameters = new ModuleParameters();

            builtModuleParameters._manifest = manifest;

            // TODO: Change manager registers so that they only need an instance of the ExternalModule and not specific params
            builtModuleParameters._settingsManager    = Managers.SettingsManager.GetModuleInstance(module);
            builtModuleParameters._contentsManager    = GameService.Content.RegisterContents(module.DataReader);
            builtModuleParameters._directoriesManager = GameService.Directory.RegisterDirectories(manifest.Directories);
            builtModuleParameters._gw2ApiManager      = GameService.Gw2Api.RegisterGw2ApiConnection(manifest, module.State.UserEnabledPermissions ?? new TokenPermission[0]);

            if (builtModuleParameters._gw2ApiManager == null) {
                // Indicates a conflict of user granted permissions and module required permissions
                return null;
            }

            return builtModuleParameters;
        }

    }

}
