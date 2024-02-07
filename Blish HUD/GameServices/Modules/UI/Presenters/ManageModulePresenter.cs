﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Views;
using Humanizer;

namespace Blish_HUD.Modules.UI.Presenters {
    public class ManageModulePresenter : Presenter<ManageModuleView, ModuleManager> {

        private static readonly Logger Logger = Logger.GetLogger<ManageModulePresenter>();

        public ManageModulePresenter(ManageModuleView view, ModuleManager model) : base(view, model) { /* NOOP */ }

        private ModulePermissionView _permissionView;
        private ModuleDependencyView _dependencyView;

        protected override void UpdateView() {
            DisplayBaseViews();

            InvalidateViewState(staticDetails: true);

            this.Model.ModuleDisabled += ModelOnModuleDisabled;

            this.View.EnableModuleClicked += ViewOnEnableModuleClicked;
            this.View.DisableModuleClicked += ViewOnDisableModuleClicked;

            SubscribeToModuleRunState();
        }

        private void DisplayBaseViews() {
            UpdatePermissionView();
            UpdateDependenciesView();
        }

        private void UpdatePermissionView() {
            this.View.SetPermissionsView(_permissionView = new ModulePermissionView(this.Model));
        }

        private void UpdateDependenciesView() {
            if (_dependencyView != null) {
                _dependencyView.IgnoreModuleDependenciesChanged -= DependencyViewOnIgnoreModuleDependenciesChanged;
            }

            this.View.SetDependenciesView(_dependencyView = new ModuleDependencyView(this.Model));

            _dependencyView.IgnoreModuleDependenciesChanged += DependencyViewOnIgnoreModuleDependenciesChanged;
        }

        private void DependencyViewOnIgnoreModuleDependenciesChanged(object sender, ValueEventArgs<bool> e) {
            InvalidateViewState(stateOptions: true);
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
                DisplaySettingMenu();
            }
        }

        private void DisplayStaticDetails() {
            // Load static details based on the manifest

            this.View.ModuleName = this.Model.Manifest.Name;
            this.View.ModuleNamespace = this.Model.Manifest.Namespace;
            this.View.ModuleDescription = this.Model.Manifest.Description;
            this.View.ModuleVersion = this.Model.Manifest.Version;
            this.View.ModuleAssemblyStateDirtied = this.Model.IsModuleAssemblyStateDirty;

            this.View.AuthorImage = GetModuleAuthorImage();
            this.View.AuthorName = GetModuleAuthor();
        }

        private void DisplaySettingMenu() {
            var settingMenu = new ContextMenuStrip();

            settingMenu.AddMenuItem(BuildClearSettingsMenuItem());
            settingMenu.AddMenuItems(BuildOpenDirsMenuItem());
            settingMenu.AddMenuItem(BuildDeleteModuleMenuItem());

            this.View.SettingMenu = settingMenu;
        }

        private ContextMenuStripItem BuildDeleteModuleMenuItem() {
            var deleteModule = new ContextMenuStripItem() { Text = Strings.GameServices.ModulesService.ModuleOption_DeleteModule };

            deleteModule.Click += delegate {
                this.Model.DeleteModule();
            };

            return deleteModule;
        }

        private ContextMenuStripItem BuildClearSettingsMenuItem() {
            var clearSettings = new ContextMenuStripItem() { Text = Strings.GameServices.ModulesService.ModuleOption_ClearSettings };

            clearSettings.BasicTooltipText = (clearSettings.Enabled = !this.Model.Enabled) == true
                                                 ? Strings.GameServices.ModulesService.ModuleOption_ClearSettings_DescriptionEnabled
                                                 : Strings.GameServices.ModulesService.ModuleOption_ClearSettings_DescriptionDisabled;

            clearSettings.Click += delegate { 
                this.Model.State.Settings = null;
                foreach (var directoryName in this.Model.Manifest.Directories) {
                    var dirPath = Path.Combine(DirectoryUtil.BasePath, directoryName);
                    if (Directory.Exists(dirPath)) {
                        Directory.Delete(dirPath, true);
                    }
                }
            };

            return clearSettings;
        }

