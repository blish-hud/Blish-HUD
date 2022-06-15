using Blish_HUD.Graphics.UI;
using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Overlay.UI.Presenters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Overlay.UI.Views {
    public class AboutView : View {

        protected override void Build(Container buildPanel) {
            _ = new Image(AsyncTexture2D.FromAssetId(1025164)) {
                SpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
                Location      = new Point(buildPanel.Width - 969, buildPanel.Height - 220),
                ClipsBounds   = false,
                Parent        = buildPanel
            };

            var gw2CopyrightStatement = new Label() {
                Font                = GameService.Content.DefaultFont16,
                Text                = string.Format(Strings.GameServices.OverlayService.AboutAnetNotice, DateTime.Now.Year),
                AutoSizeHeight      = true,
                Width               = buildPanel.Width,
                StrokeText          = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Top,
                Parent              = buildPanel
            };

            gw2CopyrightStatement.Location = new Point(0, buildPanel.Height - gw2CopyrightStatement.Height - 64);

            var lovePanel = new Panel() {
                Size = new Point(buildPanel.Width - 128, 128),
                Left = 64,
                Top = gw2CopyrightStatement.Top - 128 - 12,
                Parent = buildPanel
            };

            var heart = new Image(AsyncTexture2D.FromAssetId(156127)) {
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
                AutoSizeHeight   = true,
                AutoSizeWidth    = true,
                Text             = $"{Strings.Common.BlishHUD} v{Program.OverlayVersion.BaseAndPrerelease()}",
                BasicTooltipText = $"v{Program.OverlayVersion}",
                Font             = GameService.Content.DefaultFont14,
                StrokeText       = true,
                ClipsBounds      = false,
                Parent           = buildPanel
            };

            version.Location = new Point(buildPanel.Width - version.Width + 8, buildPanel.Height - version.Height);

            var mumbleConnection = new ViewContainer() {
                Size   = new Point(128, 20),
                Left   = 8,
                Bottom = version.Bottom,
                Parent = buildPanel
            };

            var arcdpsBridgeConnection = new ViewContainer() {
                Size   = new Point(128, 20),
                Bottom = version.Bottom,
                Left   = mumbleConnection.Right,
                Parent = buildPanel
            };

            mumbleConnection.Show(new ConnectionStatusView().WithPresenter(new ConnectionStatusPresenter(() => Strings.GameServices.OverlayService.ConnectionStatus_Mumble_Name,
                                                                                                         () => GameService.Gw2Mumble.IsAvailable,
                                                                                                         () => GameService.Gw2Mumble.IsAvailable
                                                                                                                   ? string.Format(Strings.GameServices.OverlayService.ConnectionStatus_Mumble_Connected, GameService.Gw2Mumble.CurrentMumbleMapName)
                                                                                                                   : string.Format(Strings.GameServices.OverlayService.ConnectionStatus_Mumble_Disconnected, GameService.Gw2Mumble.CurrentMumbleMapName))));

            arcdpsBridgeConnection.Show(new ConnectionStatusView().WithPresenter(new ConnectionStatusPresenter(() => Strings.GameServices.OverlayService.ConnectionStatus_ArcDPSBridge_Name,
                                                                                                               () => GameService.ArcDps.HudIsActive,
                                                                                                               () => GameService.ArcDps.HudIsActive 
                                                                                                                         ? Strings.GameServices.OverlayService.ConnectionStatus_ArcDPSBridge_Connected
                                                                                                                         : Strings.GameServices.OverlayService.ConnectionStatus_ArcDPSBridge_Disconnected)));
        }

    }
}
