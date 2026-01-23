using System;
using Blish_HUD.Controls;

namespace Blish_HUD.Settings.UI.Views {
    public class FloatSettingView : NumericSettingView<float> {

        protected const int INPUT_WIDTH = 110;

        private FloatInput _floatInput;

        public FloatSettingView(SettingEntry<float> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Container buildPanel) {
            base.BuildSetting(buildPanel);

            _valueTrackBar.SmallStep = true;

            _floatInput = new FloatInput() {
                Width           = INPUT_WIDTH,
                Left            = TRACKBAR_LEFT,
                MinValue        = _valueTrackBar.MinValue,
                MaxValue        = _valueTrackBar.MaxValue,
                IncrementAmount = 0.01f, // Default increment, will be overridden by compliance if applicable
                FormatString    = "F2",  // Default format, will be overridden by compliance if applicable
                Parent          = buildPanel
            };

            _valueTrackBar.Top    = (_floatInput.Height - _valueTrackBar.Height) / 2; // Center with number input
            _valueTrackBar.Left   = _floatInput.Right + CONTROL_PADDING;
            _valueTrackBar.Width -= _floatInput.Width + CONTROL_PADDING;

            _floatInput.ValueChanged += HandleFloatInputChanged;
        }

        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            switch (complianceRequisite) {
                case FloatRangeRangeComplianceRequisite floatRangeRequisite:
                    _valueTrackBar.MinValue = floatRangeRequisite.MinValue;
                    _valueTrackBar.MaxValue = floatRangeRequisite.MaxValue;
                    _floatInput.MinValue = floatRangeRequisite.MinValue;
                    _floatInput.MaxValue = floatRangeRequisite.MaxValue;
                    _floatInput.IncrementAmount = floatRangeRequisite.IncrementAmount;
                    _floatInput.FormatString = floatRangeRequisite.GetFormatString();
                    break;
                case SettingDisabledComplianceRequisite disabledRequisite:
                    _displayNameLabel.Enabled = !disabledRequisite.Disabled;
                    _floatInput.Enabled = !disabledRequisite.Disabled;
                    _valueTrackBar.Enabled = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void HandleTrackBarChanged(object sender, ValueEventArgs<float> e) {
            _floatInput.Value = e.Value;
            this.OnValueChanged(new ValueEventArgs<float>(e.Value));
        }

        private void HandleFloatInputChanged(object sender, EventArgs e) {
            _valueTrackBar.Value = _floatInput.Value;
            this.OnValueChanged(new ValueEventArgs<float>(_floatInput.Value));
        }

        protected override void RefreshValue(float value) {
            // Prevent us clamping the setting value before compliance is applied
            _valueTrackBar.MinValue = Math.Min(_valueTrackBar.MinValue, value);
            _valueTrackBar.MaxValue = Math.Max(_valueTrackBar.MaxValue, value);
            _floatInput.MinValue = Math.Min(_floatInput.MinValue, value);
            _floatInput.MaxValue = Math.Max(_floatInput.MaxValue, value);

            _valueTrackBar.Value = value;
            _floatInput.Value = value;
        }

        protected override void RefreshDescription(string description) {
            base.RefreshDescription(description);
            _floatInput.BasicTooltipText = description;
        }

    }
}