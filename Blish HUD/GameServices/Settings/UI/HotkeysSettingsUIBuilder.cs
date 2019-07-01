using Blish_HUD.Controls;

namespace Blish_HUD.GameServices.Settings.UI {
    public static class HotkeysSettingsUIBuilder {

        public static void BuildApplicationHotkeySettings(Panel buildPanel, object nothing) {
            GameService.Settings.RenderSettingsToPanel(buildPanel, GameService.Hotkeys._hotkeySettings.Entries);
        }

    }
}
