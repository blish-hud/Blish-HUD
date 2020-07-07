using System;
using System.Collections.Generic;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Version = SemVer.Version;

namespace Blish_HUD.Modules.UI.Views {
    public class ManageModuleView : View {

        public event EventHandler<EventArgs> ClearPermissionsClicked;
        public event EventHandler<EventArgs> ModifyPermissionsClicked;

        public event EventHandler<EventArgs> EnableModuleClicked;
        public event EventHandler<EventArgs> DisableModuleClicked;

        private Label _moduleTextLabel;
        private Image _moduleHeaderLabel;
        private Label _moduleNameLabel;
        private Label _moduleVersionLabel;
        private Label _moduleStateLabel;

        private Image _authorImage;
        private Label _authoredByLabel;
        private Label _authorNameLabel;

        private StandardButton _enableButton;
        private StandardButton _disableButton;

        private Panel _collapsePanel;

        private Panel _descriptionPanel;
        private Label _descriptionLabel;

        private ViewContainer _permissionView;
        private ViewContainer _dependencyView;

        private ViewContainer _settingView;

        private ContextMenuStrip _settingsMenu;
        private GlowButton       _settingsButton;

        private readonly Dictionary<ModuleRunState, (string Status, Color color)> _moduleStatusLookup = new Dictionary<ModuleRunState, (string Status, Color color)> {
            {ModuleRunState.Unloaded, (Strings.GameServices.ModulesService.ModuleState_Disabled, Control.StandardColors.DisabledText)},
            {ModuleRunState.Loading, (Strings.GameServices.ModulesService.ModuleState_Loading, Control.StandardColors.Yellow)},
            {ModuleRunState.Loaded, (Strings.GameServices.ModulesService.ModuleState_Enabled, Color.FromNonPremultiplied(0, 255, 25, 255))},
            {ModuleRunState.Unloading, (Strings.GameServices.ModulesService.ModuleState_Disabling, Control.StandardColors.Yellow)},
            {ModuleRunState.FatalError, (Strings.GameServices.ModulesService.ModuleState_FatalError, Control.StandardColors.Red)}
        };

        public string ModuleName {
            get => _moduleNameLabel.Text;
            set => _moduleNameLabel.Text = value;
        }

        public string ModuleNamespace {
            get => _moduleNameLabel.BasicTooltipText;
            set => _moduleNameLabel.BasicTooltipText = value;
        }

        private Version _moduleVersion;
        public Version ModuleVersion {
            get => _moduleVersion;
            set {
                _moduleVersion = value;

                _moduleVersionLabel.Text = $"v{_moduleVersion}";

                UpdateHeaderLayout();
            }
        }

        private ModuleRunState _moduleRunState = ModuleRunState.Unloaded;
        public ModuleRunState ModuleState {
            get => _moduleRunState;
            set {
                _moduleRunState = value;

                var (status, color) = _moduleStatusLookup[_moduleRunState];

                UpdateModuleRunState(status, color);

                UpdateHeaderLayout();
            }
        }

        public AsyncTexture2D AuthorImage {
            get => _authorImage.Texture;
            set => _authorImage.Texture = value;
        }

        public string AuthorName {
            get => _authorNameLabel.Text;
            set => _authorNameLabel.Text = value;
        }

        public string ModuleDescription {
            get => _descriptionLabel.Text;
            set => _descriptionLabel.Text = value;
        }

        public bool CanEnable {
            get => _enableButton.Enabled;
            set => _enableButton.Enabled = value;
        }

        public bool CanDisable {
            get => _disableButton.Enabled;
            set => _disableButton.Enabled = value;
        }

        private ModuleDependencyCheckDetails[] _moduleDependencyDetails;
        public ModuleDependencyCheckDetails[] ModuleDependencyDetails {
            get => _moduleDependencyDetails;
            set {
                _moduleDependencyDetails = value;

                _dependencyView.Show(new ModuleDependencyView(_moduleDependencyDetails));
            }
        }

        public void SetPermissionsView(ModulePermissionView view) {
            _permissionView.Show(view);
        }

        public void SetDependenciesView(ModuleDependencyView view) {
            _dependencyView.Show(view);
        }

