using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameServices.UI.Module {
    public static class SingleModuleSettingsUIBuilder {

        public static void BuildSingleModuleSettings(Panel buildPanel, object module) {
            if (module is ModuleManager cModuleMan) {

                var moduleText = new Label() {
                    Text           = "Manage Modules",
                    Location       = new Point(24, 0),
                    AutoSizeHeight = true,
                    AutoSizeWidth  = true,
                    StrokeText     = true,
                    Parent         = buildPanel
                };

                var moduleHeader = new Image() {
                    Texture  = GameService.Content.GetTexture("358411"),
                    Location = new Point(0, moduleText.Bottom - 6),
                    Size     = new Point(875, 110),
                    Parent   = buildPanel
                };

                var moduleName = new Label() {
                    Text           = cModuleMan.Manifest.Name,
                    Font           = GameService.Content.DefaultFont32,
                    AutoSizeHeight = true,
                    AutoSizeWidth  = true,
                    StrokeText     = true,
                    Location       = new Point(moduleText.Left, moduleText.Bottom),
                    Parent         = buildPanel
                };

                var moduleVersion = new Label() {
                    Text              = $"v{cModuleMan.Manifest.Version}",
                    Height            = moduleName.Height - 6,
                    VerticalAlignment = DrawUtil.VerticalAlignment.Bottom,
                    AutoSizeWidth     = true,
                    StrokeText        = true,
                    Font              = GameService.Content.DefaultFont12,
                    Location          = new Point(moduleName.Right + 8, moduleName.Top),
                    Parent            = buildPanel
                };

                var moduleState = new Label() {
                    Text              = cModuleMan.State.Enabled ? "Enabled" : "Disabled",
                    Height            = moduleName.Height - 6,
                    VerticalAlignment = DrawUtil.VerticalAlignment.Bottom,
                    AutoSizeWidth     = true,
                    StrokeText        = true,
                    Font              = GameService.Content.DefaultFont12,
                    TextColor         = cModuleMan.State.Enabled ? Color.FromNonPremultiplied(0, 255, 25, 255) : Color.Red,
                    Location          = new Point(moduleVersion.Right + 8, moduleName.Top),
                    Parent            = buildPanel
                };

                if (cModuleMan.Manifest.Author != null) {

                    var authorImage = new Image() {
                        Texture  = GameService.Content.GetTexture("733268"),
                        Location = new Point(moduleName.Left, moduleName.Bottom),
                        Size     = new Point(32, 32),
                        Parent   = buildPanel
                    };

                    var authorName = new Label() {
                        Text           = cModuleMan.Manifest.Author.Name,
                        Font           = GameService.Content.DefaultFont16,
                        AutoSizeWidth  = true,
                        AutoSizeHeight = true,
                        StrokeText     = true,
                        Parent         = buildPanel
                    };

                    authorName.Location = new Point(authorImage.Right + 2, authorImage.Bottom - authorName.Height);

                    var authoredBy = new Label() {
                        Text              = "Authored by",
                        Height            = authorImage.Height - authorName.Height,
                        AutoSizeWidth     = true,
                        StrokeText        = true,
                        VerticalAlignment = DrawUtil.VerticalAlignment.Bottom,
                        Font              = GameService.Content.DefaultFont12,
                        Location          = new Point(authorImage.Right + 2, authorImage.Top),
                        Parent            = buildPanel
                    };

                }

                var enableButton = new StandardButton() {
                    Location = new Point(buildPanel.Right - 192, moduleHeader.Top + moduleHeader.Height / 4 - StandardButton.STANDARD_CONTROL_HEIGHT / 2),
                    Text     = "Enable Module",
                    Enabled  = !cModuleMan.State.Enabled,
                    Parent   = buildPanel
                };

                var disableButton = new StandardButton() {
                    Location = new Point(buildPanel.Right - 192, enableButton.Bottom + 2),
                    Text     = "Disable Module",
                    Enabled  = cModuleMan.State.Enabled,
                    Parent   = buildPanel
                };
            }
        }

    }
}
