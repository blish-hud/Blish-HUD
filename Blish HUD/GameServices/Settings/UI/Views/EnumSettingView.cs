using System;
using System.Linq;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public static class EnumSettingView {

        public static IView FromEnum(ISettingEntry setting, int definedWidth = -1) {
            // Extract the generic type from ISettingsEntry
            var type = setting.GetType();
            if (!type.IsGenericType) {
                return null;
            }
            var genericType = type.GetGenericArguments().FirstOrDefault();
            if (genericType == null || !genericType.IsEnum) {
                return null;
            }
            
            var specificEnumType = typeof(EnumSettingView<>).MakeGenericType(genericType);

            return Activator.CreateInstance(specificEnumType, setting, definedWidth) as IView;
        }

        public static IView FromEnum<T>(ISettingEntry<T> setting, int definedWidth = -1) where T : struct, Enum {
            return Activator.CreateInstance(typeof(EnumSettingView<T>), setting, definedWidth) as IView;
        }

    }
}
