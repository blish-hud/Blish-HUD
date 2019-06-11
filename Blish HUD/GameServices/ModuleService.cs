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
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD {

    public class ModuleManager {

        public event EventHandler<EventArgs> ModuleEnabled;
        public event EventHandler<EventArgs> ModuleDisabled;

        private readonly Manifest    _manifest;
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
            }
        }

        public Manifest Manifest => _manifest;

        [Import]
        public ExternalModule ModuleInstance { get; private set; }

        public ModuleManager(Manifest manifest, IDataReader dataReader) {
            _manifest   = manifest;
            _dataReader = dataReader;
        }

        public void Enable() {
            var moduleParams = ModuleParameters.BuildFromManifest(_manifest, _dataReader);

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

        public void Disable() {
            // TODO: Disable module
        }

        private void ComposeModuleFromFileSystemReader(string dllName, ModuleParameters parameters) {
            byte[] assemblyData = _dataReader.GetFileBytes(dllName);

            var catalog   = new AssemblyCatalog(Assembly.Load(assemblyData));
            var container = new CompositionContainer(catalog);

            container.ComposeExportedValue("ModuleParameters", parameters);

            container.SatisfyImportsOnce(this);
        }

    }

    public class ModuleService : GameService {

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";

        private const string MODULES_DIRECTORY = "modules";

        private const string MODULE_EXTENSION = ".bhm";

        private string ModulesDirectory => Path.Combine(GameService.Directory.BasePath, MODULES_DIRECTORY);

        private SettingEntry<Dictionary<string, bool>> _moduleStates;

        public SettingEntry<Dictionary<string, bool>> ModuleStates => _moduleStates;

        private readonly List<ModuleManager> _modules;
        public IReadOnlyList<ModuleManager> Modules => _modules.ToList();

        public List<IModule> AvailableModules { get; private set; }

        public List<ExternalModule> ExternalModules { get; set; }

        public ModuleService() {
            _modules = new List<ModuleManager>();
        }

        protected override void Initialize() {
            _moduleStates = Settings.CoreSettings.DefineSetting(MODULESTATES_CORE_SETTING,
                                                                new Dictionary<string, bool>(),
                                                                new Dictionary<string, bool>());
        }

        //this.AvailableModules.Add(module);

        //// All modules are disabled by default, currently to allow for users to select which modules they would like to enable
        //if (this.ModuleStates.Value.ContainsKey(module.GetModuleInfo().Namespace)) {
        //    module.Enabled = this.ModuleStates.Value[module.GetModuleInfo().Namespace];
        //} else {
        //    this.ModuleStates.Value.Add(module.GetModuleInfo().Namespace, false);
        //}

        public ModuleManager RegisterModule(IDataReader moduleReader) {
            string manifestContents;
            using (var manifestReader = new StreamReader(moduleReader.GetFileStream("manifest.json"))) {
                manifestContents = manifestReader.ReadToEnd();
            }

            var moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);
            var moduleManager  = new ModuleManager(moduleManifest, moduleReader);

            _modules.Add(moduleManager);

            if (this.ModuleStates.Value.ContainsKey(moduleManifest.Namespace)) {
                moduleManager.Enabled = this.ModuleStates.Value[moduleManifest.Namespace];
            } else {
                this.ModuleStates.Value.Add(moduleManifest.Namespace, false);
            }

            return moduleManager;
        }

        protected override void Load() {
            if (!System.IO.Directory.Exists(this.ModulesDirectory)) System.IO.Directory.CreateDirectory(this.ModulesDirectory);

            // TODO: Load modules dynamically
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

            foreach (var moduleArchivePath in System.IO.Directory.GetFiles(this.ModulesDirectory, $"*{MODULE_EXTENSION}", SearchOption.AllDirectories)) {
                ZipArchiveReader moduleReader = new ZipArchiveReader(moduleArchivePath);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }

#if DEBUG
            foreach (var manifestPath in System.IO.Directory.GetFiles(this.ModulesDirectory, "manifest.json", SearchOption.AllDirectories)) {
                string moduleDir = System.IO.Directory.GetParent(manifestPath).FullName;

                DirectoryReader moduleReader = new DirectoryReader(moduleDir);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }
#endif

            //foreach (var externalModule in this.ExternalModules) {
            //    Console.WriteLine($"Registering external module: {externalModule.Name} with Namespace {externalModule.Namespace}");
            //    externalModule.DoInitialize();
            //    //RegisterModule(externalModule);
            //}
        }

        protected override void Unload() {

        }

        protected override void Update(GameTime gameTime) {
            _modules.ForEach(s => {
                try {
                    if (s.Enabled) s.ModuleInstance.DoUpdate(gameTime);
                } catch (Exception ex) {
#if DEBUG
                    //throw;
#endif
                    Console.WriteLine($"{s.Manifest.Name} module had an error:");
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
