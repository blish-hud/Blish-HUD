using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleDependencyPresenter : Presenter<ModuleDependencyView, ModuleDependencyCheckDetails[]> {

        public ModuleDependencyPresenter(ModuleDependencyView view, ModuleDependencyCheckDetails[] model) : base(view, model) { /* NOOP */ }

        protected override void UpdateView() {
            UpdateDependencyList();
            UpdateStatus();
        }

        private void UpdateDependencyList() {
            var checkResults = new List<(string Name, string Status, ModuleDependencyCheckResult Result)>();

            foreach (var dependencyCheck in this.Model) {
                string requiredRange = string.Format(Strings.GameServices.ModulesService.Dependency_RequiresVersion, dependencyCheck.Dependency.VersionRange.ToString());

                string actualVersion = dependencyCheck.Module == null
                                           ? Program.OverlayVersion.BaseVersion().ToString()
                                           : dependencyCheck.Module.Manifest.Version.BaseVersion().ToString();

                string status = dependencyCheck.CheckResult switch {
                    ModuleDependencyCheckResult.NotFound => $"{Strings.GameServices.ModulesService.Dependency_NotFound} {requiredRange}",
                    ModuleDependencyCheckResult.Available => $"v{actualVersion}",
                    ModuleDependencyCheckResult.AvailableNotEnabled => $"{Strings.GameServices.ModulesService.Dependency_NotEnabled} {requiredRange}",
                    ModuleDependencyCheckResult.AvailableWrongVersion => $"{Strings.GameServices.ModulesService.Dependency_WrongVersion} {requiredRange}",
                    ModuleDependencyCheckResult.FoundInRepo => $"[Found In Repo (Not Implemented)] v{requiredRange}",
                    _ => ""
                };

                checkResults.Add((dependencyCheck.GetDisplayName(), status, dependencyCheck.CheckResult));
            }

            this.View.SetDependencies(checkResults);
        }

        private void UpdateStatus() {
            string[] unmet = this.Model
                                 .Where(d => d.CheckResult != ModuleDependencyCheckResult.Available)
                                 .Select(d => d.GetDisplayName()).ToArray();

            if (unmet.Any()) {
                this.View.SetWarning(Strings.GameServices.ModulesService.Dependency_MissingDependencies
                                   + "\n\n"
                                   + string.Join("\n- ", unmet));
            } else {
                this.View.ClearWarning();
            }
        }

    }
}
