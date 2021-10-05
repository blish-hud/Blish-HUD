using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public delegate IView SettingViewFactoryDelegate(SettingEntry setting, int definedWidth);

    public abstract class SettingViewFactory : ISettingViewFactory {
        /// <inheritdoc/>
        public abstract IView CreateView(SettingEntry setting, int definedWidth);

        /// <summary>
        /// Creates an <see cref="ISettingViewFactory"/>, implemented by the <paramref name="factoryDelegate"/> provided.
        /// </summary>
        /// <param name="factoryDelegate">The delegate used to create the view.</param>
        /// <returns>A new <see cref="ISettingViewFactory"/> instance.</returns>
        public static ISettingViewFactory Create(SettingViewFactoryDelegate factoryDelegate) {
            return new InlineSettingViewFactory(factoryDelegate);
        }
    }
}
