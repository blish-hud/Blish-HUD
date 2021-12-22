using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.SelfUpdater;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Settings;
using Version = SemVer.Version;

namespace Blish_HUD.Overlay.UI.Presenters {
    public class CoreUpdatePresenter : Presenter<CoreUpdateView, string> {

        private static readonly Logger Logger = Logger.GetLogger<CoreUpdatePresenter>();

        private const string CORERELEASE_SETTINGS    = "CoreReleaseConfiguration";
        private const string CORERELEASE_URL_SETTING = "AllUrl";
        private const string SHOWPRERELEASES_SETTING = "ShowPrereleases";

        private const string DEFAULT_CORERELEASE_URL = "https://versions.blishhud.com/all.json";

        private CoreVersionManifest[] _releases;

        private readonly SettingEntry<bool> _showPrereleases;
        private readonly SettingEntry<string> _versionsUrl;

        private CoreVersionManifest _selectedRelease;

        public CoreUpdatePresenter(CoreUpdateView view, string model) : base(view, model) {
            var settings = GameService.Settings.RegisterRootSettingCollection(CORERELEASE_SETTINGS);

            _versionsUrl = settings.DefineSetting(CORERELEASE_URL_SETTING, DEFAULT_CORERELEASE_URL);
            _showPrereleases = settings.DefineSetting(SHOWPRERELEASES_SETTING, false);
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            progress.Report("Loading release versions...");
            (CoreVersionManifest[] releases, var releaseRequestException) = await CoreReleaseVersionsProvider.GetAvailableReleases(_versionsUrl.Value);

            if (releaseRequestException == null && releases.Any()) {
                _releases = releases;
            }

            return true;
        }

        private Version[] GetApplicableVersions() {
            return _releases.Where(release => !release.IsPrerelease || _showPrereleases.Value)
                            .Select(release => release.Version).ToArray();
        }

        private void SetActiveVersion(Version version) {
            _selectedRelease = _releases.FirstOrDefault(release => release.Version == version);

            UpdateVersionsView();
        }

        private void UpdateVersionsView() {
            this.View.SelectedVersion = _selectedRelease.Version;

            UpdateViewForVersion();
        }

        protected override void UpdateView() {
            if (_releases != null) {
                this.View.ActionClicked += OnActionClicked;
                this.View.VersionSelected += OnVersionSelected;
                this.View.ShowPrereleasesChanged += OnPrereleasesChanged;
            }

            SetUi();
        }

        private void UpdateViewForVersion() {
            var actionDetails = GetVersionAction(_selectedRelease.Version);

            this.View.PackageActionEnabled = actionDetails.CanInstall;
            this.View.PackageActionText = actionDetails.ActionText;
        }

        private void OnPrereleasesChanged(object sender, ValueEventArgs<bool> e) {
            _showPrereleases.Value = e.Value;
        }

        private void OnVersionSelected(object sender, ValueEventArgs<Version> e) {
            SetActiveVersion(e.Value);
        }

        private async void OnActionClicked(object sender, EventArgs e) {
            //await GameService.Overlay.SelfUpdate(_selectedRelease);
        }

        private (bool CanInstall, string ActionText) GetVersionAction(Version selectedVersion) {
            if (Program.OverlayVersion < selectedVersion) {
                // A new version of Blish HUD is selected
                return (true, Strings.GameServices.ModulesService.PkgManagement_Update);
            } else if (Program.OverlayVersion > selectedVersion) {
                // An older version of Blish HUD is selected
                return (true, Strings.GameServices.ModulesService.PkgManagement_Downgrade);
            }

            // The selected version is the current version
            return (false, Strings.GameServices.ModulesService.PkgManagement_CurrentVersion);
        }

        private void SetUi() {
            if (_releases == null) {
                this.View.CoreVersions = null;
                this.View.PackageActionText = "Unavailable";
                return;
            }

            this.View.CoreVersions = GetApplicableVersions().OrderByDescending(version => version);

            SetActiveVersion(this.View.CoreVersions.First());
        }

    }
}