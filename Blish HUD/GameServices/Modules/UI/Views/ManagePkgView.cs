using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Graphics.UI.Exceptions;
using Blish_HUD.Input;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Presenters;
using Microsoft.Scripting.Utils;
using Microsoft.Xna.Framework;
using Version = SemVer.Version;

namespace Blish_HUD.Modules.UI.Views {

    public class ManagePkgView : View {

        public enum PkgVersionRelationship {
            NotInstalled,
            CanUpdate,
            CurrentVersion
        }

        public event EventHandler<ValueEventArgs<Version>> VersionSelected;
        public event EventHandler<EventArgs>               ActionClicked;

        public string PackageActionText {
            get => _actionButton?.Text ?? throw new ViewNotBuiltException();
            set => (_actionButton ?? throw new ViewNotBuiltException()).Text = value;
        }

        public bool PackageActionEnabled {
            get => _actionButton?.Enabled ?? throw new ViewNotBuiltException();
            set => (_actionButton ?? throw new ViewNotBuiltException()).Enabled = value;
        }

        public string ModuleName {
            get => _nameLabel?.Text ?? throw new ViewNotBuiltException();
            set {
                (_nameLabel ?? throw new ViewNotBuiltException()).Text = value;
                _nameLabel.Visible                                     = true;
                _authLabel.Left                                        = _nameLabel.Right + 10;
            }
        }

        public string ModuleNamespace {
            get => _nameLabel?.BasicTooltipText ?? throw new ViewNotBuiltException();
            set => (_nameLabel ?? throw new ViewNotBuiltException()).BasicTooltipText = value;
        }

        private ModuleContributor _moduleContributor;

        public ModuleContributor ModuleContributor {
            get =>
                _authLabel != null
                    ? _moduleContributor
                    : throw new ViewNotBuiltException();
            set {
                if (_authLabel == null) throw new ViewNotBuiltException();

                _moduleContributor = value;

                _authLabel.Text             = $"{Strings.GameServices.ModulesService.ModuleManagement_AuthoredBy} {_moduleContributor.Name}";
                _authLabel.BasicTooltipText = _moduleContributor.Username;
            }
        }

        public string ModuleDescription {
            get => _descLabel?.Text ?? throw new ViewNotBuiltException();
            set {
                if (_descLabel == null) throw new ViewNotBuiltException();

                _descLabel.Text             = value;
                _descLabel.BasicTooltipText = value;
            }
        }

        private IEnumerable<Version> _moduleVersions;

        public IEnumerable<Version> ModuleVersions {
            get =>
                _versionDropdown != null
                    ? _moduleVersions
                    : throw new ViewNotBuiltException();
            set {
                if (_versionDropdown == null) throw new ViewNotBuiltException();

                _moduleVersions = value;

                _versionDropdown.Items.Clear();
                _versionDropdown.Items.AddRange(CollectionUtils.Select(_moduleVersions, v => v.ToString()));
            }
        }

        private Version _selectedVersion;

        public Version SelectedVersion {
            get =>
                _versionDropdown != null
                    ? _selectedVersion
                    : throw new ViewNotBuiltException();
            set {
                if (_versionDropdown == null) throw new ViewNotBuiltException();

                _selectedVersion = value;

                _versionDropdown.SelectedItem = _selectedVersion.ToString();
            }
        }

        private void SetRelationIcon(PkgVersionRelationship pkgVersionRelationship) {
            switch (pkgVersionRelationship) {
                case PkgVersionRelationship.NotInstalled:
                    _statusImage.Visible      = false;
                    break;
                case PkgVersionRelationship.CanUpdate:
                    _statusImage.Texture          = GameService.Content.GetTexture("common/157397");
                    _statusImage.BasicTooltipText = Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_PackageRelationship_CanUpdate;
                    _statusImage.Visible          = true;
                    break;
                case PkgVersionRelationship.CurrentVersion:
                    _statusImage.Texture          = GameService.Content.GetTexture("common/157330");
                    _statusImage.BasicTooltipText = Strings.GameServices.Modules.RepoAndPkgManagement.PkgRepo_PackageRelationship_CurrentVersion;
                    _statusImage.Visible          = true;
                    break;
            }
        }

        private PkgVersionRelationship _versionRelationship = PkgVersionRelationship.NotInstalled;

        public PkgVersionRelationship VersionRelationship {
            get => _versionRelationship;
            set {
                if (_statusImage == null) throw new ViewNotBuiltException();

                _versionRelationship = value;

                SetRelationIcon(value);
            }
        }

        private Label          _nameLabel;
        private Label          _authLabel;
        private Label          _descLabel;
        private Image          _statusImage;
        private Dropdown       _versionDropdown;
        private StandardButton _actionButton;

        public ManagePkgView() { /* NOOP */ }

        public ManagePkgView(IGrouping<string, PkgManifest> model) {
            this.WithPresenter(new ManagePkgPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {
            _nameLabel = new Label() {
                Text           = "W",
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Font           = GameService.Content.DefaultFont18,
                Location       = new Point(5, 5),
                Visible        = false,
                Parent         = buildPanel
            };

            _authLabel = new Label() {
                Visible           = false,
                AutoSizeWidth     = true,
                AutoSizeHeight    = false,
                Height            = _nameLabel.Height,
                VerticalAlignment = VerticalAlignment.Bottom,
                Left              = _nameLabel.Right + 10,
                Top               = _nameLabel.Top,
                Parent            = buildPanel
            };

            _versionDropdown = new Dropdown() {
                Width   = 128,
                Right   = buildPanel.Width - 5,
                Top     = 5,
                Parent  = buildPanel
            };

            _statusImage = new Image(GameService.Content.GetTexture("common/157397")) {
                Visible = false,
                Size    = new Point(16, 16),
                Top     = _versionDropdown.Height / 2 - 8 + _versionDropdown.Top,
                Right   = _versionDropdown.Left           - 8,
                Parent  = buildPanel
            };

            _actionButton = new StandardButton() {
                Width  = 132,
                Right  = buildPanel.Width  - 3,
                Bottom = buildPanel.Height - 3,
                Parent = buildPanel
            };

            _descLabel = new Label() {
                WrapText          = true,
                Font              = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic),
                Location          = new Point(_nameLabel.Left, _nameLabel.Bottom + 4),
                Size              = new Point(_actionButton.Left                 - _nameLabel.Left * 2, buildPanel.Height - _nameLabel.Bottom - 8),
                Parent            = buildPanel,
                VerticalAlignment = VerticalAlignment.Top
            };

            _statusImage.Click            += StatusImageOnClick;
            _actionButton.Click           += OnActionClicked;
            _versionDropdown.ValueChanged += OnVersionSelected;
        }

        private void StatusImageOnClick(object sender, MouseEventArgs e) {
            this.SelectedVersion = _moduleVersions.Max();
        }

        private void OnActionClicked(object sender, MouseEventArgs e) {
            this.ActionClicked?.Invoke(sender, e);
        }

        private void OnVersionSelected(object sender, ValueChangedEventArgs e) {
            this.VersionSelected?.Invoke(sender, new ValueEventArgs<Version>(new Version(e.CurrentValue)));
        }

    }
}
