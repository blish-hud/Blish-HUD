using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.GameServices;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Settings;
using Flurl.Http;
using Humanizer;

namespace Blish_HUD.Modules {
    public class ModulePkgRepoHandler : ServiceModule<ModuleService> {

        private static readonly Logger Logger = Logger.GetLogger<ModulePkgRepoHandler>();

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

        private View GetRepoView(MenuItem repoMenuItem) {
            AcknowledgePendingModuleUpdates();

            return new ModuleRepoView(_defaultRepoProvider);
        }

        #region Module Update Indicators

        private bool GetUpdateIsNotAcknowledged(PkgManifest modulePkg) {
            if (_acknowledgedUpdates.TryGetSetting(modulePkg.Namespace, out var setting) && setting is SettingEntry<string> acknowledgedModuleUpdate) {
                return modulePkg.Version > new SemVer.Version(acknowledgedModuleUpdate.Value, true);
            }

            return true;
        }

        private void RefreshUpdateIndicatorStates() {
            // TODO: Blish HUD icon should be handled in the Overlay service - this will likely have to wait for the old TabbedWindow to get replaced with the new one.

            if (this.UnacknowledgedUpdates.Any(module => GameService.Overlay.ShowPreviews.Value || !module.IsPreview)) {
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

        private void AcknowledgePendingModuleUpdates() {
            // Mark all updates as acknowledged
            foreach (var unacknowledgedModuleUpdate in this.UnacknowledgedUpdates) {
                if (!_acknowledgedUpdates.TryGetSetting<string>(unacknowledgedModuleUpdate.Namespace, out SettingEntry<string> acknowledgementEntry)) {
                    acknowledgementEntry = _acknowledgedUpdates.DefineSetting(unacknowledgedModuleUpdate.Namespace, "0.0.0");
                }

                acknowledgementEntry.Value = unacknowledgedModuleUpdate.Version.ToString();
            }

            RefreshUpdateIndicatorStates();
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
            var latest = pkgGroup.Last();
            var current = _service.Modules.FirstOrDefault(module => string.Equals(module.Manifest.Namespace, latest.Namespace, StringComparison.OrdinalIgnoreCase));

            if (current != null) {
                return $"{latest.Name} v{current.Manifest.Version} -> v{latest.Version}";
            }

            // Shouldn't be possible.
            return $"{latest.Name} v{latest.Version}";
        }

        #endregion

        #region Module Install/Replace/Remove from PkgManifest

        private ModuleManager FinalizeInstalledPackage(ModuleManager existingModule, string newModulePath) {
            existingModule?.DeleteModule();

            return GameService.Module.RegisterPackedModule(newModulePath);
        }

        public async Task<(ModuleManager NewModule, bool Success, string Error)> ReplacePackage(PkgManifest pkgManifest, ModuleManager existingModule, IProgress<string> progress = null) {
            Logger.Info($"Package replacement initiated for {existingModule.Manifest.GetDetailedName()}.");

            bool wasEnabled = existingModule.Enabled;

            if (wasEnabled) {
                progress?.Report(Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_PackageStatus_DisablingModule);
                existingModule.Disable();
            }

            progress?.Report(Strings.GameServices.ModulesService.PkgInstall_Progress_Upgrading);

            string moduleName = Path.GetFileName(existingModule.DataReader.PhysicalPath);

            if (moduleName == null || !moduleName.EndsWith(".bhm", StringComparison.InvariantCultureIgnoreCase)) {
                // Module might be a directory one - not supported
                Logger.Warn($"'{existingModule.DataReader.PhysicalPath}' could not be updated.  Module type may not support updates.");

                // TODO: Localize error 'Module type not supported.' response
                return (null, false, "Module type not supported.");
            }

            var installResult = await InstallPackage(pkgManifest, existingModule, progress);

            if (wasEnabled) {
                // Ensure that module is set to enabled for when Blish HUD restarts
                GameService.Module.ModuleStates.Value[existingModule.Manifest.Namespace].Enabled = true;
                GameService.Settings.Save();
            }

            return installResult;
        }

        public async Task<(ModuleManager NewModule, bool Success, string Error)> InstallPackage(PkgManifest pkgManifest, ModuleManager existingModule = null, IProgress<string> progress = null) {
            Logger.Debug("Install package action.");

            progress?.Report(Strings.GameServices.ModulesService.PkgInstall_Progress_Installing);

            if (pkgManifest is PkgManifestV1 pkgv1) {
                try {
                    byte[] downloadedModule = await pkgv1.Location.GetBytesAsync();

                    string moduleName = $"{pkgManifest.Namespace}_{pkgManifest.Version}.bhm";

                    using var dataSha256 = System.Security.Cryptography.SHA256.Create();
                    byte[] rawChecksum = dataSha256.ComputeHash(downloadedModule, 0, downloadedModule.Length);

                    string checksum = BitConverter.ToString(rawChecksum).Replace("-", string.Empty);

                    if (string.Equals(pkgManifest.Hash, checksum, StringComparison.InvariantCultureIgnoreCase)) {
                        Logger.Info($"{moduleName} matched expected checksum '{pkgManifest.Hash}'.");

                        string fullPath = $@"{GameService.Module.ModulesDirectory}\{moduleName}";

                        if (!File.Exists(fullPath)) {
                            File.WriteAllBytes(fullPath, downloadedModule);
                        } else {
                            Logger.Warn($"Module already exists at path '{fullPath}'.");
                            // TODO: Localize error 'Module already exists at the path {0}.' response
                            return (null, false, $"Module already exists at the path {fullPath}.");
                        }

                        Logger.Info($"Module saved to '{fullPath}'.");

                        return (FinalizeInstalledPackage(existingModule, fullPath), true, string.Empty);
                    } else {
                        Logger.Warn($"{moduleName} (with checksum '{checksum}') failed to match expected checksum '{pkgManifest.Hash}'.  The module can not be trusted.  The publisher should be contacted immediately!");

                        // TODO: Revise and localize error for this response.
                        return (null, false, "Checksum failed for module!");
                    }
                } catch (Exception ex) {
                    Logger.Error(ex, "Failed to install module.");

                    // TODO: Revise and localize error for this response.
                    return (null, false, $"Module failed to install {ex}");
                }
            }

            // TODO: Revise and localize error for this response.
            return (null, false, "Module was not installed - PkgManifest may be incorrect.");
        }

        #endregion

    }
}
