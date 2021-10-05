namespace Blish_HUD.Settings.UI.Views {
    using System;

    public abstract class SettingViewFactorySelector : ISettingViewFactorySelector {
        /// <summary>
        /// Gets the default <see cref="ISettingViewFactorySelector"/>.
        /// </summary>
        public static ISettingViewFactorySelector Default { get; } = new DefaultSettingViewFactorySelector();

        /// <inheritdoc/>
        public ISettingViewFactory<T> GetFactoryForType<T>() {
            return (ISettingViewFactory<T>)GetFactoryForType(typeof(T));
        }

        /// <inheritdoc/>
        public abstract ISettingViewFactory GetFactoryForType(Type t);
    }
}
