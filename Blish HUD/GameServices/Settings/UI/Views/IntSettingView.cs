using Blish_HUD.Controls;
using System;

namespace Blish_HUD.Settings.UI.Views {
    public class IntSettingView : NumericSettingView<int> {

        protected const int INPUT_WIDTH = 80;

        private NumberInput _numberInput;

        public IntSettingView(SettingEntry<int> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Container buildPanel) {
            base.BuildSetting(buildPanel);

            _numberInput = new NumberInput() {
                Width       = INPUT_WIDTH,
                Left        = TRACKBAR_LEFT,
                ClipsBounds = false, // bit of a hack to allow overflowing text for large numbers
                MinValue    = (int)_valueTrackBar.MinValue,
                MaxValue    = (int)_valueTrackBar.MaxValue,
                Parent      = buildPanel
            };

            _valueTrackBar.Top  = (_numberInput.Height - _valueTrackBar.Height) / 2; // Center with number input
            _valueTrackBar.Left = _numberInput.Right + CONTROL_PADDING;

            _numberInput.ValueChanged += HandleNumberInputChanged;
        }
        
        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            switch (complianceRequisite) {
                case IntRangeRangeComplianceRequisite intRangeRequisite:
                    _valueTrackBar.MinValue = intRangeRequisite.MinValue;
                    _valueTrackBar.MaxValue = intRangeRequisite.MaxValue;
                    _numberInput.MinValue   = intRangeRequisite.MinValue;
                    _numberInput.MaxValue   = intRangeRequisite.MaxValue;
                    break;
                case SettingDisabledComplianceRequisite disabledRequisite:
                    _displayNameLabel.Enabled = !disabledRequisite.Disabled;
                    _numberInput.Enabled      = !disabledRequisite.Disabled;
                    _valueTrackBar.Enabled    = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void HandleTrackBarChanged(object sender, ValueEventArgs<float> e) {
            _numberInput.Value = (int)e.Value;
            this.OnValueChanged(new ValueEventArgs<int>((int)e.Value));
        }

        private void HandleNumberInputChanged(object sender, EventArgs e) {
            _valueTrackBar.Value = _numberInput.Value;
            this.OnValueChanged(new ValueEventArgs<int>(_numberInput.Value));
        }

        protected override void RefreshValue(int value) {
            // Prevent us clamping the setting value before compliance is applied
            _valueTrackBar.MinValue = Math.Min(_valueTrackBar.MinValue, value);
            _valueTrackBar.MaxValue = Math.Max(_valueTrackBar.MaxValue, value);
            _numberInput.MinValue   = Math.Min(_numberInput.MinValue, value);
            _numberInput.MaxValue   = Math.Max(_numberInput.MaxValue, value);

            _valueTrackBar.Value = value;
            _numberInput.Value   = value;
        }

        protected override void RefreshDescription(string description) {
            base.RefreshDescription(description);
            _numberInput.BasicTooltipText = description;
        }

    }
}
