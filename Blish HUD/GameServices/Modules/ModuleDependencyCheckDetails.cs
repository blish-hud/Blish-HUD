using SemVer;

namespace Blish_HUD.Modules {
    public struct ModuleDependencyCheckDetails {

        public ModuleDependency Dependency { get; }

        public ModuleDependencyCheckResult CheckResult { get; }

        public ModuleManager Module { get; }

        public ModuleDependencyCheckDetails(ModuleDependency dependency, ModuleDependencyCheckResult checkResult, ModuleManager module = null) {
            this.Dependency  = dependency;
            this.CheckResult = checkResult;
            this.Module      = module;
        }

        public string GetDisplayName() {
            if (this.Dependency.IsBlishHud) return Strings.Common.BlishHUD;

            return this.Module?.Manifest.Name
                ?? this.Dependency.Namespace;
        }

    }
}
