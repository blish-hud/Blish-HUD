using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public class StringSettingView : SettingView<string> {

        private const int CONTROL_PADDING = 5;

        private const int TEXTBOX_WIDTH  = 250;
        private const int TEXTBOX_HEIGHT = 27;

        private Label   _displayNameLabel;
        private TextBox _stringTextbox;

        public StringSettingView(IUiSettingEntry<string> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            switch (complianceRequisite) {
                case SettingDisabledComplianceRequisite disabledRequisite:
                    _displayNameLabel.Enabled = !disabledRequisite.Disabled;
                    _stringTextbox.Enabled    = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void BuildSetting(Panel buildPanel) {
            _displayNameLabel = new Label() {
                AutoSizeWidth = true,
                Location      = new Point(CONTROL_PADDING, 0),
                Parent        = buildPanel
            };

            _stringTextbox = new TextBox() {
                Size   = new Point(TEXTBOX_WIDTH, TEXTBOX_HEIGHT),
                Parent = buildPanel
            };

            // Update setting when the textbox loses focus
            // instead of anytime the text changes
            _stringTextbox.InputFocusChanged += StringTextboxOnInputFocusChanged;
        }

        private void StringTextboxOnInputFocusChanged(object sender, ValueEventArgs<bool> e) {
            if (e.Value == false) {
                this.OnValueChanged(new ValueEventArgs<string>(_stringTextbox.Text));
            }
        }

        private void UpdateSizeAndLayout() {
            this.ViewTarget.Height   = _stringTextbox.Bottom;
            _displayNameLabel.Height = _stringTextbox.Bottom;

            if (this.DefinedWidth > 0) {
                _stringTextbox.Left   = _displayNameLabel.Right + CONTROL_PADDING;
                this.ViewTarget.Width = _stringTextbox.Right    + CONTROL_PADDING;
            } else {
                _stringTextbox.Location = new Point(this.ViewTarget.Width - CONTROL_PADDING - TEXTBOX_WIDTH, 0);
            }
        }

        protected override void RefreshDisplayName(string displayName) {
            _displayNameLabel.Text = displayName;

            UpdateSizeAndLayout();
        }

        protected override void RefreshDescription(string description) {
            _stringTextbox.BasicTooltipText = description;
        }

        protected override void RefreshValue(string value) {
            _stringTextbox.Text = value;
        }

        protected override void Unload() {
            _stringTextbox.InputFocusChanged -= StringTextboxOnInputFocusChanged;
        }

    }
}
