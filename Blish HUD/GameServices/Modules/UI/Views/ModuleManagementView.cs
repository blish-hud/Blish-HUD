using Blish_HUD.Controls;
using Blish_HUD.Modules.UI.Presenters;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleManagementView : View<ModuleManagementPresenter> {

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

        private Panel _permissionPanel;

        private Panel         _dependencyPanel;
        private ViewContainer _dependencyView;
        //private Menu  _dependencyMenuList;


        private ContextMenuStrip     _settingsMenu;
        private GlowButton           _settingsButton;
        private ContextMenuStripItem _ignoreDependencyRequirementsMenuStripItem;
        private ContextMenuStripItem _viewModuleLogsMenuStripItem;

        public Label ModuleName    => _moduleName;
        public Label ModuleVersion => _moduleVersion;
        public Label ModuleState   => _moduleState;

        public Image AuthorImage => _authorImage;
        public Label AuthorName  => _authorName;

        public StandardButton EnableButton  => _enableButton;
        public StandardButton DisableButton => _disableButton;

        public Label DescriptionLabel => _descriptionLabel;

        public ViewContainer DependencyView => _dependencyView;

        public ContextMenuStripItem IgnoreDependencyRequirementsMenuStripItem => _ignoreDependencyRequirementsMenuStripItem;

        public ModuleManagementView(ModuleManager model) {
            this.Presenter = new ModuleManagementPresenter(this, model);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            _moduleText = new Label() {
                Text           = "Manage Modules",
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
                Text              = "Authored by",
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
                Location = new Point(buildPanel.Right - 192, _moduleHeader.Top + _moduleHeader.Height / 4 - StandardButton.STANDARD_CONTROL_HEIGHT / 2),
                Text     = "Enable Module",
                Enabled  = false,
                Parent   = buildPanel
            };

            _disableButton = new StandardButton() {
                Location = new Point(buildPanel.Right - 192, _enableButton.Bottom + 2),
                Text     = "Disable Module",
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
                Title      = "Description",
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

            var permissionPanel = new FlowPanel() {
                Size                 = _descriptionPanel.Size - new Point(350, 0),
                Location             = new Point(0, _descriptionPanel.Bottom + Panel.MenuStandard.ControlOffset.Y),
                PadLeftBeforeControl = true,
                PadTopBeforeControl  = true,
                ControlPadding       = new Vector2(10),
                Title                = "Permissions",
                ShowBorder           = true,
                CanScroll            = true,
                Parent               = _collapsePanel
            };

            // TODO: Enable permissions on modules UI
            //foreach (var perm in cModuleMan.Manifest.ApiPermissions) {
            //    var permCheckbox = new Checkbox() {
            //        Text   = perm.Key.ToString(),
            //        Parent = permissionPanel,
            //        Width  = permissionPanel.Width / 3
            //    };
            //}

            // Dependencies

            _dependencyPanel = new Panel() {
                Size       = new Point(_descriptionPanel.Width - permissionPanel.Right - Panel.MenuStandard.ControlOffset.X / 2, _descriptionPanel.Height),
                Location   = new Point(permissionPanel.Right                           + Panel.MenuStandard.ControlOffset.X / 2, permissionPanel.Top),
                Title      = "Dependencies",
                ShowBorder = true,
                CanScroll  = true,
                Parent     = _collapsePanel
            };

            _dependencyView = new ViewContainer() {
                Size   = _dependencyPanel.ContentRegion.Size,
                Parent = _dependencyPanel
            };

            // Settings Menu
            _settingsMenu = new ContextMenuStrip();

            _settingsButton = new GlowButton() {
                Location         = new Point(_enableButton.Right + 12, _enableButton.Top),
                Icon             = GameService.Content.GetTexture(@"common\157109"),
                ActiveIcon       = GameService.Content.GetTexture(@"common\157110"),
                BasicTooltipText = "Options",
                Parent           = buildPanel
            };

            _settingsButton.Click += delegate { _settingsMenu.Show(_settingsButton); };

            _ignoreDependencyRequirementsMenuStripItem          = _settingsMenu.AddMenuItem("Ignore Dependency Requirements");
            _ignoreDependencyRequirementsMenuStripItem.CanCheck = true;

            _viewModuleLogsMenuStripItem = _settingsMenu.AddMenuItem("View Module Logs");

            //if (cModuleMan.Manifest.Directories.Any()) {
            //    var directoriesMenu = settingsMenu.AddMenuItem("Directories");
            //    var subDirectoriesMenu = new ContextMenuStrip();

            //    foreach (var directory in cModuleMan.Manifest.Directories) {
            //        subDirectoriesMenu.AddMenuItem($"Explore '{directory}'");
            //    }

            //    directoriesMenu.Submenu = subDirectoriesMenu;
            //}


        }

    }
}