        protected override void Build(Panel buildPanel) {
            // Header

            _moduleTextLabel = new Label() {
                Text           = Strings.GameServices.ModulesService.ManageModulesSection,
                Location       = new Point(24, 0),
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                StrokeText     = true,
                Parent         = buildPanel
            };

            _moduleHeaderLabel = new Image() {
                Texture  = GameService.Content.GetTexture("358411"),
                Location = new Point(0,   _moduleTextLabel.Bottom - 6),
                Size     = new Point(875, 110),
                Parent   = buildPanel
            };

            _moduleNameLabel = new Label() {
                Text           = "[Module Name]",
                Font           = GameService.Content.DefaultFont32,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                StrokeText     = true,
                Location       = new Point(_moduleTextLabel.Left, _moduleTextLabel.Bottom),
                Parent         = buildPanel
            };

            _moduleVersionLabel = new Label() {
                Text              = "[Module Version]",
                Height            = _moduleNameLabel.Height - 6,
                VerticalAlignment = VerticalAlignment.Bottom,
                AutoSizeWidth     = true,
                StrokeText        = true,
                Font              = GameService.Content.DefaultFont12,
                Location          = new Point(_moduleNameLabel.Right + 8, _moduleNameLabel.Top),
                Parent            = buildPanel
            };

            _moduleStateLabel = new Label() {
                Text              = "[Module State]",
                Height            = _moduleNameLabel.Height - 6,
                VerticalAlignment = VerticalAlignment.Bottom,
                AutoSizeWidth     = true,
                StrokeText        = true,
                Font              = GameService.Content.DefaultFont12,
                Location          = new Point(_moduleVersionLabel.Right + 8, _moduleNameLabel.Top),
                Parent            = buildPanel
            };

            // Author

            _authorImage = new Image() {
                Location = new Point(_moduleNameLabel.Left, _moduleNameLabel.Bottom),
                Size     = new Point(32,               32),
                Parent   = buildPanel
            };

            _authoredByLabel = new Label() {
                Text              = Strings.GameServices.ModulesService.ModuleManagement_AuthoredBy,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                StrokeText        = true,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font              = GameService.Content.DefaultFont12,
                Location          = new Point(_authorImage.Right + 2, _authorImage.Top - 2),
                Parent            = buildPanel
            };

            _authorNameLabel = new Label() {
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                StrokeText     = true,
                Location       = new Point(_authorImage.Right + 2, _authoredByLabel.Bottom),
                Parent         = buildPanel
            };

            // Enable & disable module

            _enableButton = new StandardButton() {
                Location = new Point(buildPanel.Width - 192, _moduleHeaderLabel.Top + _moduleHeaderLabel.Height / 4 - StandardButton.STANDARD_CONTROL_HEIGHT / 2),
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

            _enableButton.Click  += delegate { EnableModuleClicked?.Invoke(this, EventArgs.Empty); };
            _disableButton.Click += delegate { DisableModuleClicked?.Invoke(this, EventArgs.Empty); };

            // Collapse Sections

            _collapsePanel = new Panel() {
                Size      = new Point(buildPanel.Width, buildPanel.Height - _moduleNameLabel.Bottom + 32 + 4),
                Location  = new Point(0,                _moduleNameLabel.Bottom + 32                     + 4),
                CanScroll = true,
                Parent    = buildPanel
            };

            // Description

            _descriptionPanel = new Panel() {
                Size       = new Point(_collapsePanel.ContentRegion.Width, 180),
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
                Size     = _descriptionPanel.Size - new Point(350, 0),
                Location = new Point(0, _descriptionPanel.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Parent   = _collapsePanel
            };

            // Dependencies

            _dependencyView = new ViewContainer() {
                Size     = new Point(_descriptionPanel.Width - _permissionView.Right - Panel.MenuStandard.ControlOffset.X / 2, _descriptionPanel.Height),
                Location = new Point(_permissionView.Right                           + Panel.MenuStandard.ControlOffset.X / 2, _permissionView.Top),
                Parent   = _collapsePanel
            };

            // Module Settings

            _settingView = new ViewContainer() {
                CanScroll  = true,
                Size       = new Point(_dependencyView.Right - _permissionView.Left, _descriptionPanel.Height),
                Location   = new Point(_permissionView.Left,                         _permissionView.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Title      = Strings.GameServices.ModulesService.ModuleManagement_ModuleSettings,
                ShowBorder = true,
                Parent     = _collapsePanel
            };

            // Settings Menu
            _settingsMenu = new ContextMenuStrip();

            _settingsButton = new GlowButton() {
                Location         = new Point(_enableButton.Right + 12, _enableButton.Top),
                Icon             = GameService.Content.GetTexture("common/157109"),
                ActiveIcon       = GameService.Content.GetTexture("common/157110"),
                BasicTooltipText = Strings.Common.Options,
                Parent           = buildPanel
            };

            _settingsButton.Click += delegate { _settingsMenu.Show(_settingsButton); };
        }

        private void UpdateHeaderLayout() {
            _moduleVersionLabel.Location = new Point(_moduleNameLabel.Right    + 8, _moduleNameLabel.Top);
            _moduleStateLabel.Location   = new Point(_moduleVersionLabel.Right + 8, _moduleNameLabel.Top);
        }

        private void UpdateModuleRunState(string status, Color color) {
            _moduleStateLabel.Text      = status;
            _moduleStateLabel.TextColor = color;
        }

    }
}
