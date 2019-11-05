using System.Linq;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleWebApiPromptPresenter : Presenter<ModuleWebApiPromptView, ModuleManager> {

        /// <inheritdoc />
        public ModuleWebApiPromptPresenter(ModuleWebApiPromptView view, ModuleManager model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.ModuleName      = GetModuleDisplayName();
            this.View.ModuleNamespace = GetModuleNamespace();
        }

        public string GetModuleDisplayName() {
            return this.Model.Manifest.Name;
        }

        public string GetModuleNamespace() {
            return this.Model.Manifest.Namespace;
        }

        public string GetModuleVersion() {
            return $"{this.Model.Manifest.Version}";
        }

        public string GetModuleAuthor() {
            if (this.Model.Manifest.Author != null) {
                return this.Model.Manifest.Author.Name;
            }

            if (this.Model.Manifest.Contributors.Count > 0) {
                return string.Join(", ", this.Model.Manifest.Contributors.Select(c => c.Name));
            }

            return Strings.GameServices.ModulesService.ModuleAuthor_Unknown;
        }

    }
}
