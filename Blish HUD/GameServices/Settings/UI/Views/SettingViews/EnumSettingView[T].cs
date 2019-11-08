using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Common;
using Blish_HUD.Controls;
using Microsoft.Scripting.Utils;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views.SettingViews {

    public class EnumSettingView<TEnum> : SettingView<TEnum> where TEnum : struct, Enum {

        private const int CONTROL_SIDEPADDING = 5;

        private Label    _displayNameLabel;
        private Dropdown _enumDropdown;

        private TEnum[] _enumValues;

        /// <inheritdoc />
        public EnumSettingView(SettingEntry<TEnum> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            progress.Report("Loading setting values...");
            _enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
            progress.Report(string.Empty);

            return base.Load(progress);
        }

        /// <inheritdoc />
        protected override void BuildSetting(Panel buildPanel) {
            _displayNameLabel = new Label {
                AutoSizeWidth = true,
                Location      = new Point(CONTROL_SIDEPADDING, 0),
                Parent        = buildPanel
            };

            _enumDropdown = new Dropdown {
                Size   = new Point(250, 27),
                Parent = buildPanel
            };

            _enumDropdown.Items.AddRange(_enumValues.Select(e => e.ToString()));

            _enumDropdown.ValueChanged += EnumDropdownOnValueChanged;
        }

        private void EnumDropdownOnValueChanged(object sender, ValueChangedEventArgs e) {
            if (Enum.TryParse(e.CurrentValue, true, out TEnum value)) {
                OnValueChanged(new EventValueArgs<TEnum>(value));
            } else {
                _enumDropdown.SelectedItem = this.Value.ToString();
            }
        }

        private void UpdateSizeAndLayout() {
            this.ViewTarget.Height   = _enumDropdown.Bottom;
            _displayNameLabel.Height = this.ViewTarget.Height;

            if (this.DefinedWidth > 0) {
                _enumDropdown.Left = _displayNameLabel.Right + CONTROL_SIDEPADDING;

                this.ViewTarget.Width = _enumDropdown.Right + CONTROL_SIDEPADDING;
            } else {
                _enumDropdown.Location = new Point(this.ViewTarget.Width - CONTROL_SIDEPADDING - 250, 0);
            }
        }

        /// <inheritdoc />
        protected override void RefreshDisplayName(string displayName) {
            _displayNameLabel.Text = displayName;

            UpdateSizeAndLayout();
        }

        /// <inheritdoc />
        protected override void RefreshDescription(string description) {
            _enumDropdown.BasicTooltipText = description;
        }

        /// <inheritdoc />
        protected override void RefreshValue(TEnum value) {
            _enumDropdown.SelectedItem = value.ToString();
        }

    }

}