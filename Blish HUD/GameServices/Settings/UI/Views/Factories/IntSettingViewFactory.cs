using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public class IntSettingViewFactory : SettingViewFactory<int> {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<int> setting, int definedWidth) {
            return new IntSettingView(setting, definedWidth);
        }
    }
}
