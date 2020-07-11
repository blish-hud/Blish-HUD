using System;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ManageModulePresenter : Presenter<ManageModuleView, ModuleManager> {

        private static readonly Logger Logger = Logger.GetLogger<ManageModulePresenter>();

        public ManageModulePresenter(ManageModuleView view, ModuleManager model) : base(view, model) { /* NOOP */ }
        
        protected override void UpdateView() {
            DisplayBaseViews();

            InvalidateViewState(staticDetails: true);

            this.Model.ModuleEnabled  += ModelOnModuleEnabled;
            this.Model.ModuleDisabled += ModelOnModuleDisabled;

            this.View.EnableModuleClicked  += ViewOnEnableModuleClicked;
            this.View.DisableModuleClicked += ViewOnDisableModuleClicked;

            SubscribeToModuleRunState();
        }

        private void DisplayBaseViews() {
            this.View.SetPermissionsView(new ModulePermissionView(this.Model));
            this.View.SetDependenciesView(new ModuleDependencyView(this.Model));
        }

        private void InvalidateViewState(bool staticDetails = false, bool stateDetails = false, bool stateOptions = false) {
            if (staticDetails) {
                DisplayStaticDetails();
            }

            if (stateDetails) {
                DisplayStateDetails();
            }

            if (stateOptions) {
                DisplayStatedOptions();
            }
        }

        private void DisplayStaticDetails() {
            // Load static details based on the manifest

            this.View.ModuleName        = this.Model.Manifest.Name;
            this.View.ModuleNamespace   = this.Model.Manifest.Namespace;
            this.View.ModuleDescription = this.Model.Manifest.Description;
            this.View.ModuleVersion     = this.Model.Manifest.Version;

            this.View.AuthorImage = GetModuleAuthorImage();
            this.View.AuthorName  = GetModuleAuthor();
        }

        private void DisplayStateDetails() {
            this.View.ModuleState = Model.ModuleInstance?.RunState ?? ModuleRunState.Unloaded;

            GameService.Settings.Save();

            DisplaySettingsView(this.View.ModuleState == ModuleRunState.Loaded);
        }

        private void DisplaySettingsView(bool enable) {
            SettingsView toDisplay = null;

            if (enable) {
                toDisplay = new SettingsView(this.Model.State.Settings);
            }

            this.View.SetSettingsView(toDisplay);
        }

        private void DisplayStatedOptions() {
            this.View.CanEnable  = GetModuleCanEnable();
            this.View.CanDisable = GetModuleCanDisable();
        }

        private AsyncTexture2D GetModuleAuthorImage() {
            if (this.Model.Manifest.Contributors?.Count > 1) {
                return GameService.Content.GetTexture("common/157112");
            }

            return GameService.Content.GetTexture("common/733268");
        }

        private string GetModuleAuthor() {
            if (this.Model.Manifest.Contributors?.Count > 0) {
                return string.Join(", ", this.Model.Manifest.Contributors.Select(c => c.Name));
            }

            if (this.Model.Manifest.Author != null) {
                return this.Model.Manifest.Author.Name;
            }

            return Strings.Common.Unknown;
        }

        private void ViewOnEnableModuleClicked(object sender, EventArgs e) {
            this.Model.Enabled = true;
            SubscribeToModuleRunState();
        }

        private void ViewOnDisableModuleClicked(object sender, EventArgs e) {
            this.Model.Enabled = false;
        }

        private void SubscribeToModuleRunState() {
            if (this.Model.ModuleInstance != null) {
                this.Model.ModuleInstance.ModuleRunStateChanged += ModuleInstanceOnModuleRunStateChanged;
            }

            InvalidateViewState(stateDetails: true, stateOptions: true);
        }

        private void UnsubscribeFromModuleRunState() {
            if (this.Model.ModuleInstance != null) {
                this.Model.ModuleInstance.ModuleRunStateChanged -= ModuleInstanceOnModuleRunStateChanged;
            }
        }

        private void ModuleInstanceOnModuleRunStateChanged(object sender, ModuleRunStateChangedEventArgs e) {
            InvalidateViewState(stateDetails: true, stateOptions: true);
        }

        private void ModelOnModuleEnabled(object sender, EventArgs e) {
            //SubscribeToModuleRunState();
        }

        private void ModelOnModuleDisabled(object sender, EventArgs e) {
            InvalidateViewState(stateDetails: true, stateOptions: true);
        }

        private bool GetModuleCanEnable() {
            // Can't enable if already enabled
            if (this.Model.Enabled) return false;

            // Can't enable if there is an instance of the
            // module already while the module is unloading
            if (this.Model.ModuleInstance != null) return false;

            // Can't enable if the dependencies aren't met (unless
            // ignore module dependencies has been selected)
            if (!this.Model.DependenciesMet) return false;

            return true;
        }

        private bool GetModuleCanDisable() {
            // Can't disable if already disabled
            if (!this.Model.Enabled) return false;

            // Can't disable if the module is currently unloading
            if (this.Model.ModuleInstance == null) return false;

            // Can't disable if the module isn't currently marked as loaded
            if (this.Model.ModuleInstance.RunState != ModuleRunState.Loaded) return false;

            return true;
        }

        protected override void Unload() {
            UnsubscribeFromModuleRunState();
        }

    }
}
