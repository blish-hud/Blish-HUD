using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameServices.Module {
    public static class SingleModuleSettingsUIBuilder {

        public static void BuildSingleModuleSettings(Panel buildPanel, object module) {
            if (!(module is ModuleManager cModuleMan)) return;

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
                Location = new Point(0,   moduleText.Bottom - 6),
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

            // Author & Contributors
            if (cModuleMan.Manifest.Author != null) {
                // Author
                var authorImage = new Image() {
                    Texture  = GameService.Content.GetTexture("733268"),
                    Location = new Point(moduleName.Left, moduleName.Bottom),
                    Size     = new Point(32,              32),
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

            } else if (cModuleMan.Manifest.Contributors.Any()) {
                // TODO: Draw out contributors
            }

            // Enable & disable module

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

            enableButton.Click += delegate {
                enableButton.Enabled = false;
                disableButton.Enabled = false;

                cModuleMan.Enabled = true;

                moduleState.Text      = "Loading";
                moduleState.TextColor = Control.StandardColors.Yellow;

                cModuleMan.ModuleInstance.ModuleLoaded += delegate {
                    enableButton.Enabled  = !cModuleMan.Enabled;
                    disableButton.Enabled = cModuleMan.Enabled;

                    moduleState.Text      = cModuleMan.State.Enabled ? "Enabled" : "Disabled";
                    moduleState.TextColor = cModuleMan.State.Enabled ? Color.FromNonPremultiplied(0, 255, 25, 255) : Color.Red;
                };
            };

            disableButton.Click += delegate {
                enableButton.Enabled = false;
                disableButton.Enabled = false;

                cModuleMan.Enabled = false;

                enableButton.Enabled  = !cModuleMan.Enabled;
                disableButton.Enabled = cModuleMan.Enabled;

                moduleState.Text      = cModuleMan.State.Enabled ? "Enabled" : "Disabled";
                moduleState.TextColor = cModuleMan.State.Enabled ? Color.FromNonPremultiplied(0, 255, 25, 255) : Color.Red;
            };

            // Settings Menu
            var settingsMenu = new ContextMenuStrip();

            var settingsButton = new GlowButton() {
                Location = new Point(enableButton.Right + 12, enableButton.Top),

                Icon       = GameService.Content.GetTexture(@"common\157109"),
                ActiveIcon = GameService.Content.GetTexture(@"common\157110"),

                BasicTooltipText = "Options",

                Parent = buildPanel
            };

            settingsButton.Click += delegate { settingsMenu.Show(settingsButton); };

            var viewModuleLogs = settingsMenu.AddMenuItem("View Module Logs");

            if (cModuleMan.Manifest.Directories.Any()) {
                var directoriesMenu = settingsMenu.AddMenuItem("Directories");
                var subDirectoriesMenu = new ContextMenuStrip();

                foreach (var directory in cModuleMan.Manifest.Directories) {
                    subDirectoriesMenu.AddMenuItem($"Explore '{directory}'");
                }

                directoriesMenu.Submenu = subDirectoriesMenu;
            }

            var deleteModule = settingsMenu.AddMenuItem("Delete Module");

            // Collapse Sections

            var collapsePanel = new FlowPanel() {
                Size          = new Point(buildPanel.Width, buildPanel.Height - moduleName.Bottom + 32 + 4),
                Location      = new Point(0,                moduleName.Bottom + 32                     + 4),
                CanScroll     = true,
                Parent        = buildPanel
            };

            // Description

            var descriptionPanel = new Panel() {
                Size       = new Point(collapsePanel.ContentRegion.Width, 155),
                CanScroll  = true,
                Location = new Point(0, moduleName.Bottom + 32 + 4),
                Title      = "Description",
                ShowBorder = true,
                Parent     = collapsePanel
            };

            var descriptionLabel = new Label() {
                Text           = cModuleMan.Manifest.Description,
                Location       = new Point(8, 8),
                Width          = descriptionPanel.Width - 16,
                AutoSizeHeight = true,
                WrapText       = true,
                Parent         = descriptionPanel
            };
        }

    }
}
