using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;

namespace Blish_HUD.Modules.Pkgs {
    public class GitHubPkgRepoProvider : StaticPkgRepoProvider {

        private const string GITHUB_RELEASES_URI = "/releases";
        private const string ASSET_PACKAGE_NAME  = "packages.gz";

        [Serializable]
        public struct GitHubRelease {

            public string Name { get; set; }

            public GitHubAsset[] Assets { get; set; }

        }

        [Serializable]
        public struct GitHubAsset {

            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }

        }

        protected GitHubPkgRepoProvider() { /* NOOP */ }

        public GitHubPkgRepoProvider(string repoUrl) : base(repoUrl) { /* NOOP */ }

        protected override async Task<PkgManifest[]> LoadRepo(IProgress<string> progress = null) {
            progress?.Report(Strings.GameServices.ModulesService.PkgManagement_Progress_CheckingRepository);
            var releaseResult = await UpdateGitHubReleases();
            
            if (releaseResult.Exception != null) {
                progress?.Report($"{Strings.GameServices.ModulesService.PkgManagement_Progress_FailedToGetReleases}\r\n{releaseResult.Exception.Message}");
                return null;
            }

            progress?.Report(Strings.GameServices.ModulesService.PkgManagement_Progress_GettingModuleList);
            var manifests = await LoadPkgManifestsFromGitHub(releaseResult.Releases);

            if (manifests.Exception != null) {
                progress?.Report($"{Strings.GameServices.ModulesService.PkgManagement_Progress_FailedToReadOrParseRepoManifest}\r\n{manifests.Exception.Message}");
                return null;    
            }

            return manifests.PkgManifests;
        }

        private async Task<(GitHubRelease[] Releases, Exception Exception)> UpdateGitHubReleases() {
            try {
                return (await $"{this.PkgUrl}{GITHUB_RELEASES_URI}".WithHeader("User-Agent", "Blish-HUD").GetJsonAsync<GitHubRelease[]>(), null);
            } catch (FlurlHttpException ex) {
                return (new GitHubRelease[0], ex);
            }
        }

        private async Task<(PkgManifest[] PkgManifests, Exception Exception)> LoadPkgManifestsFromGitHub(IEnumerable<GitHubRelease> releases) {
            (PkgManifest[] PkgManifests, Exception Exception) lastReleaseSet = (Array.Empty<PkgManifest>(), null);

            foreach (var release in releases) {
                string compressedReleaseUrl = release.Assets.First(asset => asset.Name.Equals(ASSET_PACKAGE_NAME, StringComparison.InvariantCultureIgnoreCase)).BrowserDownloadUrl;

                lastReleaseSet = await LoadPkgManifests(compressedReleaseUrl);

                if (lastReleaseSet.Exception == null) break;
            }

            return lastReleaseSet;
        }
        
    }
}
