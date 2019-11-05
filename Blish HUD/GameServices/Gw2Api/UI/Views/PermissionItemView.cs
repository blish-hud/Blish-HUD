using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Presenters;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Api.UI.Views {
    public class PermissionItemView : View<PermissionItemPresenter> {

        public event EventHandler<EventArgs> ConsentChanged;

        private Image    _permissionIcon;
        private Label    _permissionNameLabel;
        private Label    _permissionDescriptionLabel;
        private Checkbox _permissionConsentCheckbox;

        public AsyncTexture2D Icon {
            get => _permissionIcon.Texture;
            set => _permissionIcon.Texture = value;
        }

        public string Name {
            get => _permissionNameLabel.Text;
            set => _permissionNameLabel.Text = value;
        }

        public string Description {
            get => _permissionDescriptionLabel.Text;
            set {
                _permissionDescriptionLabel.Text = value;

                UpdateLayout();
            }
        }

        public bool ShowConsent {
            get => _permissionConsentCheckbox.Visible;
            set => _permissionConsentCheckbox.Visible = value;
        }

        public bool Consented {
            get => _permissionConsentCheckbox.Checked;
            set => _permissionConsentCheckbox.Checked = value;
        }

        public PermissionItemView(PermissionItemPresenter.PermissionConsent permissionConsent) {
            this.Presenter = new PermissionItemPresenter(this, permissionConsent);
        }

        private void UpdateLayout() {
            this.ViewTarget.Height = _permissionDescriptionLabel.Bottom;
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            _permissionIcon = new Image() {
                Location = new Point(6,  6),
                Size     = new Point(32, 32),
                Parent   = buildPanel
            };

            _permissionConsentCheckbox = new Checkbox() {
                Location = Point.Zero,
                Checked  = true,
                Parent   = buildPanel,
            };

            _permissionNameLabel = new Label() {
                Font = GameService.Content.DefaultFont16,
                Text           = "_",
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                ShowShadow     = true,
                Location       = new Point(_permissionIcon.Right + 6, 0),
                TextColor      = Color.FromNonPremultiplied(255, 238, 153, 255),
                Parent         = buildPanel
            };

            _permissionDescriptionLabel = new Label() {
                Text           = "_",
                WrapText       = true,
                AutoSizeHeight = true,
                ShowShadow     = true,
                Width          = buildPanel.Width - _permissionNameLabel.Left,
                Location       = new Point(_permissionNameLabel.Left, _permissionNameLabel.Bottom - 3),
                Parent         = buildPanel
            };

            _permissionConsentCheckbox.CheckedChanged += PermissionConsentCheckboxOnCheckedChanged;

            _permissionIcon.Click             += ToggleConsentCheckbox;
            _permissionNameLabel.Click        += ToggleConsentCheckbox;
            _permissionDescriptionLabel.Click += ToggleConsentCheckbox;
        }

        private void ToggleConsentCheckbox(object sender, MouseEventArgs e) {
            if (this.ShowConsent) {
                _permissionConsentCheckbox.Checked = !_permissionConsentCheckbox.Checked;
            }
        }

        private void PermissionConsentCheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            this.ConsentChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override void Unload() {
            _permissionConsentCheckbox.CheckedChanged -= PermissionConsentCheckboxOnCheckedChanged;

            _permissionIcon.Click             -= ToggleConsentCheckbox;
            _permissionNameLabel.Click        -= ToggleConsentCheckbox;
            _permissionDescriptionLabel.Click -= ToggleConsentCheckbox;
        }

    }
}
