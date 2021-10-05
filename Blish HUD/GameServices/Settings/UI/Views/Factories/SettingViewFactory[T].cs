using System;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public delegate IView SettingViewFactoryDelegate<T>(SettingEntry<T> setting, int definedWidth);

    public abstract class SettingViewFactory<T> : SettingViewFactory, ISettingViewFactory<T> {
        private static Lazy<ISettingViewFactory<T>> _lazyDefault = new Lazy<ISettingViewFactory<T>>(() => SettingViewFactorySelector.Default.GetFactoryForType<T>());

        /// <summary>
        /// Gets or creates the default view factory for type <typeparamref name="T"/>
        /// </summary>
        /// <returns>The <see cref="ISettingViewFactory{T}"/> instance, or <see langword="null"/> if no default factory for <typeparamref name="T"/> was found.</returns>
        public static ISettingViewFactory<T> Default => _lazyDefault.Value;

        /// <summary>
        /// Creates an <see cref="ISettingViewFactory{T}"/>, implemented by the <paramref name="factoryDelegate"/> provided.
        /// </summary>
        /// <param name="factoryDelegate">The delegate used to create the view.</param>
        /// <returns>A new <see cref="ISettingViewFactory{T}"/> instance.</returns>
        public static ISettingViewFactory<T> Create(SettingViewFactoryDelegate<T> factoryDelegate) {
            return new InlineSettingViewFactory<T>(factoryDelegate);
        }

        /// <inheritdoc/>
        public abstract IView CreateView(SettingEntry<T> setting, int definedWidth);

        /// <inheritdoc/>
        public override IView CreateView(SettingEntry setting, int definedWidth) {
            return CreateView((SettingEntry<T>)setting, definedWidth);
        }
    }
}
