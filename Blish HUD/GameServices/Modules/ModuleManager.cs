using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
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

        private bool _enabled              = false;
        private bool _forceAllowDependency = false;

        public bool AssemblyLoaded => _moduleAssembly != null;

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

        public bool DependenciesMet =>
            State.IgnoreDependencies
         || Manifest.Dependencies.TrueForAll(d => d.GetDependencyDetails().CheckResult == ModuleDependencyCheckResult.Available);

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

        private Assembly LoadPackagedAssembly(string assemblyPath) {
            string symbolsPath = assemblyPath.Replace(".dll", ".pdb");

            byte[] assemblyData = _dataReader.GetFileBytes(assemblyPath);
            byte[] symbolData   = _dataReader.GetFileBytes(symbolsPath) ?? new byte[0];

            return Assembly.Load(assemblyData, symbolData);
        }

        private Assembly GetResourceAssembly(Assembly requestingAssembly, AssemblyName resourceDetails, string assemblyPath) {
            // Avoid loading resource assembly from wrong module
            if (_moduleAssembly != requestingAssembly) return null;

            // English is default — ignore it
            if (!string.Equals(resourceDetails.CultureInfo.TwoLetterISOLanguageName, "en")) {
                // Non-English resource to be loaded
                assemblyPath = $"{resourceDetails.CultureName}/{assemblyPath}";

                if (_dataReader.FileExists(assemblyPath)) {
                    try {
                        return LoadPackagedAssembly(assemblyPath);
                    } catch (Exception ex) {
                        Logger.Debug(ex, "Failed to load resource {dependency} for {module}.", resourceDetails.FullName, _manifest.GetDetailedName());
                    }
                } else {
                    Logger.Debug("Resource assembly {dependency} for {module} could not be found.", resourceDetails.FullName, _manifest.GetDetailedName());
                }
            }

            return null;
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args) {
            if (_enabled || _forceAllowDependency) {
                var assemblyDetails = new AssemblyName(args.Name);

                string assemblyPath = $"{assemblyDetails.Name}.dll";

                if (!Equals(assemblyDetails.CultureInfo, CultureInfo.InvariantCulture)) {
                    return GetResourceAssembly(args.RequestingAssembly, assemblyDetails, assemblyPath);
                }

                if (!_dataReader.FileExists(assemblyPath)) return null;

                Logger.Debug("Requested dependency {dependency} ({assemblyName}) was found by module {module}.", args.Name, assemblyPath, _manifest.GetDetailedName());

                try {
                    return LoadPackagedAssembly(assemblyPath);
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
                    if (!_dataReader.FileExists(dllName)) {
                        Logger.Warn("Module {module} does not contain assembly DLL {dll}", _manifest.GetDetailedName(), dllName);
                        return;
                    }

                    _moduleAssembly = LoadPackagedAssembly(dllName);

                    if (_moduleAssembly == null) {
                        Logger.Warn("Module {module} failed to load assembly DLL {dll}.", _manifest.GetDetailedName(), dllName);
                        return;
                    }
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

            _forceAllowDependency = true;

            try {
                container.SatisfyImportsOnce(this);
            } catch (CompositionException ex) {
                Logger.Warn(ex, "Module {module} failed to be composed.", _manifest.GetDetailedName());
            } catch (FileNotFoundException ex) {
                Logger.Warn(ex, "Module {module} failed to load a dependency.", _manifest.GetDetailedName());
            }

            _forceAllowDependency = false;
        }

    }

}
