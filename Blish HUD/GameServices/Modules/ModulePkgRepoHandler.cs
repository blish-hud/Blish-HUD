using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.GameServices;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Settings;
using Humanizer;

namespace Blish_HUD.Modules {
    public class ModulePkgRepoHandler : ServiceModule<ModuleService> {

        // TODO: ModuleRepos should be made to handle multiple repos - not just our public one

        private const string REPO_SETTINGS = "ModuleRepoConfiguration";

        private const string DEFAULT_REPOURL_SETTING      = "DefaultPkgsUrl";
        private const string ACKNOWLEDGED_UPDATES_SETTING = "AcknowledgedUpdates";

        private const string DEFAULT_BHUDPKGS_REPOURL = "https://pkgs.blishhud.com/";

        private const string TEXTUREREF_REPOMENU               = "156764-noarrow";
        private const string TEXTUREREF_REPOMENU_PENDINGUPDATE = "156764-update";

        private readonly List<IPkgRepoProvider> _repos = new List<IPkgRepoProvider>();

        private IGrouping<string, PkgManifest>[] _pendingUpdates = Array.Empty<IGrouping<string, PkgManifest>>();

        /// <summary>
        /// All pending module updates applicable to the current version of Blish HUD.
        /// </summary>
        public IEnumerable<PkgManifest> PendingUpdates => _pendingUpdates.Select(grouping => grouping.Last());

        /// <summary>
        /// Pending module updates which have not been acknowledged by the user.
        /// </summary>
        public IEnumerable<PkgManifest> UnacknowledgedUpdates => this.PendingUpdates.Where(GetUpdateIsNotAcknowledged);

        /// <summary>
        /// The collection of repos which are available to look up modules.
        /// </summary>
        public IReadOnlyCollection<IPkgRepoProvider> PkgRepos => _repos.AsReadOnly();

        private SettingEntry<string> _defaultRepoUrlSetting;
        private SettingCollection    _acknowledgedUpdates;

        private MenuItem         _repoMenuItem;
        private IPkgRepoProvider _defaultRepoProvider;

        public ModulePkgRepoHandler(ModuleService service) : base(service) { /* NOOP */ }
        
        public override void Load() {
            DefineModuleRepoSettings(GameService.Settings.RegisterRootSettingCollection(REPO_SETTINGS));
            _defaultRepoProvider = new StaticPkgRepoProvider(_defaultRepoUrlSetting.Value);
            _repos.Add(_defaultRepoProvider);

            RegisterRepoManagementInSettingsUi();
        }

        private bool GetUpdateIsNotAcknowledged(PkgManifest modulePkg) {
            if (_acknowledgedUpdates.TryGetSetting(modulePkg.Namespace, out var setting)) {
                if (setting is SettingEntry<string> acknowledgedModuleUpdate) {
                    return modulePkg.Version > new SemVer.Version(acknowledgedModuleUpdate.Value, true);
                }
            }

            return true;
        }

        private void DefineModuleRepoSettings(SettingCollection settingCollection) {
            _defaultRepoUrlSetting = settingCollection.DefineSetting(DEFAULT_REPOURL_SETTING, DEFAULT_BHUDPKGS_REPOURL);

            _acknowledgedUpdates = settingCollection.AddSubCollection(ACKNOWLEDGED_UPDATES_SETTING);
        }

        private void RegisterRepoManagementInSettingsUi() {
            _repoMenuItem = new MenuItem(Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepoSection, GameService.Content.GetTexture(TEXTUREREF_REPOMENU));

            GameService.Overlay.SettingsTab.RegisterSettingMenu(_repoMenuItem, GetRepoView, int.MaxValue - 11);

            // TODO: Check all repos
            _defaultRepoProvider.Load(null).ContinueWith(RepoResultsLoaded);
        }

        private void RefreshUpdateIndicatorStates() {
            // TODO: Blish HUD icon should be handled in the Overlay service - this will likely have to wait for the old TabbedWindow to get replaced with the new one.

            if (this.UnacknowledgedUpdates.Any()) {
                // settings menu item indicator
                _repoMenuItem.Text             = $"{Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepoSection} ({Strings.GameServices.ModulesService.PkgManagement_Update.ToQuantity(_pendingUpdates.Length)})";
                _repoMenuItem.Icon             = GameService.Content.GetTexture(TEXTUREREF_REPOMENU_PENDINGUPDATE);
                _repoMenuItem.BasicTooltipText = $"{Strings.GameServices.ModulesService.PkgManagement_Update.Pluralize()}:\n\n{string.Join("\n", _pendingUpdates.Select(GetUpgradePathStringFromRepoPkgGroup))}";

                // Main Blish HUD icon
                GameService.Overlay.BlishMenuIcon.Icon      = GameService.Content.GetTexture("logo-update");
                GameService.Overlay.BlishMenuIcon.HoverIcon = GameService.Content.GetTexture("logo-big-update");
            } else {
                // settings menu item indicator
                _repoMenuItem.Icon             = GameService.Content.GetTexture(TEXTUREREF_REPOMENU);
                _repoMenuItem.Text             = Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepoSection;
                _repoMenuItem.BasicTooltipText = null;

                // Main Blish HUD icon
                GameService.Overlay.BlishMenuIcon.Icon      = GameService.Content.GetTexture("logo");
                GameService.Overlay.BlishMenuIcon.HoverIcon = GameService.Content.GetTexture("logo-big");
            }
        }

        private void AcknowlegePendingModuleUpdates() {
            // Mark all updates as acknowledged
            foreach (var unacknowlegedModuleUpdate in this.UnacknowledgedUpdates) {
                if (!_acknowledgedUpdates.TryGetSetting<string>(unacknowlegedModuleUpdate.Namespace, out SettingEntry<string> acknowledgementEntry)) {
                    acknowledgementEntry = _acknowledgedUpdates.DefineSetting(unacknowlegedModuleUpdate.Namespace, "0.0.0");
                }

                acknowledgementEntry.Value = unacknowlegedModuleUpdate.Version.ToString();
            }

            RefreshUpdateIndicatorStates();
        }

        private View GetRepoView(MenuItem repoMenuItem) {
            AcknowlegePendingModuleUpdates();

            return new ModuleRepoView(_defaultRepoProvider);
        }

        private void RepoResultsLoaded(Task<bool> repoLoadedTask) {
            if (repoLoadedTask.Result) {
                _pendingUpdates = _defaultRepoProvider.GetPkgManifests(new Func<PkgManifest, bool>[] { StaticPkgRepoProvider.FilterShowOnlySupportedVersion, StaticPkgRepoProvider.FilterShowOnlyUpdates })
                                                      .GroupBy(pkgManfiest => pkgManfiest.Namespace)
                                                      .ToArray();

                RefreshUpdateIndicatorStates();
            }
        }

        private string GetUpgradePathStringFromRepoPkgGroup(IGrouping<string, PkgManifest> pkgGroup) {
            var latest  = pkgGroup.Last();
            var current = _service.Modules.FirstOrDefault(module => string.Equals(module.Manifest.Namespace, latest.Namespace, StringComparison.OrdinalIgnoreCase));

            if (current != null) {
                return $"{latest.Name} v{current.Manifest.Version} -> v{latest.Version}";
            }

            // Shouldn't be possible.
            return $"{latest.Name} v{latest.Version}";
        }

    }
}
