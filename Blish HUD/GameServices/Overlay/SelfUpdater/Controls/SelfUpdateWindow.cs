using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;
using Blish_HUD.Content;

namespace Blish_HUD.Overlay.SelfUpdater.Controls {
    internal class SelfUpdateWindow : Container {

        private static readonly Logger Logger = Logger.GetLogger<SelfUpdateWindow>();

        private readonly BouncyNotification  _bouncyChest;
        private readonly CoreVersionManifest _releaseManifest;

        private readonly Texture2D _windowTexture         = GameService.Content.GetTexture(@"views/selfupdater/updateview");
        private readonly Texture2D _heroReleaseTexture    = GameService.Content.GetTexture(@"views/selfupdater/hero-release");
        private readonly Texture2D _heroPrereleaseTexture = GameService.Content.GetTexture(@"views/selfupdater/hero-prerelease");

        private readonly FlowPanel _changePanel;

        private readonly StandardButton _bttnUpdate;
        private readonly GlowButton     _bttnSkip;

        private readonly LoadingSpinner _loadingSpinner;
        private readonly Label          _lblProgressMessage;

        public SelfUpdateWindow(CoreVersionManifest newReleaseManifest, bool withBouncyChest = false) {
            _releaseManifest = newReleaseManifest;

            this.Size          = _windowTexture.Bounds.Size;
            this.ContentRegion = new Rectangle(187, 82, 632, 824);
            this.Visible       = false;

            if (withBouncyChest) {
                _bouncyChest = new BouncyNotification(GameService.Content.GetTexture(@"views/selfupdater/744427-blue"), GameService.Content.GetTexture(@"views/selfupdater/744428-blue")) {
                    Parent = GameService.Graphics.SpriteScreen
                };

                _bouncyChest.Click += BouncyChest_Click;
            }
            
            var blishHudHero = new Label() {
                Width               = this.ContentRegion.Width,
                Top                 = 0,
                Height              = 36,
                Text                = $"- {Strings.Common.BlishHUD} -",
                Font                = GameService.Content.DefaultFont16,
                StrokeText          = true,
                VerticalAlignment   = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = this
            };

            _ = new Label() {
                Width               = this.ContentRegion.Width,
                AutoSizeHeight      = true,
                Height              = 82,
                Top                 = blishHudHero.Bottom,
                Text                = Strings.GameServices.OverlayService.SelfUpdate_NewUpdateAvailable,
                Font                = GameService.Content.DefaultFont32,
                StrokeText          = true,
                VerticalAlignment   = VerticalAlignment.Middle,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = this
            };

            var subLabel = new Label() {
                Top                 = 324,
                Width               = this.ContentRegion.Width,
                Height              = 82,
                TextColor           = ContentService.Colors.Chardonnay,
                Text                = string.Format(Strings.GameServices.OverlayService.SelfUpdate_ChangesInVersion, newReleaseManifest.Version.BaseAndPrerelease()),
                Font                = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular),
                StrokeText          = true,
                VerticalAlignment   = VerticalAlignment.Middle,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = this,
            };

            _changePanel = new FlowPanel() {
                Top       = subLabel.Bottom + 17,
                Height    = 306,
                Width     = 474,
                Left      = this.ContentRegion.Width / 2 - 474 / 2,
                Parent    = this,
                CanScroll = true
            };

            void CreateTitleLabel(string text) {
                _ = new Label() {
                    AutoSizeHeight = true,
                    AutoSizeWidth  = true,
                    Text           = $"\n{text}",
                    Font           = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),
                    Parent         = _changePanel
                };
            }

            void CreateBodyLabel(StringBuilder text) {
                if (string.IsNullOrEmpty(text.ToString()))
                    return;

                _ = new Label() {
                    AutoSizeHeight = true,
                    Width          = _changePanel.Width - 16,
                    WrapText       = true,
                    Text           = $"\n{text}",
                    Font           = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular),
                    Parent         = _changePanel
                };

