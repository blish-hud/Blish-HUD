using Blish_HUD.Graphics.UI;
using Blish_HUD.Controls;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Overlay.UI.Views {
    public class OverlaySettingsView : View {

        protected override void Build(Panel buildPanel) {
            var rootPanel = new FlowPanel() {
                WidthSizingMode  = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection    = ControlFlowDirection.SingleTopToBottom,
                Parent           = buildPanel
            };

            BuildOverlaySettings(rootPanel);
        }

        private ViewContainer GetStandardPanel(Panel rootPanel, string title) {
            return new ViewContainer() {
                WidthSizingMode  = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title            = title,
                ShowBorder       = true,
                Parent           = rootPanel
            };
        }

        private void BuildOverlaySettings(Panel rootPanel) {
            var overlaySettingsRoot = GetStandardPanel(rootPanel, "Overlay Settings");

            overlaySettingsRoot.Show(new SettingsView(GameService.Overlay.OverlaySettings));
        }

    }
}
