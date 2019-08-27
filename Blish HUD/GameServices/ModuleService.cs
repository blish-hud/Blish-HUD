using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using File = System.IO.File;
using Module = Blish_HUD.Modules.Module;

namespace Blish_HUD {

    public class ModuleService : GameService {

        private static readonly Logger Logger = Logger.GetLogger(typeof(ModuleService));

        private const string MODULE_SETTINGS = "ModuleConfiguration";

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";
        private const string EXPORTED_VERSION_SETTING = "ExportedOn"; 

        private const string MODULES_DIRECTORY = "modules";

        private const string MODULE_EXTENSION = ".bhm";

        private SettingCollection _moduleSettings;

        private string ModulesDirectory => DirectoryUtil.RegisterDirectory(MODULES_DIRECTORY);

        private SettingEntry<List<string>> _exportedOnVersions;
        private SettingEntry<Dictionary<string, ModuleState>> _moduleStates;

        public SettingEntry<Dictionary<string, ModuleState>> ModuleStates => _moduleStates;

        private readonly List<ModuleManager> _modules;
        public IReadOnlyList<ModuleManager> Modules => _modules.ToList();

        public ModuleService() {
            _modules = new List<ModuleManager>();
        }

        protected override void Initialize() {
            _moduleSettings = Settings.RegisterRootSettingCollection(MODULE_SETTINGS);

            DefineSettings(_moduleSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            _moduleStates       = settings.DefineSetting(MODULESTATES_CORE_SETTING, new Dictionary<string, ModuleState>());
            _exportedOnVersions = settings.DefineSetting(EXPORTED_VERSION_SETTING,  new List<string>());
        }

        public ModuleManager RegisterModule(IDataReader moduleReader) {
            string manifestContents;
            using (var manifestReader = new StreamReader(moduleReader.GetFileStream("manifest.json"))) {
                manifestContents = manifestReader.ReadToEnd();
            }
            var moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);
            bool enableModule = false;

            if (_moduleStates.Value.ContainsKey(moduleManifest.Namespace)) {
                enableModule = _moduleStates.Value[moduleManifest.Namespace].Enabled;
            } else {
                _moduleStates.Value.Add(moduleManifest.Namespace, new ModuleState());
            }

            var moduleManager  = new ModuleManager(moduleManifest,
                                                   _moduleStates.Value[moduleManifest.Namespace],
                                                   moduleReader);

            moduleManager.Enabled = enableModule;

            _modules.Add(moduleManager);

            return moduleManager;
        }

        private void ExtractPackagedModule(Stream fileData, IDataReader reader) {
            string moduleName = string.Empty;

            using (var moduleArchive = new ZipArchive(fileData, ZipArchiveMode.Read)) {
                using (var manifestStream = moduleArchive.GetEntry("manifest.json")?.Open()) {
                    if (manifestStream == null) return;

                    string manifestContents;
                    using (var manifestReader = new StreamReader(manifestStream)) {
                        manifestContents = manifestReader.ReadToEnd();
                    }

                    var moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);

                    Logger.Info("Exporting internally packaged module {moduleName} ({$moduleNamespace}) v{$moduleVersion}", moduleManifest.Name, moduleManifest.Namespace, moduleManifest.Version);

                    moduleName = moduleManifest.Name;
                }
            }

            if (!string.IsNullOrEmpty(moduleName)) {
                File.WriteAllBytes(Path.Combine(this.ModulesDirectory, $"{moduleName}.bhm"), ((MemoryStream)fileData).GetBuffer());
            }
        }

        private void UnpackInternalModules() {
            var internalModulesReader = new ZipArchiveReader("ref.dat");

            internalModulesReader.LoadOnFileType(ExtractPackagedModule, ".bhm");
        }

        protected override void Load() {
            /*
            RegisterModule(new Modules.DebugText());
            RegisterModule(new Modules.DiscordRichPresence());
            RegisterModule(new Modules.BeetleRacing.BeetleRacing());
            RegisterModule(new Modules.EventTimers.EventTimers());
            RegisterModule(new Modules.Compass());
            RegisterModule(new Modules.PoiLookup.PoiLookup());
            RegisterModule(new Modules.MarkersAndPaths.MarkersAndPaths());
            RegisterModule(new Modules.Musician.Musician());
            */
            // RegisterModule(new Modules.LoadingScreenHints.LoadingScreenHints());
            // RegisterModule(new Modules.RangeCircles());
            // RegisterModule(new Modules.MouseUsability.MouseUsability());

#if DEBUG && !NODIRMODULES
            // Allows devs to symlink the output directories of modules in development straight to the modules folder
            foreach (string manifestPath in Directory.GetFiles(this.ModulesDirectory, "manifest.json", SearchOption.AllDirectories)) {
                string moduleDir = Directory.GetParent(manifestPath).FullName;

                var moduleReader = new DirectoryReader(moduleDir);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }
#endif

            // Get the base version string and see if we've exported the modules for this version yet
            string baseVersionString = Program.OverlayVersion.BaseVersion().ToString();
            if (!_exportedOnVersions.Value.Contains(baseVersionString)) {
                UnpackInternalModules();
                _exportedOnVersions.Value.Add(baseVersionString);
            }

            foreach (string moduleArchivePath in Directory.GetFiles(this.ModulesDirectory, $"*{MODULE_EXTENSION}", SearchOption.AllDirectories)) {
                var moduleReader = new ZipArchiveReader(moduleArchivePath);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }
        }

        protected override void Unload() {
            _modules.ForEach(s => {
                try {
                    // TODO: Unload module
                } catch (Exception ex) {
                    #if DEBUG
                    // To assist in debugging
                    throw;
                    #endif
                    Logger.Error(ex, "Module '{$moduleName} ({$moduleNamespace}) threw an exception while being unloaded.", s.Manifest.Name, s.Manifest.Namespace);
                }
            });
        }

        protected override void Update(GameTime gameTime) {
            _modules.ForEach(s => {
                                 try {
                                     if (s.Enabled) s.ModuleInstance.DoUpdate(gameTime);
                                 } catch (Exception ex) {
                                     #if DEBUG
                                     // To assist in debugging
                                     throw;
                                     #endif
                                     Logger.Error(ex, "Module '{$moduleName} ({$moduleNamespace}) threw an exception.", s.Manifest.Name, s.Manifest.Namespace);
                                 }
            });
        }
    }
}
