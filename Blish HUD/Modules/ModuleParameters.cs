using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;

namespace Blish_HUD.Modules {

    public class ModuleParameters {

        private IDataReader _moduleReader;

        private Manifest           _manifest;
        private SettingsManager    _settingsManager;
        private ContentsManager    _contentsManager;
        private DirectoriesManager _directoriesManager;

        public Manifest Manifest => _manifest;

        public SettingsManager SettingsManager => _settingsManager;

        public ContentsManager ContentsManager => _contentsManager;

        public DirectoriesManager DirectoriesManager => _directoriesManager;

        private ModuleParameters(IDataReader moduleReader) {
            _moduleReader = moduleReader;
        }

        internal static ModuleParameters BuildFromManifest(Manifest manifest, IDataReader moduleReader) {
            switch (manifest.ManifestVersion) {
                case SupportedModuleManifestVersion.V1:
                    return BuildFromManifest(manifest as ManifestV1, moduleReader);
                    break;

                default:
                    GameService.Debug.WriteErrorLine($"Unsupported manifest version '{manifest.ManifestVersion}'.");
                    break;
            }

            return null;
        }

        internal static ModuleParameters BuildFromManifest(ManifestV1 manifest, IDataReader moduleReader) {
            var builtModuleParameters = new ModuleParameters(moduleReader);

            builtModuleParameters._manifest = manifest;
            builtModuleParameters._settingsManager    = GameService.Settings.RegisterSettings($"module:{manifest.Namespace}", true);
            builtModuleParameters._contentsManager    = GameService.Content.RegisterContents(moduleReader);
            builtModuleParameters._directoriesManager = GameService.Directory.RegisterDirectories(manifest.Directories);

            return builtModuleParameters;
        }

    }

}
