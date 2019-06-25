using Blish_HUD.Controls;

namespace Blish_HUD.GameServices.Overlay {
    public static class OverlaySettingsUIBuilder {

        public static void BuildSingleModuleSettings(Panel buildPanel, object nothing) {
            GameService.Settings.RenderSettingsToPanel(buildPanel, GameService.Overlay._applicationSettings.Entries);
        }

    }

}
