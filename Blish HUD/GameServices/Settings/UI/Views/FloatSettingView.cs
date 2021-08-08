using Blish_HUD.Controls;

namespace Blish_HUD.Settings.UI.Views {
    public class FloatSettingView : NumericSettingView<float> {

        public FloatSettingView(IUiSettingEntry<float> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Panel buildPanel) {
            base.BuildSetting(buildPanel);

            _valueTrackBar.SmallStep = true;
        }

        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            switch (complianceRequisite) {
                case FloatRangeRangeComplianceRequisite intRangeRequisite:
                    _valueTrackBar.MinValue = intRangeRequisite.MinValue;
                    _valueTrackBar.MaxValue = intRangeRequisite.MaxValue;
                    break;
                case SettingDisabledComplianceRequisite disabledRequisite:
                    _displayNameLabel.Enabled = !disabledRequisite.Disabled;
                    _valueTrackBar.Enabled    = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }
        
        protected override void HandleTrackBarChanged(object sender, ValueEventArgs<float> e) {
            this.OnValueChanged(new ValueEventArgs<float>(e.Value));
        }

        protected override void RefreshValue(float value) {
            _valueTrackBar.Value = value;
        }

    }
}
