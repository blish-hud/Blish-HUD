
using System;
using Blish_HUD.Common;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views.SettingViews {
    public class StringSettingView : SettingView<string> {

        private const int CONTROL_SIDEPADDING = 5;

        private Label   _displayNameLabel;
        private TextBox _stringTextBox;

        /// <inheritdoc />
        public StringSettingView(SettingEntry<string> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        /// <inheritdoc />
        protected override void BuildSetting(Panel buildPanel) {
            _displayNameLabel = new Label() {
                AutoSizeWidth = true,
                Location      = new Point(CONTROL_SIDEPADDING, 0),
                Parent        = buildPanel
            };

            _stringTextBox = new TextBox() {
                Size   = new Point(250, 27),
                Parent = buildPanel
            };

            _stringTextBox.TextChanged += StringTextBoxOnTextChanged;
        }

        private void StringTextBoxOnTextChanged(object sender, EventArgs e) {
            OnValueChanged(new EventValueArgs<string>(_stringTextBox.Text));
        }

        private void UpdateSizeAndLayout() {
            _displayNameLabel.Height = _stringTextBox.Bottom;

            if (this.DefinedWidth > 0) {
                _stringTextBox.Left = _displayNameLabel.Right + CONTROL_SIDEPADDING;

                this.ViewTarget.Width = _stringTextBox.Right + CONTROL_SIDEPADDING;
            } else {
                _stringTextBox.Location = new Point(this.ViewTarget.Width - CONTROL_SIDEPADDING - 250, 0);
            }
        }

        /// <inheritdoc />
        protected override void RefreshDisplayName(string displayName) {
            _displayNameLabel.Text = displayName;

            UpdateSizeAndLayout();
        }

        /// <inheritdoc />
        protected override void RefreshDescription(string description) {
            _stringTextBox.BasicTooltipText = description;
        }

        /// <inheritdoc />
        protected override void RefreshValue(string value) {
            _stringTextBox.Text = value;
        }

        /// <inheritdoc />
        protected override void Unload() {
            _stringTextBox.TextChanged -= StringTextBoxOnTextChanged;
        }

    }
}
