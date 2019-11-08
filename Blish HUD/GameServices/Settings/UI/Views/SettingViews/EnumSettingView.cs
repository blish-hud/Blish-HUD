using System;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views.SettingViews {

    public static class EnumSettingView {

        public static IView FromEnum(SettingEntry setting, int definedWidth = -1) {
            var specificEnumType = typeof(EnumSettingView<>).MakeGenericType(setting.SettingType);

            return Activator.CreateInstance(specificEnumType, setting, definedWidth) as IView;
        }

    }

}
