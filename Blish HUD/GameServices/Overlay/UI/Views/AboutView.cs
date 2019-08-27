using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Presenters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Overlay.UI.Views {
    public class AboutView : View<AboutPresenter> {

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

        public AboutView() {
            this.Presenter = new AboutPresenter(this, null);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            var redCornerTint = new Image(GameService.Content.GetTexture("1025164")) {
                SpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
                Location      = new Point(buildPanel.Width - 1024 + 100 - 45, buildPanel.Height - 256 + 100 - 63 + 15),
                ClipsBounds   = false,
                Parent        = buildPanel
            };

            var gw2CopyrightStatement = new Label() {
                Font                = GameService.Content.DefaultFont16,
                Text                = ANET_COPYRIGHT_NOTICE_CLEAN,
                AutoSizeHeight      = true,
                Width               = buildPanel.Width,
                StrokeText          = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Top,
                Parent              = buildPanel
            };

            gw2CopyrightStatement.Location = new Point(0, buildPanel.Height - gw2CopyrightStatement.Height - 10);

            var lovePanel = new Panel() {
                Size   = new Point(buildPanel.Width - 128, 128),
                Left   = 64,
                Top    = gw2CopyrightStatement.Top - 128 - 24,
                Parent = buildPanel
            };

            var heart = new Image(GameService.Content.GetTexture("156127")) {
                Size     = new Point(64, 64),
                Location = new Point(0,  lovePanel.Height / 2 - 32),
                Parent   = lovePanel
            };

            var heartMessage = new Label() {
                Font              = GameService.Content.DefaultFont16,
                Text              = BLISH_HUD_LOVE_MESSAGE,
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
