using System;
using System.Threading.Tasks;
using Flurl.Http;

namespace Blish_HUD.Overlay.SelfUpdater {
    public static class CoreReleaseVersionsProvider {

        private static readonly Logger Logger = Logger.GetLogger(typeof(CoreReleaseVersionsProvider));

        public static async Task<(CoreVersionManifest[] Releases, Exception Exception)> GetAvailableReleases(string versionsUrl) {
            try {
                return (await versionsUrl.GetJsonAsync<CoreVersionManifest[]>(), null);
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed to load list of release versions from '{versionsUrl}'.");
                return (Array.Empty<CoreVersionManifest>(), ex);
            }
        }

    }
}