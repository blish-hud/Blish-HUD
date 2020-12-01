using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Views;
using Flurl.Http;
using Version = SemVer.Version;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ManagePkgPresenter : Presenter<ManagePkgView, IGrouping<string, PkgManifest>> {

        private static readonly Logger Logger = Logger.GetLogger<ManagePkgPresenter>();

        private ModuleManager _existingModule;

        private Func<Task> _packageAction;

        private PkgManifest _selectedVersion;

        public ManagePkgPresenter(ManagePkgView view, IGrouping<string, PkgManifest> model) : base(view, model) { /* NOOP */ }

        protected override Task<bool> Load(IProgress<string> progress) {
            _existingModule = GameService.Module.Modules.FirstOrDefault(m => m.Manifest.Namespace == this.Model.Key);

            return base.Load(progress);
        }

        private void FinalizeInstalledPackage(string modulePath) {
            if (_existingModule != null) {
                if (!GameService.Module.UnregisterModule(_existingModule)) return;
            }

            _existingModule = GameService.Module.RegisterPackedModule(modulePath);
        }

        private async Task InstallPackage() {
            Logger.Debug("Install package action.");

            this.View.PackageActionText = Strings.GameServices.ModulesService.PkgInstall_Progress_Installing;

            if (_selectedVersion is PkgManifestV1 pkgv1) {
                byte[] downloadedModule = await pkgv1.Location.GetBytesAsync();

                string moduleName = $"{_selectedVersion.Namespace}_{_selectedVersion.Version}.bhm";

                using var dataSha256 = System.Security.Cryptography.SHA256.Create();
                byte[] rawChecksum = dataSha256.ComputeHash(downloadedModule, 0, downloadedModule.Length);

                string checksum = BitConverter.ToString(rawChecksum).Replace("-", string.Empty);

                if (string.Equals(_selectedVersion.Hash, checksum, StringComparison.InvariantCultureIgnoreCase)) {
                    Logger.Info($"{moduleName} matched expected checksum '{_selectedVersion.Hash}'.");

                    string fullPath = $@"{GameService.Module.ModulesDirectory}\{moduleName}";

                    if (!File.Exists(fullPath)) {
                        File.WriteAllBytes(fullPath, downloadedModule);
                    } else {
                        Logger.Info($"Module already exists at path '{fullPath}'.");
                    }

                    Logger.Info($"Module saved to '{fullPath}'.");

                    TempUtil.VoidFilePendingDeletion(fullPath);

                    FinalizeInstalledPackage(fullPath);
                } else {
                    Logger.Warn($"{moduleName} (with checksum '{checksum}') failed to match expected checksum '{_selectedVersion.Hash}'.  The module can not be trusted.  The publisher should be contacted immediately!");
                }
            }

            SetUi();
        }

        private async Task ReplacePackage() {
            Logger.Info($"Package replacement initiated for {_existingModule.Manifest.GetDetailedName()}.");

            this.View.PackageActionText = Strings.GameServices.ModulesService.PkgInstall_Progress_Upgrading;

            string moduleName = Path.GetFileName(_existingModule.DataReader.PhysicalPath);

            if (moduleName == null || !moduleName.EndsWith(".bhm", StringComparison.InvariantCultureIgnoreCase)) {
                // Module might be a directory one - not supported
                Logger.Warn($"'{_existingModule.DataReader.PhysicalPath}' could not be updated.  Module type may not support updates.");

                return;
            }

            TempUtil.EnqueueFileForDeletion(_existingModule.DataReader.PhysicalPath);

            await InstallPackage();
        }

        private Version GetDefaultVersion() {
            if (_existingModule != null) {
                if (this.Model.Any(m => m.Version == _existingModule.Manifest.Version)) {
                    return _existingModule.Manifest.Version;
                }
            }

            return this.Model.Max(m => m.Version);
        }

        private void SetActiveVersion(Version version) {
            _selectedVersion = this.Model.First(m => m.Version == version);

            UpdateViewForVersion();
        }

        private void UpdateViewForVersion() {
            this.View.ModuleName        = _selectedVersion.Name;
            this.View.ModuleNamespace   = _selectedVersion.Namespace;
            this.View.ModuleContributor = _selectedVersion.Contributors[0];
            this.View.SelectedVersion   = _selectedVersion.Version;

            if (_selectedVersion is PkgManifestV1 pkgv1) {
                this.View.ModuleDescription = pkgv1.Description;
            }

            (_packageAction, this.View.PackageActionText) = GetPackageAction();
            this.View.PackageActionEnabled                = _packageAction != null;
        }

        private (Func<Task> Action, string ActionText) GetPackageAction() {
            if (_existingModule != null) {
                if (_existingModule.Manifest.Version < _selectedVersion.Version) {
                    // A newer version of the module is selected
                    return (ReplacePackage, Strings.GameServices.ModulesService.PkgManagement_Update);
                } else if (_existingModule.Manifest.Version > _selectedVersion.Version) {
                    // An older version of the module is selected
                    return (ReplacePackage, Strings.GameServices.ModulesService.PkgManagement_Downgrade);
                }

                // The selected version is the current version
                return (null, Strings.GameServices.ModulesService.PkgManagement_UpToDate);
            } else {
                // We don't have this module at all
                return (InstallPackage, Strings.GameServices.ModulesService.PkgManagement_Install);
            }
        }

        protected override void UpdateView() {
            this.View.ActionClicked   += OnActionClicked;
            this.View.VersionSelected += OnVersionSelected;

            SetUi();
        }

        private void SetUi() {
            this.View.ModuleVersions = this.Model.Select(m => m.Version);

            if (_existingModule != null) {
                if (this.Model.Max(m => m.Version) > _existingModule.Manifest.Version) {
                    this.View.CanUpdate = true;
                }
            }

            SetActiveVersion(GetDefaultVersion());
        }

        private void OnVersionSelected(object sender, ValueEventArgs<Version> e) {
            SetActiveVersion(e.Value);
        }

        private void OnActionClicked(object sender, EventArgs e) {
            this.View.PackageActionEnabled = false;

            _packageAction?.Invoke();
        }

    }
}
