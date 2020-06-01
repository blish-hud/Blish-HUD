using System;
using Blish_HUD.Controls;
using Blish_HUD.Input;

namespace Blish_HUD.Settings.UI.Views {
    public class KeybindingSettingView : SettingView<KeyBinding> {

        private KeybindingAssigner _keybindingAssigner;

        public KeybindingSettingView(SettingEntry<KeyBinding> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Panel buildPanel) {
            _keybindingAssigner = new KeybindingAssigner() {
                Parent = buildPanel
            };

            buildPanel.HeightSizingMode = SizingMode.AutoSize;
            buildPanel.WidthSizingMode  = SizingMode.AutoSize;

            _keybindingAssigner.BindingChanged += KeybindingAssignerOnBindingChanged;
        }

        private void KeybindingAssignerOnBindingChanged(object sender, EventArgs e) {
            this.OnValueChanged(new ValueEventArgs<KeyBinding>(_keybindingAssigner.KeyBinding));
        }

        protected override void RefreshDisplayName(string displayName) {
            _keybindingAssigner.KeyBindingName = displayName;
        }

        protected override void RefreshDescription(string description) {
            _keybindingAssigner.BasicTooltipText = description;
        }

        protected override void RefreshValue(KeyBinding value) {
            _keybindingAssigner.KeyBinding = value;
        }

    }
}
