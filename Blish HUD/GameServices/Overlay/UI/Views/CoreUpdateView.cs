using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Graphics.UI.Exceptions;
using Blish_HUD.Input;
using Blish_HUD.Overlay.UI.Presenters;
using Microsoft.Scripting.Utils;
using Microsoft.Xna.Framework;
using Version = SemVer.Version;

namespace Blish_HUD.Overlay.UI.Views {
    public class CoreUpdateView : View {

        public event EventHandler<ValueEventArgs<Version>> VersionSelected;
        public event EventHandler<EventArgs>               ActionClicked;
        public event EventHandler<ValueEventArgs<bool>>    ShowPrereleasesChanged;

        private StandardButton _actionButton;
        private Dropdown       _versionDropdown;
        private Label          _failedLabel;

        private IEnumerable<Version> _coreVersions;
        private Version              _selectedVersion;

        public string PackageActionText {
            get => _actionButton?.Text ?? throw new ViewNotBuiltException();
            set => (_actionButton ?? throw new ViewNotBuiltException()).Text = value;
        }

        public bool PackageActionEnabled {
            get => _actionButton?.Enabled ?? throw new ViewNotBuiltException();
            set => (_actionButton ?? throw new ViewNotBuiltException()).Enabled = value;
        }

        public IEnumerable<Version> CoreVersions {
            get =>
                _versionDropdown != null
                    ? _coreVersions
                    : throw new ViewNotBuiltException();
            set {
                if (_versionDropdown == null) throw new ViewNotBuiltException();

                _coreVersions = value;

                _failedLabel.Visible = !(_actionButton.Enabled = _versionDropdown.Enabled = value.Any());
                
                _versionDropdown.Items.Clear();
                _versionDropdown.Items.AddRange(_coreVersions?.Select<string>(v => v.ToString()) ?? Array.Empty<string>());
            }
        }

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

        public CoreUpdateView(string versionsUrl) {
            this.WithPresenter(new CoreUpdatePresenter(this, versionsUrl));
        }

        public CoreUpdateView() : this(null) { /* NOOP */ }

        protected override void Build(Panel buildPanel) {
            //buildPanel.BackgroundColor = Color.Magenta;
            //buildPanel.ClipsBounds = false;

            var version = new Label {
                AutoSizeWidth     = true,
                VerticalAlignment = VerticalAlignment.Middle,
                Height            = buildPanel.Height,
                Text              = $"{Strings.Common.BlishHUD} v{Program.OverlayVersion.BaseVersion()}",
                BasicTooltipText  = $"{Strings.Common.BlishHUD} v{Program.OverlayVersion}",
                Font              = GameService.Content.DefaultFont14,
                StrokeText        = true,
                ClipsBounds       = false,
                Location          = new Point(8, 0),
                Parent            = buildPanel
            };

            var settingsButton = new GlowButton {
                Right = buildPanel.Width - 8,
                Icon             = GameService.Content.GetTexture("common/157109"),
                ActiveIcon       = GameService.Content.GetTexture("common/157110"),
                Visible          = true,
                BasicTooltipText = Strings.Common.Options,
                Parent           = buildPanel
            };

            _versionDropdown = new Dropdown() {
                Width  = 128,
                Right  = settingsButton.Left - 8,
                Parent = buildPanel
            };

            _actionButton = new StandardButton() {
                Text   = "Update",
                Width  = 128,
                Right  = _versionDropdown.Left - 8,
                Parent = buildPanel
            };

            _failedLabel = new Label() {
                AutoSizeWidth = true,
                Text          = "Error while checking for new version!",
                Height        = buildPanel.Height,
                TextColor     = Control.StandardColors.Yellow,
                Visible       = false,
                Right         = _actionButton.Left - 8,
                Parent        = buildPanel
            };

            settingsButton.Top   = buildPanel.Height / 2 - _actionButton.Height    / 2 - 2;
            _actionButton.Top    = buildPanel.Height / 2 - _actionButton.Height    / 2;
            _versionDropdown.Top = buildPanel.Height / 2 - _versionDropdown.Height / 2;

            _actionButton.Click += OnActionClicked;
            _versionDropdown.ValueChanged += OnVersionSelected;
        }

        private void OnVersionSelected(object sender, ValueChangedEventArgs e) {
            this.VersionSelected?.Invoke(sender, new ValueEventArgs<Version>(new Version(e.CurrentValue)));
        }

        private void OnActionClicked(object sender, MouseEventArgs e) {
            this.ActionClicked?.Invoke(sender, e);
        }

    }
}
