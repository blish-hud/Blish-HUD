using System.Linq;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

namespace Blish_HUD.Settings.UI.Views {
    public static class SettingView {

        private static readonly Logger Logger = Logger.GetLogger(typeof(SettingView));

        public static IView FromType(ISettingEntry setting, int definedWidth) {

            // Check for common types
            var view = setting switch {
                IUiSettingEntry<bool> boolSetting => new BoolSettingView(boolSetting, definedWidth),
                IUiSettingEntry<string> stringSetting => new StringSettingView(stringSetting, definedWidth),
                IUiSettingEntry<float> floatSetting => new FloatSettingView(floatSetting, definedWidth),
                IUiSettingEntry<int> intSetting => new IntSettingView(intSetting, definedWidth),
                IUiSettingEntry<KeyBinding> keyBindingSetting => new KeybindingSettingView(keyBindingSetting, definedWidth),
                ISettingEntry<ISettingCollection> collectionSetting => GetCollectionView(setting.EntryKey, collectionSetting.Value),

                _ => (IView)null
            };

            // Check for additional types
            if (view == null) {
                // Extract the generic type from ISettingsEntry
                var type = setting.GetType();
                if (!type.IsGenericType) {
                    Logger.Warn($"{nameof(SettingCollection)} {setting.EntryKey} was skipped because the setting is not a generic settings type.");
                    return null;
                }
                var genericType = type.GetGenericArguments().FirstOrDefault();
                if (genericType == null) {
                    Logger.Warn($"{nameof(SettingCollection)} {setting.EntryKey} was skipped because the setting has no proper generic type argument.");
                    return null;
                }

                // Check for enum
                if (type.IsEnum) {
                    view = EnumSettingView.FromEnum(setting, definedWidth);
                }
            }

            // Return view
            return view;

            SettingsView GetCollectionView(string collectionKey, ISettingCollection collection) {
                if (!collection.RenderInUi) {
                    Logger.Debug($"{nameof(SettingCollection)} {collectionKey} was skipped because {nameof(SettingCollection.RenderInUi)} was false.");
                    return null;
                }

                return new SettingsView(collection, definedWidth);
            }
        }

    }
}
