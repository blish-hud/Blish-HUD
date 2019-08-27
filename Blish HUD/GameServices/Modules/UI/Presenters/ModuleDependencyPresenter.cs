using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Controls;
using Blish_HUD.Modules.UI.Views;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleDependencyPresenter : Presenter<ModuleDependencyView, ModuleDependencyCheckDetails[]> {

        /// <inheritdoc />
        public ModuleDependencyPresenter(ModuleDependencyView view, ModuleDependencyCheckDetails[] model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override void UpdateView() {
            bool dependencyRequirementsMet = true;

            foreach (var dependencyCheck in this.Model) {
                if (dependencyCheck.CheckResult != ModuleDependencyCheckResult.Available) {
                    dependencyRequirementsMet = false;
                }

                var dependencyMenuItem = new StatusMenuItem() {
                    Text = dependencyCheck.Name,
                    Enabled = false,
                    //BasicTooltipText = $"{dependency.Namespace} in range {dependency.VersionRange}"
                };

                string dependencyVersion = dependencyCheck.Module == null ? Program.OverlayVersion.BaseVersion().ToString() : dependencyCheck.Module.Manifest.Version.BaseVersion().ToString();

                switch (dependencyCheck.CheckResult) {
                    case ModuleDependencyCheckResult.NotFound:
                        dependencyMenuItem.StatusText      = $"[Not Found] v{dependencyVersion}";
                        dependencyMenuItem.StatusTextColor = Color.Red;
                        break;

                    case ModuleDependencyCheckResult.Available:
                        dependencyMenuItem.StatusText      = $"v{dependencyVersion}";
                        dependencyMenuItem.StatusTextColor = Color.Green;
                        break;

                    case ModuleDependencyCheckResult.AvailableNotEnabled:
                        dependencyMenuItem.StatusText      = $"[Not Enabled] v{dependencyVersion}";
                        dependencyMenuItem.StatusTextColor = Color.Yellow;
                        break;

                    case ModuleDependencyCheckResult.AvailableWrongVersion:
                        dependencyMenuItem.StatusText      = $"[Wrong Version] v{dependencyVersion}";
                        dependencyMenuItem.StatusTextColor = Color.Yellow;
                        break;

                    case ModuleDependencyCheckResult.FoundInRepo:
                        dependencyMenuItem.StatusText      = $"[Not Implemented] v{dependencyVersion}";
                        dependencyMenuItem.StatusTextColor = Color.Blue;
                        break;
                }

                dependencyMenuItem.Parent = this.View.DependencyMenuList;
            }
        }

    }
}

