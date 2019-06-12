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

        private IDataReader _moduleReader;

        private Manifest           _manifest;
        private SettingsManager    _settingsManager;
        private ContentsManager    _contentsManager;
        private DirectoriesManager _directoriesManager;
        private Gw2ApiManager      _gw2ApiManager;

        public Manifest Manifest => _manifest;

        public SettingsManager SettingsManager => _settingsManager;

        public ContentsManager ContentsManager => _contentsManager;

        public DirectoriesManager DirectoriesManager => _directoriesManager;

        public Gw2ApiManager GW2ApiManager => _gw2ApiManager;

        private ModuleParameters(IDataReader moduleReader) {
            _moduleReader = moduleReader;
        }

        internal static ModuleParameters BuildFromManifest(Manifest manifest, ModuleState moduleState, IDataReader moduleReader) {
            switch (manifest.ManifestVersion) {
                case SupportedModuleManifestVersion.V1:
                    return BuildFromManifest(manifest as ManifestV1, moduleState, moduleReader);
                    break;

                default:
                    GameService.Debug.WriteErrorLine($"Unsupported manifest version '{manifest.ManifestVersion}'.");
                    break;
            }

            return null;
        }

        private static ModuleParameters BuildFromManifest(ManifestV1 manifest, ModuleState moduleState, IDataReader moduleReader) {
            var builtModuleParameters = new ModuleParameters(moduleReader);

            builtModuleParameters._manifest = manifest;

            // TODO: Change manager registers so that they only need an instance of the ExternalModule and not specific params
            builtModuleParameters._settingsManager    = GameService.Settings.RegisterSettings($"module:{manifest.Namespace}", true);
            builtModuleParameters._contentsManager    = GameService.Content.RegisterContents(moduleReader);
            builtModuleParameters._directoriesManager = GameService.Directory.RegisterDirectories(manifest.Directories);
            builtModuleParameters._gw2ApiManager      = GameService.Gw2Api.RegisterGw2ApiConnection(manifest, moduleState.UserEnabledPermissions ?? new TokenPermission[0]);

            if (builtModuleParameters._gw2ApiManager == null) {
                // Indicates a conflict of user granted permissions and module required permissions
                return null;
            }

            return builtModuleParameters;
        }

    }

}
