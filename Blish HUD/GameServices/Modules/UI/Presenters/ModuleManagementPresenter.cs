using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleManagementPresenter : Presenter<ModuleManagementView, ModuleManager> {

        private ModuleDependencyCheckDetails[] _dependencyResults;

        /// <inheritdoc />
        public ModuleManagementPresenter(ModuleManagementView view, ModuleManager model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            LoadDependencyResults();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.DependencyView.Show(new ModuleDependencyView(_dependencyResults));

            this.Model.ModuleEnabled  += ModelOnModuleEnabled;
            this.Model.ModuleDisabled += ModelOnModuleDisabled;

            this.View.EnableButton.Click  += EnableModuleButtonOnClick;
            this.View.DisableButton.Click += DisableModuleButtonOnClick;

            this.View.IgnoreDependencyRequirementsMenuStripItem.CheckedChanged += IgnoreDependencyRequirementsMenuStripItemOnCheckedChanged;

            SubscribeToModuleRunState();

            RefreshView();
        }

        private void LoadDependencyResults() {
            _dependencyResults = this.Model.Manifest.Dependencies.Select(d => d.GetDependencyDetails()).ToArray();
        }

        private void IgnoreDependencyRequirementsMenuStripItemOnCheckedChanged(object sender, CheckChangedEvent e) {
            SetModuleIgnoreDependencyRequirements(e.Checked);

            RefreshViewEnableDisableButton();
        }

        private void SubscribeToModuleRunState() {
            if (this.Model.ModuleInstance != null) {
                this.Model.ModuleInstance.ModuleRunStateChanged += ModuleInstanceOnModuleRunStateChanged;
            }
        }

        private void UnsubscribeFromModuleRunState() {
            if (this.Model.ModuleInstance != null) {
                this.Model.ModuleInstance.ModuleRunStateChanged -= ModuleInstanceOnModuleRunStateChanged;
            }
        }

        private void ModelOnModuleEnabled(object sender, EventArgs e) {
            SubscribeToModuleRunState();

            RefreshViewModuleState();
            RefreshViewEnableDisableButton();
        }

        private void ModelOnModuleDisabled(object sender, EventArgs e) {
            UnsubscribeFromModuleRunState();

            RefreshViewModuleState();
            RefreshViewEnableDisableButton();
        }

        private void ModuleInstanceOnModuleRunStateChanged(object sender, ModuleRunStateChangedEventArgs e) {
            RefreshViewModuleState();
            RefreshViewEnableDisableButton();
        }

        private void RefreshView() {
            this.View.ModuleName.Text = GetModuleName();
            this.View.ModuleVersion.Text = $"v{GetModuleVersion()}";

            RefreshViewModuleState();

            this.View.AuthorImage.Texture = GetModuleAuthorImage();
            this.View.AuthorName.Text = GetModuleAuthor();
            this.View.AuthorName.Invalidate();

            RefreshViewEnableDisableButton();

            this.View.DescriptionLabel.Text = GetModuleDescription();

            this.View.IgnoreDependencyRequirementsMenuStripItem.Checked = GetModuleIgnoreDependencyRequirements();
        }

        private void RefreshViewModuleState() {
            (string moduleLoadState, var moduleLoadStateColor) = GetModuleState();
            this.View.ModuleState.Text = moduleLoadState;
            this.View.ModuleState.TextColor = moduleLoadStateColor;
            this.View.ModuleState.Invalidate();
        }

        private void RefreshViewEnableDisableButton() {
            this.View.EnableButton.Enabled = GetModuleCanEnable();
            this.View.DisableButton.Enabled = GetModuleCanDisable();
        }

        /// <inheritdoc />
        protected override void Unload() {
            this.Model.ModuleEnabled  -= ModelOnModuleEnabled;
            this.Model.ModuleDisabled -= ModelOnModuleDisabled;

            this.View.EnableButton.Click  -= EnableModuleButtonOnClick;
            this.View.DisableButton.Click -= DisableModuleButtonOnClick;

            this.View.IgnoreDependencyRequirementsMenuStripItem.CheckedChanged -= IgnoreDependencyRequirementsMenuStripItemOnCheckedChanged;

            UnsubscribeFromModuleRunState();
        }

        private void EnableModuleButtonOnClick(object sender, MouseEventArgs e) {
            SetModuleEnabled();
        }

        private void DisableModuleButtonOnClick(object sender, MouseEventArgs e) {
            SetModuleDisabled();
        }

        public string GetModuleName() {
            return this.Model.Manifest.Name;
        }

        public string GetModuleVersion() {
            return $"{this.Model.Manifest.Version.ToString()}";
        }

        public (string, Color) GetModuleState() {
            if (!this.Model.Enabled || this.Model.ModuleInstance == null) return ("Disabled", Color.Red);

            switch (this.Model.ModuleInstance.RunState) {
                case ModuleRunState.Unloaded:
                    return ("Disabled", Control.StandardColors.DisabledText);
                    break;

                case ModuleRunState.Loading:
                    return ("Loading", Control.StandardColors.Yellow);
                    break;

                case ModuleRunState.Loaded:
                    return ("Enabled", Color.FromNonPremultiplied(0, 255, 25, 255));
                    break;

                case ModuleRunState.Unloading:
                    return ("Disabling", Control.StandardColors.Yellow);
                    break;

                case ModuleRunState.FatalError:
                    return ("Fatal Error", Control.StandardColors.Red);
                    break;
            }

            return ("Disabled", Color.Red);
        }

        public Texture2D GetModuleAuthorImage() {
            return GameService.Content.GetTexture("733268");
        }

        public string GetModuleAuthor() {
            if (this.Model.Manifest.Author != null) {
                return this.Model.Manifest.Author.Name;
            }

            if (this.Model.Manifest.Contributors.Count > 0) {
                return string.Join(", ", this.Model.Manifest.Contributors.Select(c => c.Name));
            }

            return "Unknown";
        }

        public bool GetModuleCanEnable() {
            if (this.Model.Enabled) return false;
            if (this.Model.ModuleInstance != null) return false;
            if (!SatisfiesDependencies()) return false;

            return true;
        }

        public bool GetModuleCanDisable() {
            if (!this.Model.Enabled) return false;
            if (this.Model.ModuleInstance == null) return false;
            if (this.Model.ModuleInstance.RunState != ModuleRunState.Loaded) return false;

            return true;
        }

        public string GetModuleDescription() {
            return this.Model.Manifest.Description;
        }

        public bool GetModuleIgnoreDependencyRequirements() {
            return this.Model.State.IgnoreDependencies;
        }

        public ModuleDependencyCheckDetails[] GetModuleDependencyDetails() {
            return _dependencyResults;
        }

        private bool SatisfiesDependencies() {
            if (this.Model.State.IgnoreDependencies) return true;

            for (int i = 0; i < _dependencyResults.Length; i++) {
                if (_dependencyResults[i].CheckResult != ModuleDependencyCheckResult.Available) {
                    return false;
                }
            }

            return true;
        }

        public void SetModuleEnabled() {
            this.Model.Enabled = true;
        }

        public void SetModuleDisabled() {
            this.Model.Enabled = false;
        }

        public void SetModuleIgnoreDependencyRequirements(bool ignoreDependencyRequirements) {
            this.Model.State.IgnoreDependencies = ignoreDependencyRequirements;
        }

    }
}
