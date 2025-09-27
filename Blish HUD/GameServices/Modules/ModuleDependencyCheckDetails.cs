using SemVer;

namespace Blish_HUD.Modules {
    public readonly struct ModuleDependencyCheckDetails {

        public ModuleDependency Dependency { get; }

        public ModuleDependencyCheckResult CheckResult { get; }

        public ModuleManager Module { get; }

        public ModuleDependencyCheckDetails(ModuleDependency dependency, ModuleDependencyCheckResult checkResult, ModuleManager module = null) {
            this.Dependency = dependency;
            this.CheckResult = checkResult;
            this.Module = module;
        }

        public readonly string GetDisplayName() {
            return this.Dependency.IsBlishHud
                ? Strings.Common.BlishHUD
                : this.Module?.Manifest.Name
                ?? this.Dependency.Namespace;
        }
    }
}
