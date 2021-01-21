using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Reflection;
using Blish_HUD.Content;

namespace Blish_HUD.Modules {

    public class ModuleManager : IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<ModuleManager>();

        public event EventHandler<EventArgs> ModuleEnabled;
        public event EventHandler<EventArgs> ModuleDisabled;

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

                this.State.Enabled = _enabled;
                GameService.Settings.Save();
            }
        }

        public bool DependenciesMet =>
            State.IgnoreDependencies
         || Manifest.Dependencies.TrueForAll(d => d.GetDependencyDetails().CheckResult == ModuleDependencyCheckResult.Available);

        public Manifest Manifest { get; }

        public ModuleState State { get; }

        public IDataReader DataReader { get; }

        [Import]
        public Module ModuleInstance { get; private set; }

        public ModuleManager(Manifest manifest, ModuleState state, IDataReader dataReader) {
            this.Manifest   = manifest;
            this.State      = state;
            this.DataReader = dataReader;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        private void Enable() {
            var moduleParams = ModuleParameters.BuildFromManifest(this.Manifest, this);

            if (moduleParams != null) {
                string packagePath = this.Manifest.Package.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                         ? this.Manifest.Package
                                         : $"{this.Manifest.Package}.dll";

                if (this.DataReader.FileExists(packagePath)) {
                    ComposeModuleFromFileSystemReader(packagePath, moduleParams);

                    if (this.ModuleInstance != null) {
                        _enabled = true;

                        this.ModuleInstance.DoInitialize();
                        this.ModuleInstance.DoLoad();
                    }
                } else {
                    throw new FileNotFoundException($"Assembly '{packagePath}' could not be loaded from {this.DataReader.GetType().Name}.");
                }
            }
        }

        private Assembly LoadPackagedAssembly(string assemblyPath) {
            string symbolsPath = assemblyPath.Replace(".dll", ".pdb");

            byte[] assemblyData = this.DataReader.GetFileBytes(assemblyPath);
            byte[] symbolData   = this.DataReader.GetFileBytes(symbolsPath) ?? new byte[0];

            return Assembly.Load(assemblyData, symbolData);
        }

        private Assembly GetResourceAssembly(Assembly requestingAssembly, AssemblyName resourceDetails, string assemblyPath) {
            // Avoid loading resource assembly from wrong module
            if (_moduleAssembly != requestingAssembly) return null;

            // English is default — ignore it
            if (!string.Equals(resourceDetails.CultureInfo.TwoLetterISOLanguageName, "en")) {
                // Non-English resource to be loaded
                assemblyPath = $"{resourceDetails.CultureName}/{assemblyPath}";

                if (this.DataReader.FileExists(assemblyPath)) {
                    try {
                        return LoadPackagedAssembly(assemblyPath);
                    } catch (Exception ex) {
                        Logger.Debug(ex, "Failed to load resource {dependency} for {module}.", resourceDetails.FullName, this.Manifest.GetDetailedName());
                    }
                } else {
                    Logger.Debug("Resource assembly {dependency} for {module} could not be found.", resourceDetails.FullName, this.Manifest.GetDetailedName());
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

                if (!this.DataReader.FileExists(assemblyPath)) return null;

                Logger.Debug("Requested dependency {dependency} ({assemblyName}) was found by module {module}.", args.Name, assemblyPath, this.Manifest.GetDetailedName());

                try {
                    return LoadPackagedAssembly(assemblyPath);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to load dependency {dependency} for {module}.", args.Name, this.Manifest.GetDetailedName());
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
                    if (!this.DataReader.FileExists(dllName)) {
                        Logger.Warn("Module {module} does not contain assembly DLL {dll}", this.Manifest.GetDetailedName(), dllName);
                        return;
                    }

                    _moduleAssembly = LoadPackagedAssembly(dllName);

                    if (_moduleAssembly == null) {
                        Logger.Warn("Module {module} failed to load assembly DLL {dll}.", this.Manifest.GetDetailedName(), dllName);
                        return;
                    }
                } catch (ReflectionTypeLoadException ex) {
                    Logger.Warn(ex, "Module {module} failed to load due to a type exception. Ensure that you are using the correct version of the Module", this.Manifest.GetDetailedName());
                    return;
                } catch (BadImageFormatException ex) {
                    Logger.Warn(ex, "Module {module} failed to load.  Check that it is a compiled module.", this.Manifest.GetDetailedName());
                    return;
                } catch (Exception ex) {
                    Logger.Warn(ex, "Module {module} failed to load due to an unexpected error.", this.Manifest.GetDetailedName());
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
                Logger.Warn(ex, "Module {module} failed to be composed.", this.Manifest.GetDetailedName());
            } catch (FileNotFoundException ex) {
                Logger.Warn(ex, "Module {module} failed to load a dependency.", this.Manifest.GetDetailedName());
            }

            _forceAllowDependency = false;
        }

        public void Dispose() {
            Disable();

            this.DataReader?.Dispose();
        }

    }

}
