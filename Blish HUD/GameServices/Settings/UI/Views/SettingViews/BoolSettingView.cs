using Blish_HUD.Common;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views.SettingViews {
    public class BoolSettingView : SettingView<bool> {

        private const int CONTROL_PADDING = 5;

        private Checkbox _boolCheckbox;

        /// <inheritdoc />
        public BoolSettingView(SettingEntry<bool> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        /// <inheritdoc />
        protected override void BuildSetting(Panel buildPanel) {
            _boolCheckbox = new Checkbox() {
                Location = new Point(CONTROL_PADDING, CONTROL_PADDING),
                Parent   = buildPanel
            };

            _boolCheckbox.CheckedChanged += BoolCheckboxOnCheckedChanged;
        }

        private void BoolCheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            OnValueChanged(new EventValueArgs<bool>(e.Checked));
        }

        private void UpdateSize() {
            this.ViewTarget.Size = new Point(_boolCheckbox.Right + CONTROL_PADDING, _boolCheckbox.Bottom + CONTROL_PADDING);
        }

        /// <inheritdoc />
        protected override void RefreshDisplayName(string displayName) {
            _boolCheckbox.Text = displayName;

            UpdateSize();
        }

        /// <inheritdoc />
        protected override void RefreshDescription(string description) {
            _boolCheckbox.BasicTooltipText = description;
        }

        /// <inheritdoc />
        protected override void RefreshValue(bool value) {
            _boolCheckbox.Checked = value;
        }

        /// <inheritdoc />
        protected override void Unload() {
            _boolCheckbox.CheckedChanged -= BoolCheckboxOnCheckedChanged;
        }

    }
}
