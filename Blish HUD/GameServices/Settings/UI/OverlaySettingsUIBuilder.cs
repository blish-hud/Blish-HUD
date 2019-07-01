using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameServices.Settings.UI {
    public static class OverlaySettingsUIBuilder {

        public static void BuildSingleModuleSettings(Panel buildPanel, object nothing) {
            buildPanel.ShowBorder = true;

            var applicationSettingsPanel = new Panel() {
                Width       = buildPanel.ContentRegion.Width,
                Title       = "Application Settings",
                CanCollapse = true,
                Parent      = buildPanel,
                Height      = 128
            };

            var applicationSettingsPadding = new Panel() {
                Size     = applicationSettingsPanel.Size - new Point(25),
                Location = new Point(25, 12),
                Parent   = applicationSettingsPanel
            };

            var updatePanel = new Panel() {
                Width       = buildPanel.ContentRegion.Width,
                Title       = "Updates",
                Height      = 128,
                CanCollapse = true,
                Collapsed   = true,
                Parent      = buildPanel
            };

            var audioSettingsPanel = new Panel() {
                Width       = buildPanel.ContentRegion.Width,
                Title       = "Volume Settings",
                Height      = 128,
                CanCollapse = true,
                Collapsed   = true,
                Parent      = buildPanel
            };

            Adhesive.Binding.CreateOneWayBinding(() => updatePanel.Top,        () => applicationSettingsPanel.Bottom, (h) => h, true);
            Adhesive.Binding.CreateOneWayBinding(() => audioSettingsPanel.Top, () => updatePanel.Bottom,              (h) => h, true);

            GameService.Settings.RenderSettingsToPanel(applicationSettingsPadding, GameService.Overlay._applicationSettings.Entries);

            UpdateUIBuilder.BuildUpdateBlishHudSettings(updatePanel, null);
        }

    }

}