        private IEnumerable<ContextMenuStripItem> BuildOpenDirsMenuItem() {
            var dirs = this.Model.Manifest.Directories ?? new List<string>(0);

            foreach (string dir in dirs) {
                var dirItem = new ContextMenuStripItem() { Text = string.Format(Strings.GameServices.ModulesService.ModuleOption_OpenDir, dir.Titleize()) };
                string dirPath = DirectoryUtil.RegisterDirectory(dir);

                dirItem.BasicTooltipText = dirPath;
                dirItem.Enabled = Directory.Exists(dirPath);

                dirItem.Click += delegate {
                    Process.Start("explorer.exe", $"/open, \"{dirPath}\\\"");
                };

                yield return dirItem;
            }
        }

        private void DisplayStateDetails() {
            if (!GameService.Module.ModuleIsExplicitlyIncompatible(this.Model)) {
                var runState = Model.ModuleInstance?.RunState ?? ModuleRunState.Unloaded;
                this.View.ModuleErrorReason = runState == ModuleRunState.FatalError ? this.Model.ModuleInstance?.ErrorReason : null;

                this.View.ModuleState = runState;

                GameService.Settings.Save();
            } else {
                this.View.SetCustomState(Strings.GameServices.ModulesService.ModuleState_Custom_ExplicitlyIncompatible, Control.StandardColors.Yellow);
            }

            DisplaySettingsView(this.View.ModuleState == ModuleRunState.Loaded);
        }

        private void DisplaySettingsView(bool enable) {
            IView toDisplay = null;

            if (enable) {
                try {
                    toDisplay = this.Model.ModuleInstance.GetSettingsView();
                } catch (Exception ex) {
                    Logger.Warn(ex, $"Failed to load settings view from module '{this.Model.Manifest.GetDetailedName()}'.");
                }
            }

            this.View.SetSettingsView(toDisplay);
        }

        private void DisplayStatedOptions() {
            this.View.CanEnable = GetModuleCanEnable();
            this.View.CanDisable = GetModuleCanDisable();
        }

        private AsyncTexture2D GetModuleAuthorImage() {
            if (this.Model.Manifest.Contributors?.Count > 1) {
                return AsyncTexture2D.FromAssetId(157112);
            }

            return AsyncTexture2D.FromAssetId(733268);
        }

        private string GetModuleAuthor() {
            if (this.Model.Manifest.Contributors?.Count > 0) {
                return string.Join(", ", this.Model.Manifest.Contributors.Select(c => c.Name));
            }

            return this.Model.Manifest.Author?.Name ?? Strings.Common.Unknown;
        }

        private void ViewOnEnableModuleClicked(object sender, EventArgs e) {
            if (this.Model.TryEnable()) {
                SubscribeToModuleRunState();
            }
        }

        private void ViewOnDisableModuleClicked(object sender, EventArgs e) {
            this.Model.Disable();
        }

        private void SubscribeToModuleRunState() {
            if (this.Model.ModuleInstance != null) {
                // Avoid subscribing more than once
                this.Model.ModuleInstance.ModuleRunStateChanged -= ModuleInstanceOnModuleRunStateChanged;
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

        private void ModelOnModuleDisabled(object sender, EventArgs e) {
            InvalidateViewState(stateDetails: true, stateOptions: true);
        }

        private bool GetModuleCanEnable() {
            // Can't enable if already enabled
            if (this.Model.Enabled) return false;

            // Can't enable if the module is on the explicit
            // "incompatible" list.
            if (GameService.Module.ModuleIsExplicitlyIncompatible(this.Model)) return false;

            // Can't enable if module's assembly is dirty
            // (i.e. previous version of it has been loaded)
            if (this.Model.IsModuleAssemblyStateDirty) return false;

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
