using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Blish_HUD.Content;

namespace Blish_HUD.Modules {

    public class ModuleManager {

        private static readonly Logger Logger = Logger.GetLogger<ModuleManager>();

        public event EventHandler<EventArgs> ModuleEnabled;
        public event EventHandler<EventArgs> ModuleDisabled;

        private readonly Manifest    _manifest;
        private readonly ModuleState _state;
        private readonly IDataReader _dataReader;

        private Assembly _moduleAssembly;

        private bool _enabled = false;

        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;

                if (value) { 
                    this.Enable();

                    if (_enabled) {
                        this.ModuleEnabled?.Invoke(this, EventArgs.Empty);
                    }
                } else {
                    this.Disable();

                    if (!_enabled) {
                        this.ModuleDisabled?.Invoke(this, EventArgs.Empty);
                    }
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

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        private void Enable() {
            var moduleParams = ModuleParameters.BuildFromManifest(_manifest, this);

            if (moduleParams != null) {
                string packagePath = _manifest.Package.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                         ? _manifest.Package
                                         : $"{this.Manifest.Package}.dll";

                if (_dataReader.FileExists(packagePath)) {
                    ComposeModuleFromFileSystemReader(packagePath, moduleParams);

                    if (this.ModuleInstance != null) {
                        _enabled = true;

                        this.ModuleInstance.DoInitialize();
                        this.ModuleInstance.DoLoad();
                    }
                } else {
                    throw new FileNotFoundException($"Assembly '{packagePath}' could not be loaded from {_dataReader.GetType().Name}.");
                }
            }
        }

        private Assembly LoadPackagedAssembly(string assemblyName) {
            // All assemblies must be at the root of the BHM.
            string symbolsPath = assemblyName.Replace(".dll", ".pdb");

            byte[] assemblyData = _dataReader.GetFileBytes(assemblyName);
            byte[] symbolData   = _dataReader.GetFileBytes(symbolsPath) ?? new byte[0];

            return Assembly.Load(assemblyData, symbolData);
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args) {
            if (_enabled && _moduleAssembly == args.RequestingAssembly) {
                try {
                    string assemblyName = $"{args.Name.Split(',')[0]}.dll";

                    Logger.Debug("Module {module} requested to load dependency {dependency} ({assemblyName}).", _manifest.GetDetailedName(), args.Name, assemblyName);

                    return LoadPackagedAssembly(assemblyName);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to load dependency {dependency} for {module}.", args.Name, _manifest.GetDetailedName());
                }
            }

            return null;
        }

        private void Disable() {
            _enabled = false;

            this.ModuleInstance?.Dispose();
            this.ModuleInstance = null;
        }

        private void ComposeModuleFromFileSystemReader(string dllName, ModuleParameters parameters) {
            if (_moduleAssembly == null) {
                try {
                    _moduleAssembly = LoadPackagedAssembly(dllName);
                } catch (ReflectionTypeLoadException ex) {
                    Logger.Warn(ex, "Module {module} failed to load due to a type exception. Ensure that you are using the correct version of the Module", _manifest.GetDetailedName());
                    return;
                } catch (BadImageFormatException ex) {
                    Logger.Warn(ex, "Module {module} failed to load.  Check that it is a compiled module.", _manifest.GetDetailedName());
                    return;
                } catch (Exception ex) {
                    Logger.Warn(ex, "Module {module} failed to load due to an unexpected error.", _manifest.GetDetailedName());
                    return;
                }
            }

            var catalog   = new AssemblyCatalog(_moduleAssembly);
            var container = new CompositionContainer(catalog);

            container.ComposeExportedValue("ModuleParameters", parameters);

            try {
                container.SatisfyImportsOnce(this);
            } catch (CompositionException ex) {
                Logger.Warn(ex, "Module {module} failed to be composed.", _manifest.GetDetailedName());
            } catch (FileNotFoundException ex) {
                Logger.Warn(ex, "Module {module} failed to load a dependency.", _manifest.GetDetailedName());
            }
        }

    }

}
