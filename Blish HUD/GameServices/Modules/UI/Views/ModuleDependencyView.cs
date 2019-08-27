using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Presenters;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleDependencyView : View<ModuleDependencyPresenter> {

        private Menu _dependencyMenuList;

        public Menu DependencyMenuList => _dependencyMenuList;

        public ModuleDependencyView(ModuleDependencyCheckDetails[] module) {
            this.Presenter = new ModuleDependencyPresenter(this, module);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            _dependencyMenuList = new Menu() {
                Size           = buildPanel.Size,
                MenuItemHeight = 22,
                Parent         = buildPanel
            };
        }

    }
}
