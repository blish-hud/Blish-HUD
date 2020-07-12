using System;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public class FloatSettingView : SettingView<float> {

        private const int CONTROL_PADDING = 5;

        private const int TRACKBAR_WIDTH  = 256;
        private const int TRACKBAR_HEIGHT = 16;

        private Label    _displayNameLabel;
        private TrackBar _floatTrackBar;

        public FloatSettingView(SettingEntry<float> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Panel buildPanel) {
            _displayNameLabel = new Label() {
                AutoSizeWidth = true,
                Location      = new Point(CONTROL_PADDING, 0),
                Parent        = buildPanel
            };

            _floatTrackBar = new TrackBar() {
                Size   = new Point(TRACKBAR_WIDTH, TRACKBAR_HEIGHT),
                Parent = buildPanel
            };

            _floatTrackBar.ValueChanged += FloatTrackBarOnValueChanged;
        }

        private void FloatTrackBarOnValueChanged(object sender, EventArgs e) {
            this.OnValueChanged(new ValueEventArgs<float>(_floatTrackBar.Value));
        }

        private void UpdateSizeAndLayout() {
            this.ViewTarget.Height   = _floatTrackBar.Bottom;
            _displayNameLabel.Height = this.ViewTarget.Height;

            if (this.DefinedWidth > 0) {
                _floatTrackBar.Left   = _displayNameLabel.Right + CONTROL_PADDING;
                this.ViewTarget.Width = _floatTrackBar.Right    + CONTROL_PADDING;
            } else {
                _floatTrackBar.Location = new Point(this.ViewTarget.Width - CONTROL_PADDING - TRACKBAR_WIDTH, 0);
            }
        }

        protected override void RefreshDisplayName(string displayName) {
            _displayNameLabel.Text = displayName;

            UpdateSizeAndLayout();
        }

        protected override void RefreshDescription(string description) {
            _floatTrackBar.BasicTooltipText = description;
        }

        protected override void RefreshValue(float value) {
            _floatTrackBar.Value = value;
        }

        public override void SetComplianceRequisite(IComplianceRequisite complianceRequisite) {
            if (complianceRequisite is FloatComplianceRequisite floatRequisite) {
                _floatTrackBar.MinValue = (int)floatRequisite.MinValue;
                _floatTrackBar.MaxValue = (int)floatRequisite.MaxValue;
            }
        }

    }
}
