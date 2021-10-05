using System;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public class EnumSettingViewFactory<TEnum> : SettingViewFactory<TEnum>
        where TEnum : struct, Enum {
        /// <inheritdoc/>
        public override IView CreateView(SettingEntry<TEnum> setting, int definedWidth) {
            return new EnumSettingView<TEnum>(setting, definedWidth);
        }
    }
}
