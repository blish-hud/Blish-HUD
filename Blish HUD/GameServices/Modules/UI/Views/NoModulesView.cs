using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public class NoModulesView : View {

        protected override void Build(Panel buildPanel) {
            var info = new Label() {
                Size                = buildPanel.Size / new Point(1, 2),
                Parent              = buildPanel,
                Text                = Strings.GameServices.ModulesService.NoModules_Info,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Middle,
                Font                = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Italic),
            };

            var openDir = new StandardButton() {
                Text     = Strings.GameServices.ModulesService.NoModules_OpenFolder,
                Parent   = buildPanel,
                Width    = 200,
                Location = new Point(buildPanel.Size.X / 2 - 100, info.Bottom - 100),
            };
        }

    }
}
