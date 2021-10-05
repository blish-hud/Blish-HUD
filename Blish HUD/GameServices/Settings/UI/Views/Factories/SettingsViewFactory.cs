using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public class SettingsViewFactory : SettingViewFactory<SettingCollection> {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<SettingCollection> setting, int definedWidth) {
            return new SettingsView(setting, definedWidth);
        }
    }
}
