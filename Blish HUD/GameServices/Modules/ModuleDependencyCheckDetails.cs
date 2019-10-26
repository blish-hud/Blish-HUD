namespace Blish_HUD.Modules {
    public struct ModuleDependencyCheckDetails {

        public string Name { get; }

        public ModuleDependencyCheckResult CheckResult { get; }

        public ModuleManager Module { get; }

        public ModuleDependencyCheckDetails(string name, ModuleDependencyCheckResult checkResult, ModuleManager module) {
            this.Name        = name;
            this.CheckResult = checkResult;
            this.Module      = module;
        }

        public ModuleDependencyCheckDetails(string name, ModuleDependencyCheckResult checkResult) : this(name, checkResult, null) { /* NOOP */ }

    }
}
