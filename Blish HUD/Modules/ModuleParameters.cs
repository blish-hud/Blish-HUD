using System.Collections.Generic;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using NLog;
using NLog.Config;

namespace Blish_HUD.Modules {

    public class ModuleParameters {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Manifest           _manifest;
        private SettingsManager    _settingsManager;
        private ContentsManager    _contentsManager;
        private DirectoriesManager _directoriesManager;
        private Gw2ApiManager      _gw2ApiManager;

        public Manifest Manifest => _manifest;

        public Managers.SettingsManager SettingsManager => _settingsManager;

        public ContentsManager ContentsManager => _contentsManager;

        public DirectoriesManager DirectoriesManager => _directoriesManager;

        public Gw2ApiManager Gw2ApiManager => _gw2ApiManager;

        internal static ModuleParameters BuildFromManifest(Manifest manifest, ModuleManager module) {
            switch (manifest.ManifestVersion) {
                case SupportedModuleManifestVersion.V1:
                    return BuildFromManifest(manifest as ManifestV1, module);
                    break;

                default:
                    Logger.Warn($"Unsupported manifest version '{manifest.ManifestVersion}'. The module manifest will not be loaded.");
                    break;
            }

            return null;
        }

        private static ModuleParameters BuildFromManifest(ManifestV1 manifest, ModuleManager module) {
            var builtModuleParameters = new ModuleParameters();

            builtModuleParameters._manifest = manifest;

            // TODO: Change manager registers so that they only need an instance of the ExternalModule and not specific params
            builtModuleParameters._settingsManager    = SettingsManager.GetModuleInstance(module);
            builtModuleParameters._contentsManager    = ContentsManager.GetModuleInstance(module);
            builtModuleParameters._directoriesManager = DirectoriesManager.GetModuleInstance(module);
            builtModuleParameters._gw2ApiManager      = GameService.Gw2Api.RegisterGw2ApiConnection(manifest, module.State.UserEnabledPermissions ?? new TokenPermission[0]);

            if (builtModuleParameters._gw2ApiManager == null) {
                // Indicates a conflict of user granted permissions and module required permissions
                return null;
            }

            return builtModuleParameters;
        }

    }

}
