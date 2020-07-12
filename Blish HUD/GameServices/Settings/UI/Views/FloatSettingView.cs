using Blish_HUD.Controls;

namespace Blish_HUD.Settings.UI.Views {
    public class FloatSettingView : NumericSettingView<float> {

        public FloatSettingView(SettingEntry<float> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Panel buildPanel) {
            base.BuildSetting(buildPanel);

            _valueTrackBar.SmallStep = true;
        }

        public override void SetComplianceRequisite(IComplianceRequisite complianceRequisite) {
            if (complianceRequisite is FloatComplianceRequisite floatRequisite) {
                _valueTrackBar.MinValue = floatRequisite.MinValue;
                _valueTrackBar.MaxValue = floatRequisite.MaxValue;
            }
        }
        
        protected override void HandleTrackBarChanged(object sender, ValueEventArgs<float> e) {
            this.OnValueChanged(new ValueEventArgs<float>(_valueTrackBar.Value));
        }

        protected override void RefreshValue(float value) {
            _valueTrackBar.Value = value;
        }

    }
}
