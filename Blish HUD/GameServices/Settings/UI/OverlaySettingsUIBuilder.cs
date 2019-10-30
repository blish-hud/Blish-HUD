using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI {
    public static class OverlaySettingsUIBuilder {

        public static void BuildOverlaySettings(Panel buildPanel, object nothing) {
            var settingPanels = new FlowPanel() {
                Size        = buildPanel.ContentRegion.Size,
                ShowBorder  = true,
                Parent      = buildPanel
            };

            var applicationSettingsPanel = new Panel() {
                Width       = settingPanels.ContentRegion.Width - 8,
                Title       = "General",
                CanCollapse = true,
                Height      = 168,
                Parent      = settingPanels,
            };

            var applicationSettingsPadding = new Panel() {
                Size     = applicationSettingsPanel.Size - new Point(25),
                Location = new Point(25, 12),
                Parent   = applicationSettingsPanel
            };

            var updatePanel = new Panel() {
                Width       = settingPanels.ContentRegion.Width - 8,
                Title       = "Updates",
                Height      = 128,
                CanCollapse = true,
                Collapsed   = false,
                Parent      = settingPanels
            };

            var audioSettingsPanel = new Panel() {
                Width       = settingPanels.ContentRegion.Width - 8,
                Title       = "Volume Settings",
                Height      = 128,
                CanCollapse = true,
                Collapsed   = false,
                Parent      = settingPanels
            };

            GameService.Settings.RenderSettingsToPanel(applicationSettingsPadding, GameService.Overlay._applicationSettings.Entries);

            UpdateUIBuilder.BuildUpdateBlishHudSettings(updatePanel, null);
        }

    }

}
