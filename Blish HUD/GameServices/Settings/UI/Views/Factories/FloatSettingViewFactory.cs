using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public class FloatSettingViewFactory : SettingViewFactory<float> {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<float> setting, int definedWidth) {
            return new FloatSettingView(setting, definedWidth);
        }
    }
}
