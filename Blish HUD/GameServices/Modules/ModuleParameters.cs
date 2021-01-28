using System;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules {

    public class ModuleParameters : IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<ModuleParameters>();

        private Manifest           _manifest;
        private SettingsManager    _settingsManager;
        private ContentsManager    _contentsManager;
        private DirectoriesManager _directoriesManager;
        private Gw2ApiManager      _gw2ApiManager;

        public Manifest Manifest => _manifest;

        public SettingsManager SettingsManager => _settingsManager;

        public ContentsManager ContentsManager => _contentsManager;

        public DirectoriesManager DirectoriesManager => _directoriesManager;

        public Gw2ApiManager Gw2ApiManager => _gw2ApiManager;

        internal static ModuleParameters BuildFromManifest(Manifest manifest, ModuleManager module) {
            switch (manifest.ManifestVersion) {
                case SupportedModuleManifestVersion.V1:
                    return BuildFromManifest(manifest as ManifestV1, module);

                default:
                    Logger.Warn("Module {module} is using an unsupported manifest version {manifestVersion}. The module manifest will not be loaded.", module, manifest.ManifestVersion);
                    break;
            }

            return null;
        }

        private static ModuleParameters BuildFromManifest(ManifestV1 manifest, ModuleManager module) {
            var builtModuleParameters = new ModuleParameters {
                _manifest = manifest,

                // TODO: Change manager registers so that they only need an instance of the ExternalModule and not specific params
                _settingsManager    = SettingsManager.GetModuleInstance(module),
                _contentsManager    = ContentsManager.GetModuleInstance(module),
                _directoriesManager = DirectoriesManager.GetModuleInstance(module),
                _gw2ApiManager      = Gw2ApiManager.GetModuleInstance(module)
            };

            if (builtModuleParameters._gw2ApiManager == null) {
                /* Indicates a conflict of user granted permissions and module required permissions
                 * How this could happen (without manually modifying settings):
                 *  1. User approves all required permissions for a module.
                 *  2. The user enables the module.
                 *  3. The user updates the module to a version that has a new required permission which haven't been explicitly approved.
                 */
                // TODO: Show a popup instead that just asks the user if the new permission(s) is/are okay
                Logger.Warn("An attempt was made to enable the module {module} before all of the required API permissions have been approved.", module.ToString());
                return null;
            }

            return builtModuleParameters;
        }

        public void Dispose() {
            _contentsManager?.Dispose();
        }

    }

}
