using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.GameServices;
using Blish_HUD.Overlay.SelfUpdater.Controls;
using Blish_HUD.Settings;
using Flurl.Http;

namespace Blish_HUD.Overlay {
    public class OverlayUpdateHandler : ServiceModule<OverlayService> {

        private static readonly Logger Logger = Logger.GetLogger<OverlayUpdateHandler>();

        private const string UPDATE_SETTINGS = nameof(OverlayUpdateHandler) + "Configuration";

        private const string DEFAULT_CORERELEASE_URL = "https://versions.blishhud.com/all.json";

        private CoreVersionManifest[] _availableUpdates = Array.Empty<CoreVersionManifest>();

        private SettingEntry<SemVer.Version> _lastAcknowledgedUpdate;
        private SettingEntry<bool>           _notifyOfNewReleases;

        private int _releaseLoadAttemptsRemaining = 3;

        /// <summary>
        /// The last update <see cref="SemVer.Version"/> that was acknowledged by the user.
        /// </summary>
        public SemVer.Version LastAcknowledgedRelease {
            get => _lastAcknowledgedUpdate.Value;
            set => _lastAcknowledgedUpdate.Value = value;
        }

        public bool NotifyOfNewReleases {
            get => _notifyOfNewReleases.Value;
            set => _notifyOfNewReleases.Value = value;
        }

        private SelfUpdateWindow _activeUpdateWindow;

        /// <summary>
        /// The highest version release.  Will include prereleases only if <see cref="PrereleasesVisible"/> is <c>true</c>.
        /// </summary>
        public CoreVersionManifest LatestRelease => _availableUpdates.Where(manifest => manifest.IsPrerelease == GameService.Overlay.ShowPreviews.Value)
                                                                     .OrderByDescending(manifest => manifest.Version)
                                                                     .FirstOrDefault();

        public OverlayUpdateHandler(OverlayService service) : base(service) { /* NOOP */ }

        public override void Load() {
            DefineOverlayUpdateSettings(GameService.Settings.RegisterRootSettingCollection(UPDATE_SETTINGS));
            BeginLoadReleases(DEFAULT_CORERELEASE_URL);
        }

        private void DefineOverlayUpdateSettings(SettingCollection settingCollection) {
            _lastAcknowledgedUpdate = settingCollection.DefineSetting(nameof(this.LastAcknowledgedRelease), new SemVer.Version("0.0.0"));
            _notifyOfNewReleases    = settingCollection.DefineSetting(nameof(this.NotifyOfNewRelease),      true);
        }

        private void BeginLoadReleases(string versionsUrl) {
            versionsUrl.GetJsonAsync<CoreVersionManifest[]>()
                       .ContinueWith(async coreVersionManifestTask => {
                             if (coreVersionManifestTask.Exception == null) {
                                 HandleLoadingReleases(coreVersionManifestTask.Result);
                             } else if (_releaseLoadAttemptsRemaining <= 0) {
                                 Logger.Warn(coreVersionManifestTask.Exception, "Failed to load list of release versions from '{0}'.", versionsUrl);
                             } else {
                                 // We're gonna try again in case it was just a blip
                                 _releaseLoadAttemptsRemaining--;

                                 await Task.Delay(1000);

                                 BeginLoadReleases(versionsUrl);
                             }
                        });
        }

        private void HandleLoadingReleases(CoreVersionManifest[] coreVersionManifests) {
            _availableUpdates = coreVersionManifests;

            if (this.LatestRelease.Version > Program.OverlayVersion && this.LatestRelease.Version > this.LastAcknowledgedRelease) {
                // The latest release is newer than the current version of Blish HUD and hasn't been notified about, before.
                NotifyOfNewRelease(this.LatestRelease);
            }
        }

        private void NotifyOfNewRelease(CoreVersionManifest coreVersionManifest) {
            Logger.Info("New version (v{0}) of Blish HUD detected.", coreVersionManifest.Version);

            if (this.NotifyOfNewReleases) {
                ShowReleaseSplash(coreVersionManifest, true);
            }
        }

        private void ShowReleaseSplash(CoreVersionManifest coreVersionManifest, bool subtle) {
            if (_activeUpdateWindow?.Parent == null) {
                // Release old window.
                _activeUpdateWindow = null;
            }

            _activeUpdateWindow ??= new SelfUpdateWindow(coreVersionManifest, subtle) {
                Parent = GameService.Graphics.SpriteScreen
            };

            if (!subtle) {
                _activeUpdateWindow.Show();
            }
        }

        public IEnumerable<ContextMenuStripItem> GetContextMenuItems() {
            if (this.LatestRelease.Version > Program.OverlayVersion) {
                var updateToReleaseMenuItem = new ContextMenuStripItem(string.Format(this.LatestRelease.IsPrerelease
                                                                                         ? Strings.GameServices.OverlayService.SelfUpdate_UpdateToPrereleaseMenuStripText
                                                                                         : Strings.GameServices.OverlayService.SelfUpdate_UpdateToReleaseMenuStripText,
                                                                                     this.LatestRelease.Version));

                if (!(_activeUpdateWindow is { Visible: true })) {
                    updateToReleaseMenuItem.Click += delegate { ShowReleaseSplash(this.LatestRelease, false); };
                } else {
                    updateToReleaseMenuItem.Enabled = false;
                }

                yield return updateToReleaseMenuItem;
            } else if (_availableUpdates.Length > 0 || _releaseLoadAttemptsRemaining > 0) {
                // We have releases loaded, but none of them are newer than the current version
                yield return new ContextMenuStripItem(Strings.GameServices.OverlayService.SelfUpdate_NoPendingUpdates) { Enabled = false };
            } else {
                // Seems we failed to load any releases at all
                yield return new ContextMenuStripItem(Strings.GameServices.OverlayService.SelfUpdate_PendingUpdatesQueryFailed) { Enabled = false };
            }
        }

        public void AcknowledgePendingReleases() {
            this.LastAcknowledgedRelease = _availableUpdates.Select(manifest => manifest.Version).Max();
        }

    }
}
