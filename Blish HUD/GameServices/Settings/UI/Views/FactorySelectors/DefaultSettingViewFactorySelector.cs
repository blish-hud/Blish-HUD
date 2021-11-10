namespace Blish_HUD.Settings.UI.Views {
    using Blish_HUD.Settings;
    using System;

    /// <summary>
    /// The default <see cref="ISettingViewFactorySelector"/> implementation.
    /// This class can be inherited from to provide the default views as fallbacks.
    /// </summary>
    public class DefaultSettingViewFactorySelector : SettingViewFactorySelector {
        private static readonly ISettingViewFactory defaultBoolSelector = new BoolSettingViewFactory();
        private static readonly ISettingViewFactory defaultStringSelector = new StringSettingViewFactory();
        private static readonly ISettingViewFactory defaultFloatSelector = new FloatSettingViewFactory();
        private static readonly ISettingViewFactory defaultIntSelector = new IntSettingViewFactory();
        private static readonly ISettingViewFactory defaultSettingsSelector = new SettingsViewFactory();

        /// <inheritdoc/>
        public override ISettingViewFactory GetFactoryForType(Type t) {
            if (t.IsEnum) {
                Type enumFactoryType = typeof(EnumSettingViewFactory<>).MakeGenericType(t);
                return (ISettingViewFactory)Activator.CreateInstance(enumFactoryType);
            }

            if (t == typeof(bool)) {
                return defaultBoolSelector;
            }

            if (t == typeof(string)) {
                return defaultStringSelector;
            }

            if (t == typeof(float)) {
                return defaultFloatSelector;
            }

            if (t == typeof(int)) {
                return defaultIntSelector;
            }

            if (t == typeof(SettingCollection)) {
                return defaultSettingsSelector;
            }

            return null;
        }
    }
}
