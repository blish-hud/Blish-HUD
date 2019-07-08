using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI {
    public static class UpdateUIBuilder {

        public static void BuildUpdateBlishHudSettings(Panel buildPanel, object empty) {
            var currentVersionLabel = new Label() {
                Text           = $"Current Version:  {Program.OverlayVersion}",
                Location       = new Point(25),
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            var latestVersionLabel = new Label() {
                Text           = $"Latest Version:   {Program.OverlayVersion}",
                Location       = new Point(currentVersionLabel.Left, currentVersionLabel.Bottom + 4),
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

        }

    }
}
