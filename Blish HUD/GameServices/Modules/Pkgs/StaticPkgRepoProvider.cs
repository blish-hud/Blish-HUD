using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;

namespace Blish_HUD.Modules.Pkgs {
    public class StaticPkgRepoProvider : IPkgRepoProvider {

        private static readonly Logger Logger = Logger.GetLogger<StaticPkgRepoProvider>();
        
        private const string ASSET_PACKAGE_NAME  = "/packages.gz";

        private static readonly Dictionary<string, PkgManifest[]> _pkgCache = new Dictionary<string, PkgManifest[]>();

        private readonly List<Func<PkgManifest, bool>> _activeFilters = new List<Func<PkgManifest, bool>> { FilterShowOnlySupportedVersion };

        public virtual string PkgUrl { get; }

        protected StaticPkgRepoProvider() { /* NOOP */ }

        public StaticPkgRepoProvider(string pkgUrl) {
            this.PkgUrl = pkgUrl;
        }

        public async Task<bool> Load(IProgress<string> progress) {
            if (!_pkgCache.ContainsKey(this.PkgUrl)) {
                _pkgCache[this.PkgUrl] = await LoadRepo(progress);
            }

            return true;
        }

        protected virtual async Task<PkgManifest[]> LoadRepo(IProgress<string> progress = null) {
            progress?.Report(Strings.GameServices.ModulesService.PkgManagement_Progress_GettingModuleList);
            var manifests = await LoadPkgManifests(Flurl.Url.Combine(this.PkgUrl, ASSET_PACKAGE_NAME));

            if (manifests.Exception != null) {
                progress?.Report($"{Strings.GameServices.ModulesService.PkgManagement_Progress_FailedToReadOrParseRepoManifest}\r\n{manifests.Exception.Message}");
            }

            return manifests.PkgManifests;
        }

        protected async Task<(PkgManifest[] PkgManifests, Exception Exception)> LoadPkgManifests(string pkgUrl) {
            try {
                using var compressedRelease = await pkgUrl.GetStreamAsync();

                using var gzipStream = new GZipStream(compressedRelease, CompressionMode.Decompress);
                using var streamReader = new StreamReader(gzipStream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                var serializer = new JsonSerializer();

                return (serializer.Deserialize<PkgManifest[]>(jsonTextReader), null);
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed to load modules from '{pkgUrl}'.");
                return (Array.Empty<PkgManifest>(), ex);
            }
        }

        public virtual IEnumerable<PkgManifest> GetPkgManifests() {
            return _pkgCache[this.PkgUrl].Where(pkg => _activeFilters.All(filter => filter(pkg)));
        }

        public virtual IEnumerable<(string OptionName, Action<bool> OptionAction, bool IsToggle, bool IsChecked)> GetExtraOptions() {
            // Actions
            yield return (Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_ProviderExtraOption_ReloadRepository, async (toggleState) => { await LoadRepo(); }, false, false);

            // Filters
            yield return (Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_ProviderExtraOption_FilterSupportedVersions, (toggleState) => ToggleFilter(FilterShowOnlySupportedVersion, toggleState), true, _activeFilters.Contains(FilterShowOnlySupportedVersion));
            yield return (Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_ProviderExtraOption_FilterModulesWithUpdates, (toggleState) => ToggleFilter(FilterShowOnlyUpdates,         toggleState), true, _activeFilters.Contains(FilterShowOnlyUpdates));
            yield return (Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_ProviderExtraOption_FilterInstalledModules, (toggleState) => ToggleFilter(FilterShowOnlyInstalled,         toggleState), true, _activeFilters.Contains(FilterShowOnlyInstalled));
            yield return (Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_ProviderExtraOption_FilterNotInstalledModules, (toggleState) => ToggleFilter(FilterShowOnlyNotInstalled,   toggleState), true, _activeFilters.Contains(FilterShowOnlyNotInstalled));
        }

        protected void ToggleFilter(Func<PkgManifest, bool> filterFunc, bool state) {
            if (state) {
                _activeFilters.Add(filterFunc);
            } else {
                _activeFilters.Remove(filterFunc);
            }
        }

        protected static bool FilterShowOnlySupportedVersion(PkgManifest pkgManifest) {
            var blishHudDependency = pkgManifest.Dependencies.Find(d => d.IsBlishHud);

            return blishHudDependency                                    != null
                && blishHudDependency.GetDependencyDetails().CheckResult == ModuleDependencyCheckResult.Available;
        }

        protected static bool FilterShowOnlyUpdates(PkgManifest pkgManifest) {
            return GameService.Module.Modules.Any(m =>
                                                      string.Equals(m.Manifest.Namespace, pkgManifest.Namespace, StringComparison.OrdinalIgnoreCase)
                                                   && m.Manifest.Version < pkgManifest.Version);
        }

        protected static bool FilterShowOnlyInstalled(PkgManifest pkgManifest) {
            return GameService.Module.Modules.Any(m => string.Equals(m.Manifest.Namespace, pkgManifest.Namespace, StringComparison.OrdinalIgnoreCase));
        }

        protected static bool FilterShowOnlyNotInstalled(PkgManifest pkgManifest) {
            return !FilterShowOnlyInstalled(pkgManifest);
        }

    }
}
