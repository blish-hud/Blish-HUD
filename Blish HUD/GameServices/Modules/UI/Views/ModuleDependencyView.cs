using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Modules.UI.Controls;
using Blish_HUD.Modules.UI.Presenters;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleDependencyView : TitledDetailView {

        private readonly Dictionary<ModuleDependencyCheckResult, Color> _dependencyDisplayColor = new Dictionary<ModuleDependencyCheckResult, Color>() {
            { ModuleDependencyCheckResult.NotFound, Color.Red },
            { ModuleDependencyCheckResult.Available, Color.White },
            { ModuleDependencyCheckResult.AvailableNotEnabled, Color.Yellow },
            { ModuleDependencyCheckResult.AvailableWrongVersion, Color.Yellow },
            { ModuleDependencyCheckResult.FoundInRepo, Color.Blue },
        };

        private Menu                 _dependencyMenuList;
        private Label                _messageLabel;
        private ContextMenuStripItem _ignoreModuleDependenciesToggle;

        public ModuleDependencyView() { /* NOOP */ }

        public ModuleDependencyView(ModuleDependencyCheckDetails[] model) {
            this.WithPresenter(new ModuleDependencyPresenter(this, model ?? new ModuleDependencyCheckDetails[0]));
        }

        protected override void BuildDetailView(Panel buildPanel) {
            this.Title = Strings.GameServices.ModulesService.ModuleManagement_Dependencies;

            this.Menu = new ContextMenuStrip();
            _ignoreModuleDependenciesToggle = this.Menu.AddMenuItem(Strings.GameServices.ModulesService.ModuleManagement_IgnoreDependencyRequirements);
            _ignoreModuleDependenciesToggle.CanCheck = true;

            _dependencyMenuList = new Menu() {
                Size           = buildPanel.ContentRegion.Size,
                MenuItemHeight = 22,
                Visible        = false,
                Parent         = buildPanel,
            };

            _messageLabel = new Label() {
                Size                = buildPanel.ContentRegion.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text                = Strings.GameServices.ModulesService.Dependency_NoDependencies,
                StrokeText          = true,
                Font                = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic),
                Parent              = buildPanel
            };
        }

        public void SetDependencies(IEnumerable<(string Name, string Status, ModuleDependencyCheckResult Result)> dependencies) {
            _dependencyMenuList.ClearChildren();
            _dependencyMenuList.Hide();

            foreach ((string name, string status, var result) in dependencies) {
                _ = new StatusMenuItem() {
                    Text            = name,
                    StatusText      = status,
                    StatusTextColor = _dependencyDisplayColor[result],
                    Enabled         = false,
                    Parent          = _dependencyMenuList
                };
            }

            // Show "No dependencies" if there are none
            _messageLabel.Visible = !(_dependencyMenuList.Visible = _dependencyMenuList.Children.Count > 0);
        }

    }
}
