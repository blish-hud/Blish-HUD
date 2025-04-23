using System;
using System.Collections.Generic;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Presenters;
using Microsoft.Xna.Framework;
using Version = SemVer.Version;

namespace Blish_HUD.Modules.UI.Views {
    public class ManageModuleView : View {

        public event EventHandler<EventArgs> EnableModuleClicked;
        public event EventHandler<EventArgs> DisableModuleClicked;

        private Label _moduleTextLabel;
        private Label _moduleAssemblyDirtiedWarning;
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

        private Label         _settingMessageLabel;
        private ViewContainer _settingView;

        private GlowButton _settingsButton;

        private readonly Dictionary<ModuleRunState, (string Status, Color color)> _moduleStatusLookup = new Dictionary<ModuleRunState, (string Status, Color color)> {
            {ModuleRunState.Unloaded, (Strings.GameServices.ModulesService.ModuleState_Disabled, Control.StandardColors.DisabledText)},
            {ModuleRunState.Loading, (Strings.GameServices.ModulesService.ModuleState_Loading, Control.StandardColors.Yellow)},
            {ModuleRunState.Loaded, (Strings.GameServices.ModulesService.ModuleState_Enabled, Color.FromNonPremultiplied(0, 255, 25, 255))},
            {ModuleRunState.Unloading, (Strings.GameServices.ModulesService.ModuleState_Disabling, Control.StandardColors.Yellow)},
            {ModuleRunState.FatalError, (Strings.GameServices.ModulesService.ModuleState_FatalError, Control.StandardColors.Red)}
        };

        public bool ModuleAssemblyStateDirtied {
            get => _moduleAssemblyDirtiedWarning.Visible;
            set => _moduleAssemblyDirtiedWarning.Visible = value;
        }

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

        public string ModuleErrorReason { get; set; }

        private ModuleRunState _moduleRunState = ModuleRunState.Unloaded;
        public ModuleRunState ModuleState {
            get => _moduleRunState;
            set {
                _moduleRunState = value;

                var (status, color) = _moduleStatusLookup[_moduleRunState];

                UpdateModuleRunState(status, color, _moduleRunState == ModuleRunState.FatalError ? this.ModuleErrorReason : null);

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

        private ContextMenuStrip _settingMenu;

        public ContextMenuStrip SettingMenu {
            get => _settingMenu;
            set {
                _settingMenu = value;

                _settingsButton.Visible = _settingMenu != null;
            }
        }

        public ManageModuleView() { /* NOOP */ }

        public ManageModuleView(ModuleManager moduleManager) {
            this.WithPresenter(new ManageModulePresenter(this, moduleManager));
        }

        public void SetPermissionsView(ModulePermissionView view) {
            _permissionView.Show(view);
        }

        public void SetDependenciesView(ModuleDependencyView view) {
            _dependencyView.Show(view);
        }

        public void SetSettingsView(IView view) {
            _settingMessageLabel.Hide();
            _settingView.Show(view);

            if (view == null) {
                _settingMessageLabel.Show();
            }
        }

        public void SetCustomState(string status, Color color) {
            UpdateModuleRunState(status, color);
            UpdateHeaderLayout();
        }

        protected override void Build(Container buildPanel) {
            // Header

            _moduleTextLabel = new Label() {
                Text           = Strings.GameServices.ModulesService.ManageModulesSection,
                Location       = new Point(24, 0),
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                StrokeText     = true,
                Parent         = buildPanel
            };

            _moduleAssemblyDirtiedWarning = new Label() {
                Text           = Strings.GameServices.ModulesService.PkgManagement_ModulesNeedRestart,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Right          = buildPanel.Width - 12,
                StrokeText     = true,
                TextColor      = Control.StandardColors.Yellow,
                Parent         = buildPanel
            };

            _moduleHeaderLabel = new Image() {
                Texture  = AsyncTexture2D.FromAssetId(358411),
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
                Size       = new Point(_collapsePanel.ContentRegion.Width, 125),
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
                Size     = new Point(_descriptionPanel.Width - _permissionView.Right - Panel.MenuStandard.ControlOffset.X / 2, _permissionView.Height),
                Location = new Point(_permissionView.Right                           + Panel.MenuStandard.ControlOffset.X / 2, _permissionView.Top),
                Parent   = _collapsePanel
            };

            // Module Settings

            var settingPanelRoot = new Panel() {
                CanScroll  = true,
                ShowBorder = true,
                Title      = Strings.GameServices.ModulesService.ModuleManagement_ModuleSettings,
                Size       = new Point(_dependencyView.Right - _permissionView.Left - 10, 320),
                Location   = new Point(_permissionView.Left,                         _permissionView.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Parent     = _collapsePanel
            };

            _settingMessageLabel = new Label() {
                Size                = settingPanelRoot.ContentRegion.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text                = Strings.GameServices.ModulesService.ModuleSettings_ShownWhenEnabled,
                StrokeText          = true,
                Font                = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic),
                Parent              = settingPanelRoot
            };

            _settingView = new ViewContainer() {
                Size             = settingPanelRoot.ContentRegion.Size,
                CanScroll        = true,
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode  = SizingMode.Fill,
                Parent           = settingPanelRoot
            };

            // Settings Menu

            _settingsButton = new GlowButton() {
                Location         = new Point(_enableButton.Right + 12, _enableButton.Top),
                Icon             = AsyncTexture2D.FromAssetId(157109),
                ActiveIcon       = AsyncTexture2D.FromAssetId(157110),
                Visible          = false,
                BasicTooltipText = Strings.Common.Options,
                Parent           = buildPanel
            };

            _settingsButton.Click += delegate { _settingMenu?.Show(_settingsButton); };
        }

        private void UpdateHeaderLayout() {
            _moduleVersionLabel.Location = new Point(_moduleNameLabel.Right    + 8, _moduleNameLabel.Top);
            _moduleStateLabel.Location   = new Point(_moduleVersionLabel.Right + 8, _moduleNameLabel.Top);
        }

        private void UpdateModuleRunState(string status, Color color, string tooltip = null) {
            _moduleStateLabel.Text      = status;
            _moduleStateLabel.TextColor = color;
            _moduleStateLabel.BasicTooltipText = tooltip;
        }

        protected override void Unload() {
            base.Unload();

            _permissionView?.Dispose();
            _dependencyView?.Dispose();
            _settingView?.Dispose();
        }

    }
}
