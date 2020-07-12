using System;
using System.Collections.Generic;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

namespace Blish_HUD.Settings.UI.Views {
    public static class SettingView {

        private static readonly Logger Logger = Logger.GetLogger(typeof(SettingView));

        private static readonly Dictionary<Type, Func<SettingEntry, int, IView>> _typeLookup = new Dictionary<Type, Func<SettingEntry, int, IView>> {
            {typeof(bool), (settingEntry,              definedWidth) => new BoolSettingView(settingEntry as SettingEntry<bool>, definedWidth)},
            {typeof(string), (settingEntry,            definedWidth) => new StringSettingView(settingEntry as SettingEntry<string>, definedWidth)},
            {typeof(float), (settingEntry,             definedWidth) => new FloatSettingView(settingEntry as SettingEntry<float>, definedWidth)},
            {typeof(KeyBinding), (settingEntry,        definedWidth) => new KeybindingSettingView(settingEntry as SettingEntry<KeyBinding>, definedWidth)},
            {typeof(SettingCollection), (settingEntry, definedWidth) => new SettingsView(settingEntry as SettingEntry<SettingCollection>, definedWidth)}
        };

        public static IView FromType(SettingEntry setting, int definedWidth) {
            if (_typeLookup.TryGetValue(setting.SettingType, out Func<SettingEntry, int, IView> typeView)) {
                return typeView(setting, definedWidth);
            }

            if (setting.SettingType.IsEnum) {
                return EnumSettingView.FromEnum(setting, definedWidth);
            }

            Logger.Debug($"Setting {setting.DisplayName} [{setting.EntryKey}] of type '{setting.SettingType.FullName}' does not have a renderer available.");

            return null;
        }

    }
}
