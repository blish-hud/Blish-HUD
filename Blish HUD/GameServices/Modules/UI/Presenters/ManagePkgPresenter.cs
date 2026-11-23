using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Views;
using Version = SemVer.Version;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ManagePkgPresenter : Presenter<ManagePkgView, IOrderedEnumerable<PkgManifest>> {

        private ModuleManager _existingModule;

        private Func<PkgManifest, ModuleManager, IProgress<string>, Task<(ModuleManager NewModule, bool Success, string Error)>> _packageAction;

        private PkgManifest _selectedVersion;

        public ManagePkgPresenter(ManagePkgView view, IOrderedEnumerable<PkgManifest> model) : base(view, model) { /* NOOP */ }

        protected override Task<bool> Load(IProgress<string> progress) {
            _existingModule = GameService.Module.Modules.FirstOrDefault(m => m.Manifest.Namespace == this.Model.LastOrDefault()?.Namespace);

            return base.Load(progress);
        }

        private Version GetDefaultVersion() {
            // It seems to be a better user experience to always default to the latest for
            // those that want to quickly update.
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
            this.View.IsPreviewVersion  = _selectedVersion.IsPreview;

            if (_selectedVersion is PkgManifestV1 pkgv1) {
                this.View.ModuleDescription = pkgv1.Description;
            }

            (_packageAction, this.View.PackageActionText) = GetPackageAction();
            this.View.PackageActionEnabled                = _packageAction != null;
        }

        private (Func<PkgManifest, ModuleManager, IProgress<string>, Task<(ModuleManager NewModule, bool Success, string Error)>> Action, string ActionText) GetPackageAction() {
            if (_existingModule != null) {
                if (_existingModule.Manifest.Version < _selectedVersion.Version) {
                    // A newer version of the module is selected
                    return (GameService.Module.ModulePkgRepoHandler.ReplacePackage, Strings.GameServices.ModulesService.PkgManagement_Update);
                } else if (_existingModule.Manifest.Version > _selectedVersion.Version) {
                    // An older version of the module is selected
                    return (GameService.Module.ModulePkgRepoHandler.ReplacePackage, Strings.GameServices.ModulesService.PkgManagement_Downgrade);
                }

                // The selected version is the current version
                return (null, Strings.GameServices.ModulesService.PkgManagement_CurrentVersion);
            } else {
                // We don't have this module at all
                return (GameService.Module.ModulePkgRepoHandler.InstallPackage, Strings.GameServices.ModulesService.PkgManagement_Install);
            }
        }

        protected override void UpdateView() {
            this.View.ActionClicked   += OnActionClicked;
            this.View.VersionSelected += OnVersionSelected;

            SetUi();
        }

        private void SetUi() {
            this.View.ModuleVersions = this.Model.Select(m => m.Version).OrderByDescending(v => v);

            if (_existingModule != null) {
                this.View.VersionRelationship = this.Model.Max(m => m.Version) > _existingModule.Manifest.Version
                                                    ? ManagePkgView.PkgVersionRelationship.CanUpdate
                                                    : ManagePkgView.PkgVersionRelationship.CurrentVersion;
            } else {
                this.View.VersionRelationship = ManagePkgView.PkgVersionRelationship.NotInstalled;
            }

            SetActiveVersion(GetDefaultVersion());
        }

        private void OnVersionSelected(object sender, ValueEventArgs<Version> e) {
            SetActiveVersion(e.Value);
        }

        private void SetActionStatus(string status) {
            this.View.PackageActionText = status;
        }

        private async void OnActionClicked(object sender, EventArgs e) {
            this.View.PackageActionEnabled = false;

            var (newModule, installSuccess, installError) = await _packageAction?.Invoke(_selectedVersion, _existingModule, new Progress<string>(SetActionStatus));

            if (installSuccess) {
                _existingModule = newModule;
            } else {
                // TODO: Better inform the user about what failed about the install
            }

            SetUi();
        }

    }
}
