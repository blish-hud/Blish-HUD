using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleDependencyPresenter : Presenter<ModuleDependencyView, ModuleManager> {

        private ModuleDependencyCheckDetails[] _moduleDependencyDetails;

        public ModuleDependencyPresenter(ModuleDependencyView view, ModuleManager model) : base(view, model) { /* NOOP */ }

        protected override Task<bool> Load(IProgress<string> progress) {
            _moduleDependencyDetails = this.Model.Manifest.Dependencies.Select(d => d.GetDependencyDetails()).ToArray();

            this.View.IgnoreModuleDependenciesChanged += ViewOnIgnoreModuleDependenciesChanged;

            return base.Load(progress);
        }

        private void ViewOnIgnoreModuleDependenciesChanged(object sender, ValueEventArgs<bool> e) {
            this.Model.State.IgnoreDependencies = e.Value;

            UpdateStatus();
        }

        protected override void UpdateView() {
            UpdateSettings();
            UpdateDependencyList();
            UpdateStatus();
        }

        private void UpdateSettings() {
            this.View.IgnoreModuleDependencies = this.Model.State.IgnoreDependencies;
        }

        private void UpdateDependencyList() {
            var checkResults = new List<(string Name, string Status, ModuleDependencyCheckResult Result)>();

            foreach (var dependencyCheck in _moduleDependencyDetails) {
                string requiredRange = string.Format(Strings.GameServices.ModulesService.Dependency_RequiresVersion, dependencyCheck.Dependency.VersionRange);

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
            string[] unmet = _moduleDependencyDetails
                            .Where(d => d.CheckResult != ModuleDependencyCheckResult.Available)
                            .Select(d => d.GetDisplayName())
                            .ToArray();

            if (unmet.Any()) {
                this.View.SetDetails(Strings.GameServices.ModulesService.Dependency_MissingDependencies
                                   + "\n\n"
                                   + string.Join("\n- ", unmet),
                                     this.Model.State.IgnoreDependencies
                                     ? TitledDetailView.DetailLevel.Info
                                     : TitledDetailView.DetailLevel.Warning);
            } else {
                this.View.ClearDetails();
            }
        }

        protected override void Unload() {
            this.View.IgnoreModuleDependenciesChanged -= ViewOnIgnoreModuleDependenciesChanged;
        }

    }
}
