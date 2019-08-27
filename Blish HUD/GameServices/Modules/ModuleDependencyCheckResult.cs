namespace Blish_HUD.Modules {
    public enum ModuleDependencyCheckResult {
        NotFound,
        Available,
        AvailableNotEnabled,
        AvailableWrongVersion,
        FoundInRepo
    }
}
