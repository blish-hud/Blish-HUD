using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Module = Blish_HUD.Modules.Module;

namespace Blish_HUD {

    public class ModuleManager {

        public event EventHandler<EventArgs> ModuleEnabled;
        public event EventHandler<EventArgs> ModuleDisabled;

        private readonly Manifest    _manifest;
        private readonly ModuleState _state;
        private readonly IDataReader _dataReader;

        private bool _enabled = false;

        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;

                _enabled = value;

                if (_enabled) {
                    this.Enable();
                    ModuleEnabled?.Invoke(this, EventArgs.Empty);
                } else {
                    this.Disable();
                    ModuleDisabled?.Invoke(this, EventArgs.Empty);
                }

                _state.Enabled = _enabled;
                GameService.Settings.Save();
            }
        }

        public Manifest Manifest => _manifest;

        public ModuleState State => _state;

        public IDataReader DataReader => _dataReader;

        [Import]
        public Module ModuleInstance { get; private set; }

        public ModuleManager(Manifest manifest, ModuleState state, IDataReader dataReader) {
            _manifest   = manifest;
            _state      = state;
            _dataReader = dataReader;
        }

        private void Enable() {
            var moduleParams = ModuleParameters.BuildFromManifest(_manifest, this);

            if (moduleParams == null) {
                _enabled = false;
                return;
            }

            string packagePath = _manifest.Package.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                     ? _manifest.Package
                                     : $"{Manifest.Package}.dll";

            if (_dataReader.FileExists(packagePath)) {
                ComposeModuleFromFileSystemReader(packagePath, moduleParams);
            } else {
                throw new FileNotFoundException($"Assembly '{packagePath}' could not be loaded from {_dataReader.GetType().Name}.");
            }

            this.ModuleInstance.DoInitialize();
            this.ModuleInstance.DoLoad();
        }

        private void Disable() {
            this.ModuleInstance?.Dispose();
            this.ModuleInstance = null;
        }

        private void ComposeModuleFromFileSystemReader(string dllName, ModuleParameters parameters) {
            string symbolsPath = dllName.Replace(".dll", ".pdb");

            byte[] assemblyData = _dataReader.GetFileBytes(dllName);
            byte[] symbolData = _dataReader.GetFileBytes(symbolsPath) ?? new byte[0];

            var moduleAssembly = Assembly.Load(assemblyData, symbolData);

            var catalog   = new AssemblyCatalog(moduleAssembly);
            var container = new CompositionContainer(catalog);

            container.ComposeExportedValue("ModuleParameters", parameters);

            container.SatisfyImportsOnce(this);
        }

    }

    public class ModuleState {

        public bool Enabled { get; set; }

        public TokenPermission[] UserEnabledPermissions { get; set; }

        public SettingCollection Settings { get; set; }

    }

    public class ModuleService : GameService {

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";

        private const string MODULES_DIRECTORY = "modules";

        private const string MODULE_EXTENSION = ".bhm";

        private string ModulesDirectory => Directory.RegisterDirectory(MODULES_DIRECTORY);

        private SettingEntry<Dictionary<string, ModuleState>> _moduleStates;

        public SettingEntry<Dictionary<string, ModuleState>> ModuleStates => _moduleStates;

        private readonly List<ModuleManager> _modules;
        public IReadOnlyList<ModuleManager> Modules => _modules.ToList();

        public ModuleService() {
            _modules = new List<ModuleManager>();
        }

        protected override void Initialize() {
            _moduleStates = Settings.Settings.DefineSetting(MODULESTATES_CORE_SETTING, new Dictionary<string, ModuleState>());
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

        protected override void Load() {
            if (!System.IO.Directory.Exists(this.ModulesDirectory)) System.IO.Directory.CreateDirectory(this.ModulesDirectory);

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

            foreach (string moduleArchivePath in System.IO.Directory.GetFiles(this.ModulesDirectory, $"*{MODULE_EXTENSION}", SearchOption.AllDirectories)) {
                var moduleReader = new ZipArchiveReader(moduleArchivePath);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }

#if DEBUG
            foreach (string manifestPath in System.IO.Directory.GetFiles(this.ModulesDirectory, "manifest.json", SearchOption.AllDirectories)) {
                string moduleDir = System.IO.Directory.GetParent(manifestPath).FullName;

                var moduleReader = new DirectoryReader(moduleDir);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }
#endif
        }

        protected override void Unload() {

        }

        protected override void Update(GameTime gameTime) {
            _modules.ForEach(s => {
                //try {
                    if (s.Enabled) s.ModuleInstance.DoUpdate(gameTime);
//                } catch (Exception ex) {
//#if DEBUG
//                    throw;
//#endif
//                    Console.WriteLine($"{s.Manifest.Name} module had an error:");
//                    Console.WriteLine(ex.Message);
//                }
            });
        }
    }
}
