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
            var checkResults = new List<(string Name, string Status, Color StatusColor)>();

            foreach (var dependencyCheck in this.Model) {
                string dependencyVersion = dependencyCheck.Module == null
                                               ? Program.OverlayVersion.BaseVersion().ToString()
                                               : dependencyCheck.Module.Manifest.Version.BaseVersion().ToString();

                switch (dependencyCheck.CheckResult) {
                    case ModuleDependencyCheckResult.NotFound:
                        checkResults.Add((dependencyCheck.Name,
                                          $"{Strings.GameServices.ModulesService.Dependency_NotFound} v{dependencyVersion}",
                                          Color.Red));
                        break;
                    case ModuleDependencyCheckResult.Available:
                        checkResults.Add((dependencyCheck.Name,
                                          $"v{dependencyVersion}",
                                          Color.Green));
                        break;
                    case ModuleDependencyCheckResult.AvailableNotEnabled:
                        checkResults.Add((dependencyCheck.Name,
                                          $"{Strings.GameServices.ModulesService.Dependency_NotEnabled} v{dependencyVersion}",
                                          Color.Yellow));
                        break;
                    case ModuleDependencyCheckResult.AvailableWrongVersion:
                        checkResults.Add((dependencyCheck.Name,
                                          $"{Strings.GameServices.ModulesService.Dependency_WrongVersion} v{dependencyVersion}",
                                          Color.Yellow));
                        break;
                    case ModuleDependencyCheckResult.FoundInRepo:
                        checkResults.Add((dependencyCheck.Name,
                                          $"[Found In Repo (Not Implemented)] v{dependencyVersion}",
                                          Color.Blue));
                        break;
                }
            }

            this.View.SetDependencies(checkResults);
        }

        private void UpdateStatus() {
            string[] unmet = this.Model
                                 .Where(d => d.CheckResult != ModuleDependencyCheckResult.Available)
                                 .Select(d => d.Name).ToArray();

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
