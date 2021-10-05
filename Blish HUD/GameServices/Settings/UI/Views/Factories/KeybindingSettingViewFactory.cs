using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

namespace Blish_HUD.Settings.UI.Views {
    public class KeybindingSettingViewFactory : SettingViewFactory<KeyBinding> {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<KeyBinding> setting, int definedWidth) {
            return new KeybindingSettingView(setting, definedWidth);
        }
    }
}
