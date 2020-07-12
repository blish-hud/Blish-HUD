using System;
using Blish_HUD.Controls;

namespace Blish_HUD.Settings.UI.Views {
    public class IntSettingView : NumericSettingView<int> {

        public IntSettingView(SettingEntry<int> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }
        
        public override void SetComplianceRequisite(IComplianceRequisite complianceRequisite) {
            if (complianceRequisite is IntComplianceRequisite intRequisite) {
                _valueTrackBar.MinValue = intRequisite.MinValue;
                _valueTrackBar.MaxValue = intRequisite.MaxValue;
            }
        }

        protected override void HandleTrackBarChanged(object sender, ValueEventArgs<float> e) {
            this.OnValueChanged(new ValueEventArgs<int>((int)_valueTrackBar.Value));
        }

        protected override void RefreshValue(int value) {
            _valueTrackBar.Value = value;
        }

    }
}
