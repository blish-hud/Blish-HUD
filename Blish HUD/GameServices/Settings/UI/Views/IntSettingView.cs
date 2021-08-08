namespace Blish_HUD.Settings.UI.Views {
    public class IntSettingView : NumericSettingView<int> {

        public IntSettingView(IUiSettingEntry<int> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }
        
        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            switch (complianceRequisite) {
                case IntRangeRangeComplianceRequisite intRangeRequisite:
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
            this.OnValueChanged(new ValueEventArgs<int>((int)e.Value));
        }

        protected override void RefreshValue(int value) {
            _valueTrackBar.Value = value;
        }

    }
}
