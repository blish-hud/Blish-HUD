using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    internal sealed class InlineSettingViewFactory<T> : SettingViewFactory<T> {
        private readonly SettingViewFactoryDelegate<T> _factoryDelegate;

        internal InlineSettingViewFactory(SettingViewFactoryDelegate<T> factoryDelegate) {
            this._factoryDelegate = factoryDelegate;
        }

        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<T> setting, int definedWidth) {
            return _factoryDelegate(setting, definedWidth);
        }
    }
}
