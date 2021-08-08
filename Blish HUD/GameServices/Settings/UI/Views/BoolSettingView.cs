using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public class BoolSettingView : SettingView<bool> {

        private const int CONTROL_PADDING = 5;

        private Checkbox _boolCheckbox;

        public BoolSettingView(IUiSettingEntry<bool> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            switch (complianceRequisite) {
                case SettingDisabledComplianceRequisite disabledRequisite:
                    _boolCheckbox.Enabled = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void BuildSetting(Panel buildPanel) {
            _boolCheckbox = new Checkbox() {
                Location = new Point(CONTROL_PADDING),
                Parent   = buildPanel
            };

            _boolCheckbox.CheckedChanged += BoolCheckboxOnCheckedChanged;
        }

        private void BoolCheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            this.OnValueChanged(new ValueEventArgs<bool>(e.Checked));
        }

        private void UpdateSize() {
            this.ViewTarget.Size = new Point(_boolCheckbox.Right + CONTROL_PADDING,
                                             _boolCheckbox.Bottom + CONTROL_PADDING);
        }

        protected override void RefreshDisplayName(string displayName) {
            _boolCheckbox.Text = displayName;

            UpdateSize();
        }

        protected override void RefreshDescription(string description) {
            _boolCheckbox.BasicTooltipText = description;
        }

        protected override void RefreshValue(bool value) {
            _boolCheckbox.Checked = value;
        }

        protected override void Unload() {
            _boolCheckbox.CheckedChanged -= BoolCheckboxOnCheckedChanged;
        }

    }
}
