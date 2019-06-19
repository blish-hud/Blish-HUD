using Blish_HUD.Controls;

namespace Blish_HUD.GameServices.Director {
    public static class ApplicationSettingsUIBuilder {

        public static void BuildSingleModuleSettings(Panel buildPanel, object nothing) {
            GameService.Settings.RenderSettingsToPanel(buildPanel, GameService.Director._applicationSettings.Entries);
        }

    }

}
