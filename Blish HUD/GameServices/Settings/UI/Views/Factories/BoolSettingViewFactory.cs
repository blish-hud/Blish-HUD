using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public class BoolSettingViewFactory : SettingViewFactory<bool> {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<bool> setting, int definedWidth) {
            return new BoolSettingView(setting, definedWidth);
        }
    }
}
