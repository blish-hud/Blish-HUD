using System;
using System.Threading.Tasks;
using Blish_HUD.Modules.Managers;

namespace Blish_HUD.Modules {

    public class ModuleParameters : IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<ModuleParameters>();

        public Manifest Manifest { get; private set; }

        public SettingsManager SettingsManager { get; private set; }

        public ContentsManager ContentsManager { get; private set; }

        public DirectoriesManager DirectoriesManager { get; private set; }

        public Gw2ApiManager Gw2ApiManager { get; private set; }

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
                Manifest = manifest,

                SettingsManager = SettingsManager.GetModuleInstance(module),
                ContentsManager = ContentsManager.GetModuleInstance(module),
                DirectoriesManager = DirectoriesManager.GetModuleInstance(module),
                Gw2ApiManager = Gw2ApiManager.GetModuleInstance(module)
            };

            if (builtModuleParameters.Gw2ApiManager == null) {
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

        internal async Task LoadAsync() => await this.Gw2ApiManager.RenewSubtoken();

        public void Dispose() => this.ContentsManager?.Dispose();

    }
}
