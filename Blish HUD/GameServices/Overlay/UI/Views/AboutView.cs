using Blish_HUD.Graphics.UI;
using System;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Overlay.UI.Views {
    public class AboutView : View {

        protected override void Build(Container viewTarget) {
            _ = new Image(GameService.Content.GetTexture("1025164")) {
                SpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
                Location = new Point(viewTarget.Width - 1024 + 100 - 45, viewTarget.Height - 256 + 100 - 63 + 15),
                ClipsBounds = false,
                Parent = viewTarget
            };

            var gw2CopyrightStatement = new Label() {
                Font = GameService.Content.DefaultFont16,
                Text = string.Format(Strings.GameServices.OverlayService.AboutAnetNotice, DateTime.Now.Year),
                AutoSizeHeight = true,
                Width = viewTarget.Width,
                StrokeText = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Parent = viewTarget
            };

            gw2CopyrightStatement.Location = new Point(0, viewTarget.Height - gw2CopyrightStatement.Height - 48);

            var lovePanel = new Panel() {
                Size = new Point(viewTarget.Width - 128, 128),
                Left = 64,
                Top = gw2CopyrightStatement.Top - 128 - 12,
                Parent = viewTarget
            };

            var heart = new Image(GameService.Content.GetTexture("156127")) {
                Size = new Point(64, 64),
                Location = new Point(0, lovePanel.Height / 2 - 32),
                Parent = lovePanel
            };

            _ = new Label() {
                Font = GameService.Content.DefaultFont16,
                Text = Strings.GameServices.OverlayService.AboutLoveMessage,
                AutoSizeWidth = true,
                Height = lovePanel.Height,
                Left = heart.Right,
                VerticalAlignment = VerticalAlignment.Middle,
                StrokeText = true,
                Parent = lovePanel
            };

            var version = new Label() {
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Text = $"Blish HUD v{Program.OverlayVersion}",
                Font = GameService.Content.DefaultFont14,
                StrokeText = true,
                ClipsBounds = false,
                Parent = viewTarget
            };

            version.Location = new Point(viewTarget.Width - version.Width + 8, viewTarget.Height - version.Height + 24);
        }

    }
}
