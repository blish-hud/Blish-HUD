using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Settings.UI {
    public static class AboutUIBuilder {

        private const string ANET_COPYRIGHT_NOTICE =
            @"©2010–2018 ArenaNet, LLC. All rights reserved. Guild Wars, Guild Wars 2, Heart of Thorns,
            Guild Wars 2: Path of Fire, ArenaNet, NCSOFT, the Interlocking NC Logo, and all associated
            logos and designs are trademarks or registered trademarks of NCSOFT Corporation. All other
            trademarks are the property of their respective owners.";

        private const string ANET_COPYRIGHT_NOTICE_CLEAN =
            "(C) 2010 - 2019 ArenaNet, LLC. All rights reserved. Guild Wars, Guild Wars 2, Heart of Thorns,\n" +
            "Guild Wars 2: Path of Fire, ArenaNet, NCSOFT, the Interlocking NC Logo, and all associated\n"     +
            "logos and designs are trademarks or registered trademarks of NCSOFT Corporation. All other\n"     +
            "trademarks are the property of their respective owners.";

        private const string BLISH_HUD_LOVE_MESSAGE =
            "Designed and built with all the love in Tyria by\nthe Blish HUD team with help from many more!";

        private const string LICENSES_FILE = "licenses.json";

        private const string DISCORD_INVITE = "https://discord.gg/78PYm77";
        private const string SUBREDDIT_URL  = "https://www.reddit.com/r/blishhud";

        public static void BuildAbout(Panel panel, object nothing) {
            var redCornerTint = new Image(GameService.Content.GetTexture("1025164")) {
                SpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
                Location      = new Point(panel.Width - 1024 + 100 - 45, panel.Height - 256 + 100 - 63 + 15),
                ClipsBounds   = false,
                Parent        = panel,
            };

            var gw2CopyrightStatement = new Label() {
                Font                = GameService.Content.DefaultFont16,
                Text                = ANET_COPYRIGHT_NOTICE_CLEAN,
                AutoSizeHeight      = true,
                Width               = panel.Width,
                StrokeText          = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Top,
                Parent              = panel
            };

            gw2CopyrightStatement.Location = new Point(0, panel.Height - gw2CopyrightStatement.Height - 10);

            var lovePanel = new Panel() {
                Size       = new Point(panel.Width - 128, 128),
                Left       = 64,
                Top        = gw2CopyrightStatement.Top - 128 - 24,
                Parent     = panel,
            };

            var heart = new Image(GameService.Content.GetTexture("156127")) {
                Size     = new Point(64, 64),
                Location = new Point(0,  lovePanel.Height / 2 - 32),
                Parent   = lovePanel
            };

            var heartMessage = new Label() {
                Font              = GameService.Content.DefaultFont16,
                Text              = "Designed and built with all the love in Tyria by\nthe Blish HUD team with help from many more!",
                AutoSizeWidth     = true,
                Height            = lovePanel.Height,
                Left              = heart.Right,
                VerticalAlignment = VerticalAlignment.Middle,
                StrokeText        = true,
                Parent            = lovePanel
            };
        }

    }
}
