using System;
using System.Collections.Generic;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.UI.Presenters;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Blish_HUD.Overlay.UI.Presenters;
using Blish_HUD.Common.UI.Views;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleManagementView : View<ModuleManagementPresenter> {

        public event EventHandler<EventArgs> ClearPermissionsClicked;
        public event EventHandler<EventArgs> ModifyPermissionsClicked;

        private Label _moduleText;
        private Image _moduleHeader;
        private Label _moduleName;
        private Label _moduleVersion;
        private Label _moduleState;

        private Image _authorImage;
        private Label _authoredBy;
        private Label _authorName;

        private StandardButton _enableButton;
        private StandardButton _disableButton;

        private Panel _collapsePanel;

        private Panel _descriptionPanel;
        private Label _descriptionLabel;

        private ViewContainer _permissionView;
        private Image         _permissionWarning;

        private ContextMenuStripItem _clearPermissions;
        private ContextMenuStripItem _modifyPermissions;

        private ViewContainer _dependencyView;
        private Image         _dependencyWarning;

        private ViewContainer _settingView;

        private ContextMenuStrip     _settingsMenu;
        private GlowButton           _settingsButton;
        private ContextMenuStripItem _ignoreDependencyRequirementsMenuStripItem;
        private ContextMenuStripItem _viewModuleLogsMenuStripItem;

        public string ModuleName {
            get => _moduleName.Text;
            set => _moduleName.Text = value;
        }

        public string ModuleVersion {
            get => _moduleVersion.Text;
            set => _moduleVersion.Text = value;
        }

        public string ModuleStateText {
            get => _moduleState.Text;
            set => _moduleState.Text = value;
        }

        public Color ModuleStateColor {
            get => _moduleState.TextColor;
            set => _moduleState.TextColor = value;
        }

        public AsyncTexture2D AuthorImage {
            get => _authorImage.Texture;
            set => _authorImage.Texture = value;
        }

        public string AuthorName {
            get => _authorName.Text;
            set => _authorName.Text = value;
        }

        public string ModuleDescription {
            get => _descriptionLabel.Text;
            set => _descriptionLabel.Text = value;
        }

        public bool CanEnable {
            get => _enableButton.Enabled;
            set {
                _enableButton.Enabled = value;

                _modifyPermissions.Enabled = (_enableButton.Enabled || _permissionWarning.Visible) && !_disableButton.Enabled;
                _clearPermissions.Enabled  = (_enableButton.Enabled || _permissionWarning.Visible) && !_disableButton.Enabled;
            }
        }

        public bool CanDisable {
            get => _disableButton.Enabled;
            set {
                _disableButton.Enabled = value;

                _modifyPermissions.Enabled = (_enableButton.Enabled || _permissionWarning.Visible) && !_disableButton.Enabled;
                _clearPermissions.Enabled  = (_enableButton.Enabled || _permissionWarning.Visible) && !_disableButton.Enabled;
            }
        }

        public StandardButton EnableButton  => _enableButton;
        public StandardButton DisableButton => _disableButton;

        public ViewContainer PermissionView => _permissionView;

        public string PermissionWarning {
            get => _permissionWarning.BasicTooltipText;
            set {
                if (string.IsNullOrEmpty(value)) {
                    _permissionWarning.Visible = false;
                } else {
                    _permissionWarning.BasicTooltipText = value;
                    _permissionWarning.Visible          = true;
                }
            }
        }

        public string DependencyWarning {
            get => _dependencyWarning.BasicTooltipText;
            set {
                if (string.IsNullOrEmpty(value)) {
                    _dependencyWarning.Visible = false;
                } else {
                    _dependencyWarning.BasicTooltipText = value;
                    _dependencyWarning.Visible          = true;
                }
            }
        }

        public ViewContainer DependencyView => _dependencyView;

        public ViewContainer SettingsView => _settingView;

        public ContextMenuStripItem IgnoreDependencyRequirementsMenuStripItem => _ignoreDependencyRequirementsMenuStripItem;

        public ModuleManagementView(ModuleManager model) {
            this.Presenter = new ModuleManagementPresenter(this, model);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            _moduleText = new Label() {
                Text           = Strings.GameServices.ModulesService.ManageModulesSection,
                Location       = new Point(24, 0),
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                StrokeText     = true,
                Parent         = buildPanel
            };

            _moduleHeader = new Image() {
                Texture  = GameService.Content.GetTexture("358411"),
                Location = new Point(0,   _moduleText.Bottom - 6),
                Size     = new Point(875, 110),
                Parent   = buildPanel
            };

            _moduleName = new Label() {
                Text           = this.Presenter.GetModuleName(),
                Font           = GameService.Content.DefaultFont32,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                StrokeText     = true,
                Location       = new Point(_moduleText.Left, _moduleText.Bottom),
                Parent         = buildPanel
            };

            _moduleVersion = new Label() {
                Text              = this.Presenter.GetModuleVersion(),
                Height            = _moduleName.Height - 6,
                VerticalAlignment = VerticalAlignment.Bottom,
                AutoSizeWidth     = true,
                StrokeText        = true,
                Font              = GameService.Content.DefaultFont12,
                Location          = new Point(_moduleName.Right + 8, _moduleName.Top),
                Parent            = buildPanel
            };

            _moduleState = new Label() {
                Height            = _moduleName.Height - 6,
                VerticalAlignment = VerticalAlignment.Bottom,
                AutoSizeWidth     = true,
                StrokeText        = true,
                Font              = GameService.Content.DefaultFont12,
                Location          = new Point(_moduleVersion.Right + 8, _moduleName.Top),
                Parent            = buildPanel
            };

            // Author
            _authorImage = new Image() {
                Location = new Point(_moduleName.Left, _moduleName.Bottom),
                Size     = new Point(32,               32),
                Parent   = buildPanel
            };

            _authoredBy = new Label() {
                Text              = Strings.GameServices.ModulesService.ModuleManagement_AuthoredBy,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                StrokeText        = true,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font              = GameService.Content.DefaultFont12,
                Location          = new Point(_authorImage.Right + 2, _authorImage.Top - 2),
                Parent            = buildPanel
            };

            _authorName = new Label() {
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                StrokeText     = true,
                Location       = new Point(_authorImage.Right + 2, _authoredBy.Bottom),
                Parent         = buildPanel
            };

            // Enable & disable module

            _enableButton = new StandardButton() {
                Location = new Point(buildPanel.Width - 192, _moduleHeader.Top + _moduleHeader.Height / 4 - StandardButton.STANDARD_CONTROL_HEIGHT / 2),
                Text     = Strings.GameServices.ModulesService.ModuleManagement_EnableModule,
                Enabled  = false,
                Parent   = buildPanel
            };

            _disableButton = new StandardButton() {
                Location = new Point(buildPanel.Width - 192, _enableButton.Bottom + 2),
                Text     = Strings.GameServices.ModulesService.ModuleManagement_DisableModule,
                Enabled  = false,
                Parent   = buildPanel
            };

            // Collapse Sections

            _collapsePanel = new Panel() {
                Size      = new Point(buildPanel.Width, buildPanel.Height - _moduleName.Bottom + 32 + 4),
                Location  = new Point(0,                _moduleName.Bottom + 32                     + 4),
                CanScroll = true,
                Parent    = buildPanel
            };

            // Description

            _descriptionPanel = new Panel() {
                Size       = new Point(_collapsePanel.ContentRegion.Width, 155),
                Title      = Strings.GameServices.ModulesService.ModuleManagement_Description,
                ShowBorder = true,
                CanScroll  = true,
                Parent     = _collapsePanel
            };

            _descriptionLabel = new Label() {
                Location       = new Point(8, 8),
                Width          = _descriptionPanel.Width - 16,
                AutoSizeHeight = true,
                WrapText       = true,
                Parent         = _descriptionPanel
            };

            // Permissions

            _permissionView = new ViewContainer() {
                Size       = _descriptionPanel.Size - new Point(350, 0),
                Location   = new Point(0, _descriptionPanel.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Title      = Strings.GameServices.ModulesService.ModuleManagement_ApiPermissions,
                ShowBorder = true,
                CanScroll  = true,
                Parent     = _collapsePanel
            };

            _permissionWarning = new Image(GameService.Content.GetTexture(@"common\1444522")) {
                Size        = new Point(32, 32),
                Location    = _permissionView.Location - new Point(10, 15),
                ClipsBounds = false,
                Parent      = _collapsePanel
            };

            var permissionOptionsMenu = new ContextMenuStrip();

            var permissionSettingsButton = new GlowButton() {
                Location         = new Point(_permissionView.Right - 42, _permissionView.Top + 3),
                Icon             = GameService.Content.GetTexture(@"common\157109"),
                ActiveIcon       = GameService.Content.GetTexture(@"common\157110"),
                BasicTooltipText = Strings.Common.Options,
                Parent           = _collapsePanel
            };

            _modifyPermissions = permissionOptionsMenu.AddMenuItem(Strings.GameServices.ModulesService.ModuleManagement_ModifyPermissions);
            _clearPermissions  = permissionOptionsMenu.AddMenuItem(Strings.GameServices.ModulesService.ModuleManagement_ClearPermissions);

            _modifyPermissions.Click += delegate { this.ModifyPermissionsClicked?.Invoke(this, EventArgs.Empty); };
            _clearPermissions.Click  += delegate { this.ClearPermissionsClicked?.Invoke(this, EventArgs.Empty); };

            permissionSettingsButton.Click += delegate { permissionOptionsMenu.Show(permissionSettingsButton); };

            // Dependencies

            _dependencyView = new ViewContainer() {
                CanScroll  = true,
                Size       = new Point(_descriptionPanel.Width - _permissionView.Right - Panel.MenuStandard.ControlOffset.X / 2, _descriptionPanel.Height),
                Location   = new Point(_permissionView.Right                           + Panel.MenuStandard.ControlOffset.X / 2, _permissionView.Top),
                Title      = Strings.GameServices.ModulesService.ModuleManagement_Dependencies,
                ShowBorder = true,
                Parent     = _collapsePanel
            };

            _dependencyWarning = new Image(GameService.Content.GetTexture(@"common\1444522")) {
                Size        = new Point(32, 32),
                Location    = _dependencyView.Location - new Point(10, 15),
                ClipsBounds = false,
                Parent      = _collapsePanel
            };

            // Module Settings

            _settingView = new ViewContainer() {
                CanScroll  = true,
                Size       = new Point(_dependencyView.Right - _permissionView.Left, _descriptionPanel.Height),
                Location   = new Point(_permissionView.Left, _permissionView.Bottom + Panel.MenuStandard.ControlOffset.Y),
                //Title      = "Module Settings",
                //ShowBorder = true,
                Parent     = _collapsePanel
            };

            // Settings Menu
            _settingsMenu = new ContextMenuStrip();

            _settingsButton = new GlowButton() {
                Location         = new Point(_enableButton.Right + 12, _enableButton.Top),
                Icon             = GameService.Content.GetTexture(@"common\157109"),
                ActiveIcon       = GameService.Content.GetTexture(@"common\157110"),
                BasicTooltipText = Strings.Common.Options,
                Parent           = buildPanel
            };

            _settingsButton.Click += delegate { _settingsMenu.Show(_settingsButton); };

            _ignoreDependencyRequirementsMenuStripItem          = _settingsMenu.AddMenuItem(Strings.GameServices.ModulesService.ModuleManagement_IgnoreDependencyRequirements);
            _ignoreDependencyRequirementsMenuStripItem.CanCheck = true;

            _viewModuleLogsMenuStripItem = _settingsMenu.AddMenuItem(Strings.GameServices.ModulesService.ModuleManagement_ViewModuleLogs);
        }

    }
}
