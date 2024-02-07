﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.UI.Controls;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Flurl.Http;

namespace Blish_HUD {

    public class ModuleService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<ModuleService>();

        private const string MODULE_SETTINGS = "ModuleConfiguration";

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";
        private const string EXPORTED_VERSION_SETTING  = "ExportedOn";

        private const string MODULES_DIRECTORY = "modules";

        private const string MODULE_EXTENSION    = ".bhm";
        private const string MODULE_MANIFESTNAME = "manifest.json";

        private const string MODULE_COMPATIBILITYLIST = "compatibility.json";
        private const string MODULE_SPOILEDURI        = "https://pkgs.blishhud.com/spoiled.json";

        public event EventHandler<ValueEventArgs<ModuleManager>> ModuleRegistered;
        public event EventHandler<ValueEventArgs<ModuleManager>> ModuleUnregistered;

        private List<ModuleDependency> _incompatibleModules      = new List<ModuleDependency>(0);
        private HashSet<string>        _spoiledModuleIdentifiers = new HashSet<string>(0);

        /// <summary>
        /// Access to repo management and state.
        /// </summary>
        public ModulePkgRepoHandler ModulePkgRepoHandler { get; private set; }

        private SettingCollection _moduleSettings;

        internal string ModulesDirectory => DirectoryUtil.RegisterDirectory(MODULES_DIRECTORY);

        private SettingEntry<List<string>>                    _exportedOnVersions;
        private SettingEntry<Dictionary<string, ModuleState>> _moduleStates;

        public SettingEntry<Dictionary<string, ModuleState>> ModuleStates => _moduleStates;

        private readonly List<ModuleManager>          _modules = new List<ModuleManager>();
        public           IReadOnlyList<ModuleManager> Modules => _modules.ToList();

        internal ModuleService() {
            SetServiceModules(this.ModulePkgRepoHandler = new ModulePkgRepoHandler(this));
        }