                text.Clear();
            }

            var bodyBuffer = new StringBuilder();

            foreach (var line in newReleaseManifest.Changelog.Split('\n')) {
                // Section titles will start with '#'
                if (line.StartsWith("#")) {
                    // Clear the body buffer first
                    CreateBodyLabel(bodyBuffer);
                    CreateTitleLabel(line.TrimStart('#', ' '));
                } else {
                    bodyBuffer.AppendLine(line);
                }
            }

            CreateBodyLabel(bodyBuffer);

            _bttnUpdate = new StandardButton() {
                Top    = _changePanel.Bottom + 32,
                Width  = 128,
                Text   = Strings.Common.Action_Update,
                Left   = this.ContentRegion.Width / 2 - 128 / 2,
                Parent = this
            };

            _bttnSkip = new GlowButton() {
                Icon             = AsyncTexture2D.FromAssetId(605017),
                ActiveIcon       = AsyncTexture2D.FromAssetId(605016),
                Top              = _bttnUpdate.Top   - 3,
                Left             = _bttnUpdate.Right + 4,
                Size             = new Point(32, 32),
                BasicTooltipText = Strings.Common.Action_UpdateLater,
                Parent           = this
            };

            _loadingSpinner = new LoadingSpinner() {
                Location = new Point(this.ContentRegion.Width / 2 - 32, _changePanel.Bottom - _changePanel.Height / 2 - 70),
                Visible  = false,
                Parent   = this
            };

            _lblProgressMessage = new Label() {
                AutoSizeHeight      = true,
                Width               = _changePanel.Width,
                Left                = this.ContentRegion.Width / 2 - _changePanel.Width / 2,
                Top                 = _loadingSpinner.Bottom       + 6,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Top,
                Visible             = false,
                WrapText            = false,
                Parent              = this
            };

            _bttnUpdate.Click += Update_Click;
            _bttnSkip.Click   += Skip_Click;
        }

        private async void Update_Click(object sender, Input.MouseEventArgs e) {
            _lblProgressMessage.Text = "";
            SetDisplayMode(false, false);

            try {
                await SelfUpdateUtil.BeginUpdate(_releaseManifest, new Progress<string>(HandleUpdatingProgress));
            } catch (Exception ex) {
                Logger.Warn(ex, "BeginUpdate failed.  Reverted back.");
                DisplayFailed(ex);
            }
        }

        private void HandleUpdatingProgress(string message) {
            // Ensures we don't mistakenly update out of order
            if (_loadingSpinner.Visible) {
                _lblProgressMessage.Text = message;
            }
        }

        private void DisplayFailed(Exception failureException) {
            _bttnUpdate.Text         = Strings.Common.Action_Retry;
            _lblProgressMessage.Text = failureException.Message;

            SetDisplayMode(true, false);
        }

        private void SetDisplayMode(bool isPending, bool showChanges) {
            _changePanel.Visible = showChanges;
            _bttnUpdate.Visible  = isPending;
            _bttnSkip.Visible    = isPending;

            _lblProgressMessage.Visible = !showChanges;
            _loadingSpinner.Visible     = !isPending;

            _lblProgressMessage.Top = _loadingSpinner.Visible
                                          ? _loadingSpinner.Bottom + 6
                                          : _loadingSpinner.Top + _loadingSpinner.Height / 2;
        }

        private void BouncyChest_Click(object sender, Input.MouseEventArgs e) {
            this.Show();
        }

        protected override void OnShown(EventArgs e) {
            GameService.Overlay.BlishHudWindow.Hide();

            if (_bouncyChest != null) {
                _bouncyChest.ChestOpen = true;
            }

            this.Opacity = 0;
            GameService.Content.PlaySoundEffectByName("hero-open");
            GameService.Animation.Tweener.Tween(this, new { Opacity = 1 }, 1f, 0f, true);

            base.OnShown(e);
        }

        private void Skip_Click(object sender, Input.MouseEventArgs e) {
            GameService.Overlay.OverlayUpdateHandler.AcknowledgePendingReleases();
            GameService.Animation.Tweener.Tween(this, new { Opacity = 0 }, 1f, 0f, true).OnComplete(Dispose);

            if (_bouncyChest != null) {
                GameService.Animation.Tweener.Tween(_bouncyChest, new { Opacity = 1 }, 1f, 0f, true);
            }
        }

        public override void UpdateContainer(GameTime gameTime) {
            this.Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 - 512, GameService.Graphics.SpriteScreen.Height / 2 - 512);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _windowTexture,                                           bounds);
            spriteBatch.DrawOnCtrl(this, _releaseManifest.IsPrerelease ? _heroPrereleaseTexture : _heroReleaseTexture, _heroPrereleaseTexture.Bounds.OffsetBy(112, 157));
        }

        protected override void DisposeControl() {
            Visible = false;

            if (_bouncyChest != null) {
                _bouncyChest.Click -= BouncyChest_Click;
                _bouncyChest.Dispose();
            }

            base.DisposeControl();
        }

    }
}
