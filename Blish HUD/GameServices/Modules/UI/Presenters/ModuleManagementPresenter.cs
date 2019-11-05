using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Modules.UI.Views;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Overlay.UI.Presenters;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ModuleManagementPresenter : Presenter<ModuleManagementView, ModuleManager> {

        private readonly Logger Logger = Logger.GetLogger(typeof(ModuleManagementPresenter));

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
            this.Model.ModuleEnabled  += ModelOnModuleEnabled;
            this.Model.ModuleDisabled += ModelOnModuleDisabled;

            this.View.EnableButton.Click  += EnableModuleButtonOnClick;
            this.View.DisableButton.Click += DisableModuleButtonOnClick;

            this.View.ClearPermissionsClicked  += ClearPermissionsClicked;
            this.View.ModifyPermissionsClicked += ModifyPermissionsClicked;

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
            RefreshViewWarnings();
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
            RefreshWebApiPermissionsView();
            RefreshDependencyView();
            RefreshModuleSettingsView();

            this.View.ModuleName    = GetModuleName();
            this.View.ModuleVersion = $"v{GetModuleVersion()}";

            RefreshViewModuleState();

            this.View.AuthorImage = GetModuleAuthorImage();
            this.View.AuthorName  = GetModuleAuthor();

            RefreshViewEnableDisableButton();

            RefreshViewWarnings();

            this.View.ModuleDescription = GetModuleDescription();

            this.View.IgnoreDependencyRequirementsMenuStripItem.Checked = GetModuleIgnoreDependencyRequirements();
        }

        private void RefreshWebApiPermissionsView() {
            this.View.PermissionView.Show(new ModuleWebApiPermissionsView(this.Model));
        }

        private void RefreshDependencyView() {
            this.View.DependencyView.Show(new ModuleDependencyView(_dependencyResults));
        }

        private void RefreshModuleSettingsView() {
            if (this.Model.Enabled) {
                var moduleSettingsView = new RepeatedView<IEnumerable<ApplicationSettingsPresenter.SettingsCategory>>();

                moduleSettingsView.Presenter = new ApplicationSettingsPresenter(moduleSettingsView,
                                                                                new List<ApplicationSettingsPresenter.SettingsCategory>() {
                                                                                    new ApplicationSettingsPresenter.SettingsCategory("Module Settings", this.Model.State.Settings)
                                                                                });

                this.View.SettingsView.Show(moduleSettingsView);
            } else {
                this.View.SettingsView.Clear();
            }
        }

        private void RefreshViewModuleState() {
            (string moduleLoadState, var moduleLoadStateColor) = GetModuleState();
            this.View.ModuleStateText  = moduleLoadState;
            this.View.ModuleStateColor = moduleLoadStateColor;
        }

        private void RefreshViewEnableDisableButton() {
            this.View.CanEnable  = GetModuleCanEnable();
            this.View.CanDisable = GetModuleCanDisable();
        }

        private void RefreshViewWarnings() {
            string permissionWarning = null;
            string dependencyWarning = null;

            // Permission warning
            TokenPermission[] missingPermissions = GetMissingApiPermissions();

            if (missingPermissions.Length > 0) {
                string permissionList = string.Join("\n", missingPermissions);
                permissionWarning = $"{Strings.GameServices.ModulesService.ApiPermission_MissingRequiredPermissions}\n\n{permissionList}";
            }

            // Dependency warning
            ModuleDependencyCheckDetails[] unmetDependencies = GetUnsatisfiedDependencies();

            if (unmetDependencies.Length > 0) {
                var dependencyWarningMessage = new StringBuilder();

                foreach (var dependency in unmetDependencies) {
                    dependencyWarningMessage.AppendLine($"{dependency.Name} {dependency.CheckResult}");
                }

                dependencyWarning = $"{Strings.GameServices.ModulesService.Dependency_MissingDependencies}\n\n{dependencyWarningMessage}";
            }

            // Update view
            this.View.PermissionWarning = permissionWarning?.Trim();
            this.View.DependencyWarning = dependencyWarning?.Trim();
        }

        /// <inheritdoc />
        protected override void Unload() {
            this.Model.ModuleEnabled  -= ModelOnModuleEnabled;
            this.Model.ModuleDisabled -= ModelOnModuleDisabled;

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
            return $"{this.Model.Manifest.Version}";
        }

        public (string, Color) GetModuleState() {
            if (!this.Model.Enabled || this.Model.ModuleInstance == null) return (Strings.GameServices.ModulesService.ModuleState_Disabled, Color.Red);

            switch (this.Model.ModuleInstance.RunState) {
                case ModuleRunState.Unloaded:
                    return (Strings.GameServices.ModulesService.ModuleState_Disabled, Control.StandardColors.DisabledText);
                    break;

                case ModuleRunState.Loading:
                    return (Strings.GameServices.ModulesService.ModuleState_Loading, Control.StandardColors.Yellow);
                    break;

                case ModuleRunState.Loaded:
                    return (Strings.GameServices.ModulesService.ModuleState_Enabled, Color.FromNonPremultiplied(0, 255, 25, 255));
                    break;

                case ModuleRunState.Unloading:
                    return (Strings.GameServices.ModulesService.ModuleState_Disabling, Control.StandardColors.Yellow);
                    break;

                case ModuleRunState.FatalError:
                    return (Strings.GameServices.ModulesService.ModuleState_FatalError, Control.StandardColors.Red);
                    break;
            }

            return ("Disabled", Color.Red);
        }

        private Texture2D GetModuleAuthorImage() {
            return GameService.Content.GetTexture("733268");
        }

        private string GetModuleAuthor() {
            if (this.Model.Manifest.Author != null) {
                return this.Model.Manifest.Author.Name;
            }

            if (this.Model.Manifest.Contributors.Count > 0) {
                return string.Join(", ", this.Model.Manifest.Contributors.Select(c => c.Name));
            }

            return Strings.GameServices.ModulesService.ModuleAuthor_Unknown;
        }

        private bool GetModuleCanEnable() {
            if (this.Model.Enabled) return false;
            if (this.Model.ModuleInstance != null) return false;
            if (GetUnsatisfiedDependencies().Length > 0) return false;

            return true;
        }

        private bool GetModuleCanDisable() {
            if (!this.Model.Enabled) return false;
            if (this.Model.ModuleInstance == null) return false;
            if (this.Model.ModuleInstance.RunState != ModuleRunState.Loaded) return false;

            return true;
        }

        private string GetModuleDescription() {
            return this.Model.Manifest.Description;
        }

        private bool GetModuleIgnoreDependencyRequirements() {
            return this.Model.State.IgnoreDependencies;
        }

        public ModuleDependencyCheckDetails[] GetModuleDependencyDetails() {
            return _dependencyResults;
        }

        private ModuleDependencyCheckDetails[] GetUnsatisfiedDependencies() {
            return this.Model.State.IgnoreDependencies
                       ? new ModuleDependencyCheckDetails[0]
                       : _dependencyResults.Where(t => t.CheckResult != ModuleDependencyCheckResult.Available)
                                           .ToArray();
        }

        private TokenPermission[] GetMissingApiPermissions() {
            return this.Model.Manifest.ApiPermissions
                       .Where(p => !p.Value.Optional)
                       .Select(p => p.Key)
                       .Except(this.Model.State.UserEnabledPermissions ?? new TokenPermission[0])
                       .ToArray();
        }

        private void SetModuleEnabled() {
            // Give the user a chance to consent to requested API permissions
            if (GetMissingApiPermissions().Length > 0) {
                ShowWebApiPermissionPrompt(true);
                return;
            }

            this.Model.Enabled = true;
        }

        private void ModifyPermissionsClicked(object sender, EventArgs e) {
            ShowWebApiPermissionPrompt();
        }

        private void ClearPermissionsClicked(object sender, EventArgs e) {
            this.Model.State.UserEnabledPermissions = new TokenPermission[0];
            RefreshWebApiPermissionsView();
        }

        private void ShowWebApiPermissionPrompt(bool enableIfAccepted = false) {
            var newPermissionPrompt = new TintedScreenView<ModuleWebApiPromptView.ApiPromptResult>(new ModuleWebApiPromptView(this.Model));

            var activeScreenViewContainer = new ViewContainer() {
                Size   = GameService.Graphics.SpriteScreen.Size,
                Parent = GameService.Graphics.SpriteScreen,
            };

            activeScreenViewContainer.Show(newPermissionPrompt);

            newPermissionPrompt.ReturnWith((value) => {
                if (value.Accepted) {
                    Logger.Info("User consented to API permissions ({permissionList}) requested by {moduleNamespace}.", string.Join(", ", value.ConsentedPermissions), this.Model.Manifest.Namespace);

                    this.Model.State.UserEnabledPermissions = value.ConsentedPermissions;

                    if (enableIfAccepted) {
                        SetModuleEnabled();
                    }

                    RefreshWebApiPermissionsView();
                } else {
                    Logger.Info("User canceled API permissions prompt requested by {moduleNamespace}.", this.Model.Manifest.Namespace);
                }

                activeScreenViewContainer.Dispose();
            });
        }

        private void SetModuleDisabled() {
            this.Model.Enabled = false;
        }

        private void SetModuleIgnoreDependencyRequirements(bool ignoreDependencyRequirements) {
            this.Model.State.IgnoreDependencies = ignoreDependencyRequirements;
        }

    }
}
