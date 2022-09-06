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

            var infoPanel = new Panel() {
                Width = buildPanel.Width,
                Height = 32,
                Bottom = buildPanel.Bottom,
                Parent = buildPanel
            };

            var aboutPanel = new Panel() {
                ShowBorder = true,
                Width = buildPanel.Width,
                Height = buildPanel.Height - infoPanel.Height,
                Parent = buildPanel,
                CanScroll = true,
            };

            var lovePanel = new Panel() {
                Size = new Point(aboutPanel.Width - 128, 128),
                Left = (aboutPanel.Width / 2) - ((aboutPanel.Width - 128) / 3),
                Parent = aboutPanel
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

            var gw2CopyrightStatement = new Label() {
                Font = GameService.Content.DefaultFont16,
                Text = string.Format(Strings.GameServices.OverlayService.AboutAnetNotice, DateTime.Now.Year),
                AutoSizeHeight = true,
                Width = aboutPanel.Width,
                Top = lovePanel.Bottom,
                StrokeText = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Parent = aboutPanel
            };

            var thirdPartySoftwareHeading = new Label() {
                Font = GameService.Content.DefaultFont16,
                Text = Strings.GameServices.OverlayService.AboutThirdPartySoftwareHeading,
                Top = gw2CopyrightStatement.Bottom + 64,
                Width = aboutPanel.Width,
                StrokeText = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Parent = aboutPanel,
            };

            var thirdPartySoftwareStatement = new Label() {
                Font = GameService.Content.DefaultFont14,
                Text = Strings.GameServices.OverlayService.AboutThirdPartySoftware,
                AutoSizeHeight = true,
                Top = thirdPartySoftwareHeading.Bottom + 32,
                Width = aboutPanel.Width,
                Left = 24,
                StrokeText = false,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Parent = aboutPanel,
            };

            var version = new Label() {
                AutoSizeHeight      = true,
                Width               = infoPanel.Width,
                Text                = $"{Strings.Common.BlishHUD} v{Program.OverlayVersion.BaseAndPrerelease()}",
                BasicTooltipText    = $"v{Program.OverlayVersion}",
                Font                = GameService.Content.DefaultFont14,
                HorizontalAlignment = HorizontalAlignment.Right,
                StrokeText          = true,
                ClipsBounds         = false,
                Parent              = infoPanel
            };

            //version.Location = new Point(infoPanel.Width - version.Width + 8, infoPanel.Height - version.Height);

            var mumbleConnection = new ViewContainer() {
                Size   = new Point(128, 20),
                Left   = 8,
                Parent = infoPanel
            };

            var arcdpsBridgeConnection = new ViewContainer() {
                Size   = new Point(128, 20),
                Left   = mumbleConnection.Right,
                Parent = infoPanel
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
