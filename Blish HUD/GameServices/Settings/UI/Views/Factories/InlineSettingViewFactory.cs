using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    internal sealed class InlineSettingViewFactory : SettingViewFactory {
        private readonly SettingViewFactoryDelegate _factoryDelegate;

        internal InlineSettingViewFactory(SettingViewFactoryDelegate factoryDelegate) {
            this._factoryDelegate = factoryDelegate;
        }

        /// <inheritdoc/>
        public override IView CreateView(SettingEntry setting, int definedWidth) {
            return _factoryDelegate(setting, definedWidth);
        }
    }
}
