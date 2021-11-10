using System;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public static class SettingView {

        private static readonly Logger Logger = Logger.GetLogger(typeof(SettingView));

        [Obsolete("Use SettingViewFactory / SettingViewFactorySelector instead.")]
        public static IView FromType(SettingEntry setting, int definedWidth) {
            if (setting is SettingEntry<SettingCollection> settingCollection && !settingCollection.Value.RenderInUi) {
                Logger.Debug($"{nameof(SettingCollection)} {setting.EntryKey} was skipped because {nameof(SettingCollection.RenderInUi)} was false.");
                return null;
            }

            ISettingViewFactory viewFactory = SettingViewFactorySelector.Default.GetFactoryForType(setting.SettingType);
            IView view = viewFactory?.CreateView(setting, definedWidth);

            if (view == null) {
                Logger.Debug($"Setting {setting.DisplayName} [{setting.EntryKey}] of type '{setting.SettingType.FullName}' does not have a default renderer available.");
            }

            return null;
        }

    }
}
