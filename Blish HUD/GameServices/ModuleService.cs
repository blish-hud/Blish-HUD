using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD {

    public class ModuleService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<ModuleService>();

        private const string MODULE_SETTINGS = "ModuleConfiguration";

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";
        private const string EXPORTED_VERSION_SETTING  = "ExportedOn";

        private const string MODULES_DIRECTORY = "modules";

        private const string MODULE_EXTENSION    = ".bhm";
        private const string MODULE_MANIFESTNAME = "manifest.json";

        public event EventHandler<ValueEventArgs<ModuleManager>> ModuleRegistered;
        public event EventHandler<ValueEventArgs<ModuleManager>> ModuleUnregistered;

        private ISettingCollection _moduleSettings;

        internal string ModulesDirectory => DirectoryUtil.RegisterDirectory(MODULES_DIRECTORY);

        private ISettingEntry<List<string>>                    _exportedOnVersions;
        private ISettingEntry<Dictionary<string, ModuleState>> _moduleStates;

        public ISettingEntry<Dictionary<string, ModuleState>> ModuleStates => _moduleStates;

        private readonly List<ModuleManager>          _modules = new List<ModuleManager>();
        public           IReadOnlyList<ModuleManager> Modules => _modules.ToList();
        
        protected override void Initialize() {
            _moduleSettings = Settings.RegisterRootSettingCollection(MODULE_SETTINGS);

            DefineSettings(_moduleSettings);
        }

        private void DefineSettings(ISettingCollection settings) {
            _moduleStates       = settings.DefineSetting(MODULESTATES_CORE_SETTING, new Dictionary<string, ModuleState>());
            _exportedOnVersions = settings.DefineSetting(EXPORTED_VERSION_SETTING,  new List<string>());
        }

        public ModuleManager RegisterModule(IDataReader moduleReader) {
            if (!moduleReader.FileExists(MODULE_MANIFESTNAME)) {
                Logger.Warn("Attempted to load an invalid module {modulePath}: {manifestName} is missing.", moduleReader.GetPathRepresentation(), MODULE_MANIFESTNAME);
                return null;
            }

            string manifestContents;
            using (var manifestReader = new StreamReader(moduleReader.GetFileStream(MODULE_MANIFESTNAME))) {
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

            _modules.Add(moduleManager);

            this.ModuleRegistered?.Invoke(this, new ValueEventArgs<ModuleManager>(moduleManager));

            if (enableModule) {
                moduleManager.TryEnable();
            }

            RegisterModuleMenuInSettings(moduleManager);

            return moduleManager;
        }

        private void ExtractPackagedModule(Stream fileData, IDataReader reader) {
            string moduleName;

            using (var moduleArchive = new ZipArchive(fileData, ZipArchiveMode.Read)) {
                using (var manifestStream = moduleArchive.GetEntry(MODULE_MANIFESTNAME)?.Open()) {
                    if (manifestStream == null) return;

                    string manifestContents;
                    using (var manifestReader = new StreamReader(manifestStream)) {
                        manifestContents = manifestReader.ReadToEnd();
                    }

                    var moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);

                    Logger.Info("Exporting internally packaged module {module}", moduleManifest.GetDetailedName());

                    moduleName = moduleManifest.Name;
                }
            }

            if (moduleName != null) {
                File.WriteAllBytes(Path.Combine(this.ModulesDirectory, $"{moduleName}.bhm"), ((MemoryStream)fileData).GetBuffer());
            }
        }

        private void UnpackInternalModules() {
            var internalModulesReader = new ZipArchiveReader("ref.dat");

            internalModulesReader.LoadOnFileType(ExtractPackagedModule, MODULE_EXTENSION);
        }
        
        /// <summary>
        /// Registers a packed (.bhm) module with the <see cref="ModuleService"/>.
        /// </summary>
        public ModuleManager RegisterPackedModule(string modulePath) {
            if (modulePath == null)
                throw new ArgumentNullException(nameof(modulePath));

            if (!File.Exists(modulePath)) {
                Logger.Warn("Attempted to load a module {modulePath} which does not exist.", modulePath);
                return null;
            }

            return RegisterModule(new ZipArchiveReader(modulePath));
        }

        /// <summary>
        /// Unregisters the module.
        /// </summary>
        public void UnregisterModule(ModuleManager moduleManager) {
            if (moduleManager == null) throw new ArgumentNullException(nameof(moduleManager));

            if (!_modules.Contains(moduleManager)) return;

            moduleManager.Disable();

            _modules.Remove(moduleManager);

            this.ModuleUnregistered?.Invoke(this, new ValueEventArgs<ModuleManager>(moduleManager));

            UnregisterModuleMenuInSettings(moduleManager);
        }

        /// <summary>
        /// Registers an unpacked module from a folder with the <see cref="ModuleService"/>.
        /// </summary>
        private ModuleManager RegisterUnpackedModule(string moduleDir) {
            if (moduleDir == null)
                throw new ArgumentNullException(nameof(moduleDir));

            if (!Directory.Exists(moduleDir)) {
                Logger.Warn("Attempted to load a module {moduleDir} which does not exist.", moduleDir);
                return null;
            }

            return RegisterModule(new DirectoryReader(moduleDir));
        }

        protected override void Load() {
            RegisterModulesInSettings();

            if (ApplicationSettings.Instance.DebugEnabled) {
                // Allows devs to symlink the output directories of modules in development straight to the modules folder
                foreach (string manifestPath in Directory.GetFiles(this.ModulesDirectory, MODULE_MANIFESTNAME, SearchOption.AllDirectories)) {
                    string moduleDir = Directory.GetParent(manifestPath).FullName;

                    RegisterUnpackedModule(moduleDir);
                }
            }

            if (ApplicationSettings.Instance.DebugModulePath != null) {
                ModuleManager debugModule = null;

                if (File.Exists(ApplicationSettings.Instance.DebugModulePath)) {
                    debugModule = RegisterPackedModule(ApplicationSettings.Instance.DebugModulePath);
                } else if (Directory.Exists(ApplicationSettings.Instance.DebugModulePath)) {
                    debugModule = RegisterUnpackedModule(ApplicationSettings.Instance.DebugModulePath);
                } else {
                    Logger.Warn("Failed to load module from path {modulePath}.", ApplicationSettings.Instance.DebugModulePath);
                }

                debugModule?.TryEnable();
            }

            // Get the base version string and see if we've exported the modules for this version yet
            string baseVersionString = Program.OverlayVersion.BaseVersion().ToString();
            if (!_exportedOnVersions.Value.Contains(baseVersionString)) {
                UnpackInternalModules();
                _exportedOnVersions.Value.Add(baseVersionString);
            }

            foreach (string moduleArchivePath in Directory.GetFiles(this.ModulesDirectory, $"*{MODULE_EXTENSION}", SearchOption.AllDirectories)) {
                RegisterPackedModule(moduleArchivePath);
            }

            RegisterRepoManagementInSettings();
        }

        private          MenuItem                            _rootModuleSettingsMenuItem;
        private readonly Dictionary<MenuItem, ModuleManager> _moduleMenus = new Dictionary<MenuItem, ModuleManager>();

        private void RegisterModuleMenuInSettings(ModuleManager moduleManager) {
            var moduleMi = new MenuItem(moduleManager.Manifest.Name) {
                BasicTooltipText = moduleManager.Manifest.Description,
                Parent           = _rootModuleSettingsMenuItem
            };

            _moduleMenus.Add(moduleMi, moduleManager);
        }

        private void UnregisterModuleMenuInSettings(ModuleManager moduleManager) {
            foreach (KeyValuePair<MenuItem, ModuleManager> moduleMenuPair in _moduleMenus) {
                if (moduleMenuPair.Value == moduleManager) {
                    _moduleMenus.Remove(moduleMenuPair.Key);
                    moduleMenuPair.Key.Parent = null;
                    break;
                }
            }
        }

        private void RegisterModulesInSettings() {
            _rootModuleSettingsMenuItem = new MenuItem(Strings.GameServices.ModulesService.ManageModulesSection, Content.GetTexture("156764-noarrow"));
            
            Overlay.SettingsTab.RegisterSettingMenu(_rootModuleSettingsMenuItem, HandleModuleSettingMenu, int.MaxValue - 10);
        }

        private void RegisterRepoManagementInSettings() {
            Overlay.SettingsTab.RegisterSettingMenu(new MenuItem(Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepoSection, Content.GetTexture("156764-noarrow")), m => new ModuleRepoView(new PublicPkgRepoProvider()), int.MaxValue - 11);
        }

        private View HandleModuleSettingMenu(MenuItem menuItem) {
            if (!this.Modules.Any()) {
                return new NoModulesView();
            }

            return _moduleMenus.ContainsKey(menuItem)
                       ? new ManageModuleView(_moduleMenus[menuItem])
                       : null;
        }

        protected override void Update(GameTime gameTime) {
            foreach (var module in _modules) {
                if (module.Enabled) {
                    try {
                        module.ModuleInstance.DoUpdate(gameTime);
                    } catch (Exception ex) {
                        Logger.Error(ex, "Module {module} threw an exception while updating.", module.Manifest.GetDetailedName());

                        if (ApplicationSettings.Instance.DebugEnabled) {
                            // To assist in debugging modules
                            throw;
                        }
                    }
                }
            }
        }

        protected override void Unload() {
            foreach (var module in _modules) {
                if (module.Enabled) {
                    try {
                        Logger.Info("Unloading module {module}.", module.Manifest.GetDetailedName());
                        module.ModuleInstance.Dispose();
                    } catch (Exception ex) {
                        Logger.Error(ex, "Module '{module} threw an exception while unloading.", module.Manifest.GetDetailedName());

                        if (ApplicationSettings.Instance.DebugEnabled) {
                            // To assist in debugging modules
                            throw;
                        }
                    }
                }
            }
        }
    }
}