        protected override void Initialize() {
            _moduleSettings = Settings.RegisterRootSettingCollection(MODULE_SETTINGS);

            DefineSettings(_moduleSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            _moduleStates       = settings.DefineSetting(MODULESTATES_CORE_SETTING, new Dictionary<string, ModuleState>());
            _exportedOnVersions = settings.DefineSetting(EXPORTED_VERSION_SETTING,  new List<string>());
        }

        internal bool ModuleIsExplicitlyIncompatible(ModuleManager moduleManager) {
            return _spoiledModuleIdentifiers.Contains($"{moduleManager.Manifest.Namespace}_{moduleManager.Manifest.Version}")
                || _incompatibleModules.Any(compatibilityListing => string.Equals(moduleManager.Manifest.Namespace, compatibilityListing.Namespace, StringComparison.OrdinalIgnoreCase)
                                                                 && compatibilityListing.VersionRange.IsSatisfied(moduleManager.Manifest.Version.BaseVersion()));
        }

        public ModuleManager RegisterModule(IDataReader moduleReader) {
            if (moduleReader == null) {
                Logger.Warn("Failed to register a module as its archive could not be loaded.");
                return null;
            }

            if (!moduleReader.FileExists(MODULE_MANIFESTNAME)) {
                Logger.Warn("Attempted to load an invalid module {modulePath}: {manifestName} is missing.", moduleReader.GetPathRepresentation(), MODULE_MANIFESTNAME);
                return null;
            }

            string manifestContents;
            using (var manifestReader = new StreamReader(moduleReader.GetFileStream(MODULE_MANIFESTNAME))) {
                manifestContents = manifestReader.ReadToEnd();
            }

            Manifest moduleManifest = null;

            try {
                moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to read module manifest.  It appears to be malformed.  The module at path {modulePath} will not be loaded.", moduleReader.GetPathRepresentation());
                return null;
            }

            // Avoid loading the same module multiple times (ensure we load the highest version).
            var existingModule = _modules.FirstOrDefault(module => string.Equals(moduleManifest.Namespace, module.Manifest.Namespace, StringComparison.OrdinalIgnoreCase));
            if (existingModule != null) {
                Logger.Warn("A module with the namespace {moduleNamespace} has has already been loaded.  The module at path {modulePath} is a duplicate of this module.  Please remove any duplicate module(s).",
                            moduleManifest.Namespace,
                            moduleReader.GetPathRepresentation());

                if (existingModule.Manifest.Version > moduleManifest.Version) {
                    // We're loading a duplicate - exit early
                    return null;
                } else {
                    // This version is newer than the existing one, so replace it
                    UnregisterModule(existingModule);
                }
            }

            if (!_moduleStates.Value.ContainsKey(moduleManifest.Namespace)) {
                _moduleStates.Value.Add(moduleManifest.Namespace, new ModuleState());
            }

            var moduleManager = new ModuleManager(moduleManifest,
                                                  _moduleStates.Value[moduleManifest.Namespace],
                                                  moduleReader);

            if (ModuleIsExplicitlyIncompatible(moduleManager)) {
                Logger.Warn("The module {module} is not compatible with this version of Blish HUD so it will not allow you to enable it.  Please remove the module or update to a compatible version if one is available.",
                            moduleManifest.GetDetailedName(),
                            moduleReader.GetPathRepresentation());
            }

            _modules.Add(moduleManager);

            this.ModuleRegistered?.Invoke(this, new ValueEventArgs<ModuleManager>(moduleManager));

            if (moduleManifest.EnabledWithoutGW2 && _moduleStates.Value[moduleManifest.Namespace].Enabled) {
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

        private void LoadSpoiledList() {
            try {
                // We block for this on purpose
                _spoiledModuleIdentifiers = MODULE_SPOILEDURI.GetJsonAsync<string[]>().GetAwaiter().GetResult().ToHashSet();
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to load the spoiled modules list!");
            }
        }

        private void LoadCompatibility(IDataReader datReader) {
            if (datReader.FileExists(MODULE_COMPATIBILITYLIST)) {
                try {
                    var    compatibilityStream = datReader.GetFileStream(MODULE_COMPATIBILITYLIST).ReplaceWithMemoryStream();
                    string compatibilityRaw    = Encoding.UTF8.GetString(compatibilityStream.GetBuffer(), 0, (int)compatibilityStream.Length);
                    _incompatibleModules = JsonConvert.DeserializeObject<List<ModuleDependency>>(compatibilityRaw, new ModuleDependency.VersionDependenciesConverter());
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to load {compatibilityFile} from the ref.dat.", MODULE_COMPATIBILITYLIST);
                }
            }
        }

        private void HandleFirstVersionLaunch(IDataReader datReader) {
            string baseVersionString = Program.OverlayVersion.BaseVersion().ToString();
            if (!_exportedOnVersions.Value.Contains(baseVersionString) || ApplicationSettings.Instance.DebugEnabled) {
                datReader.LoadOnFileType(ExtractPackagedModule, MODULE_EXTENSION);

                if (!_exportedOnVersions.Value.Contains(baseVersionString)) {
                    _exportedOnVersions.Value.Add(baseVersionString);
                }
            }
        }

        private void HandleRefLoading() {
            var datReader = new ZipArchiveReader(ApplicationSettings.Instance.RefPath);

            HandleFirstVersionLaunch(datReader);
            LoadCompatibility(datReader);
            LoadSpoiledList();
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

            ZipArchiveReader moduleArchive = null;

            try {
                moduleArchive = new ZipArchiveReader(modulePath);
            } catch (InvalidDataException e) {
                Logger.Warn(e, "Attempted to load a module {modulePath} which appears to be corrupt.  Deleting it so that it can be redownloaded.", modulePath);

                try {
                    File.Delete(modulePath); // Delete it to avoid problems and help ensure the user downloads a new copy.
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to delete module {modulePath}.", modulePath);
                }

                return null;
            } catch (Exception e) {
                Logger.Error(e, "Attempted to load a module {modulePath} but the archive could not be read.", modulePath);
            }

            return RegisterModule(moduleArchive);
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

                if (debugModule != null) {
                    debugModule.State.Enabled = true;
                    _modules.Add(debugModule); // Don't start directly and let it load normally with dependency order
                }
            }

            HandleRefLoading();

            foreach (string moduleArchivePath in Directory.GetFiles(this.ModulesDirectory, $"*{MODULE_EXTENSION}", SearchOption.AllDirectories)) {
                RegisterPackedModule(moduleArchivePath);
            }

            if (GameService.GameIntegration.Gw2Instance.Gw2IsRunning) {
                Gw2Instance_Gw2Started(null, null);
            } else {
                GameService.GameIntegration.Gw2Instance.Gw2Started += Gw2Instance_Gw2Started;
            }
        }

        private void Gw2Instance_Gw2Started(object sender, EventArgs e) {
            var resolvedDependencyOrder = SortByDependencies(_modules).ToList();

            Logger.Debug($"Resolved load dependency order: {string.Join(", ", resolvedDependencyOrder.Select(x => x.Manifest.Namespace).ToArray())}");

            foreach (var module in resolvedDependencyOrder) {
                if (module.State.Enabled) {
                    module.TryEnable();
                }
            }
        }

        private static IEnumerable<ModuleManager> SortByDependencies(IEnumerable<ModuleManager> modules) {
            var sorted = new List<ModuleManager>();
            var visited = new HashSet<ModuleManager>();

            foreach (var module in modules) {
                VisitDependency(module, visited, sorted, m => {
                    var dependencyModules = modules.Where(m => {
                        // Get all modules which have a dependency on the current module
                        return m.Manifest?.Dependencies?.Any(md => !md.IsBlishHud && md.Namespace == m.Manifest.Namespace) ?? false;
                    });

                    return dependencyModules;
                });
            }

            return sorted;
        }

        private static void VisitDependency(ModuleManager item, HashSet<ModuleManager> visited, List<ModuleManager> sorted, Func<ModuleManager, IEnumerable<ModuleManager>> dependencies) {
            if (!visited.Contains(item)) {
                visited.Add(item);

                foreach (var dep in dependencies(item)) {
                    VisitDependency(dep, visited, sorted, dependencies);
                }

                sorted.Add(item);
            } else if (!sorted.Contains(item)) {
                throw new Exception($"Cyclic dependency found: {item.Manifest.GetDetailedName()}");
            }
        }

        private MenuItem _rootModuleSettingsMenuItem;
        private readonly Dictionary<MenuItem, ModuleManager> _moduleMenus = new Dictionary<MenuItem, ModuleManager>();
        
        private void RegisterModuleMenuInSettings(ModuleManager moduleManager) {
            var moduleMi = new ModuleMenuItem(moduleManager) {
                BasicTooltipText = moduleManager.Manifest.Description,
                Parent = _rootModuleSettingsMenuItem
            };

            _moduleMenus.Add(moduleMi, moduleManager);
        }

        private void UnregisterModuleMenuInSettings(ModuleManager moduleManager) {
            foreach (KeyValuePair<MenuItem, ModuleManager> moduleMenuPair in _moduleMenus) {
                if (moduleMenuPair.Value == moduleManager) {
                    _moduleMenus.Remove(moduleMenuPair.Key);

                    MenuItem toSelect = moduleMenuPair.Key.Selected
                        ? _moduleMenus.FirstOrDefault().Key ?? _rootModuleSettingsMenuItem
                        : null;

                    moduleMenuPair.Key.Parent = null;

                    toSelect?.Select();

                    break;
                }
            }
        }

        private void RegisterModulesInSettings() {
            _rootModuleSettingsMenuItem = new MenuItem(Strings.GameServices.ModulesService.ManageModulesSection, Content.GetTexture("156764-noarrow"));
            
            Overlay.SettingsTab.RegisterSettingMenu(_rootModuleSettingsMenuItem, HandleModuleSettingMenu, int.MaxValue - 10);
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
                // Only update enabled modules if we are in game or if the module specifies it can run without Guild Wars 2
                if (module.Enabled && (GameIntegration.Gw2Instance.Gw2IsRunning || module.Manifest.EnabledWithoutGW2)) {
                    GameService.Debug.StartTimeFunc(module.Manifest.Name);
                    try {
                        module.ModuleInstance.DoUpdate(gameTime);
                    } catch (Exception ex) {
                        Logger.GetLogger(module.GetType()).Error(ex, "Module {module} threw an exception while updating.", module.Manifest.GetDetailedName());

                        if (ApplicationSettings.Instance.DebugEnabled) {
                            // To assist in debugging modules
                            throw;
                        }
                    }
                    GameService.Debug.StopTimeFunc(module.Manifest.Name);
                }
            }
        }

        protected override void Unload() {
            GameService.GameIntegration.Gw2Instance.Gw2Started -= Gw2Instance_Gw2Started;

            // We need to unload modules in reverse order as modules could need to execute context methods on unload.
            var resolvedDependencyOrder = SortByDependencies(_modules).Reverse().ToList();

            Logger.Debug($"Resolved unload dependency order: {string.Join(", ", resolvedDependencyOrder.Select(x => x.Manifest.Namespace).ToArray())}");

            foreach (var module in resolvedDependencyOrder) {
                if (module.Enabled) {
                    try {
                        Logger.Info("Unloading module {module}.", module.Manifest.GetDetailedName());
                        module.ModuleInstance.Dispose();
                    } catch (Exception ex) {
                        Logger.GetLogger(module.GetType()).Error(ex, "Module '{module} threw an exception while unloading.", module.Manifest.GetDetailedName());

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
