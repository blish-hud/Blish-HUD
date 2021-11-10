using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public class StringSettingViewFactory : SettingViewFactory<string>
    {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<string> setting, int definedWidth) {
            return new StringSettingView(setting, definedWidth);
        }
    }
}
