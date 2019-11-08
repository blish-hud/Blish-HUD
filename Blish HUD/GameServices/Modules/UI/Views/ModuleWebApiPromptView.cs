using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Common.UI;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Presenters;
using Blish_HUD.Gw2Api.UI.Views;
using Blish_HUD.Input;
using Blish_HUD.Modules.UI.Presenters;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleWebApiPromptView : View<ModuleWebApiPromptPresenter>, IReturningView<ModuleWebApiPromptView.ApiPromptResult> {

        public struct ApiPromptResult {

            public bool Accepted { get; }

            public TokenPermission[] ConsentedPermissions { get; }

            public ApiPromptResult(bool accepted, IEnumerable<TokenPermission> consentedPermissions) {
                this.Accepted             = accepted;
                this.ConsentedPermissions = consentedPermissions.ToArray();
            }

        }

        private const int SIDE_MARGIN   = 85;
        private const int TOP_MARGIN    = 20;
        private const int BOTTOM_MARGIN = 55;

        private const int STANDARD_PADDING = 6;

        private readonly ConcurrentQueue<Action<ApiPromptResult>> _returnWithQueue = new ConcurrentQueue<Action<ApiPromptResult>>();

        private Label _moduleNameLabel;
        private Label _namespaceLabel;

        private StandardButton _acceptButton;
        private StandardButton _cancelButton;

        public string ModuleName {
            get => _moduleNameLabel.Text;
            set => _moduleNameLabel.Text = value;
        }

        public string ModuleNamespace {
            get => _namespaceLabel.Text;
            set => _namespaceLabel.Text = value;
        }

        private readonly List<PermissionItemPresenter.PermissionConsent> _permissionConsents = new List<PermissionItemPresenter.PermissionConsent>();

        public ModuleWebApiPromptView(ModuleManager module) {
            this.Presenter = new ModuleWebApiPromptPresenter(this, module);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            buildPanel.BackgroundTexture = GameService.Content.GetTexture(@"common\backgrounds\156187");
            buildPanel.Size              = new Point(512, 512);

            _moduleNameLabel = new Label() {
                Font           = GameService.Content.DefaultFont24,
                TextColor      = Color.FromNonPremultiplied(255, 238, 153, 255),
                ShowShadow     = true,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Location       = new Point(SIDE_MARGIN, TOP_MARGIN),
                Text           = "_",
                Parent         = buildPanel
            };

            var infoLabel = new Label() {
                Font           = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Italic),
                ShowShadow     = true,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Text           = Strings.GameServices.ModulesService.ApiPermission_RequestedApiPermissionsInfo,
                Location       = new Point(_moduleNameLabel.Left, _moduleNameLabel.Bottom),
                Parent         = buildPanel
            };

            _acceptButton = new StandardButton() {
                Text     = Strings.Common.Action_Accept,
                Width    = 128,
                Location = new Point(buildPanel.Width / 2 - STANDARD_PADDING - 128, buildPanel.Height - BOTTOM_MARGIN - 26),
                Parent   = buildPanel
            };

            _cancelButton = new StandardButton() {
                Text     = Strings.Common.Action_Cancel,
                Width    = 128,
                Location = new Point(buildPanel.Width / 2 + STANDARD_PADDING, buildPanel.Height - BOTTOM_MARGIN - 26),
                Parent   = buildPanel
            };

            var permissionList = new FlowPanel() {
                Width               = buildPanel.Width                     - SIDE_MARGIN * 2,
                Height              = _acceptButton.Top - infoLabel.Bottom - STANDARD_PADDING,
                Location            = new Point(infoLabel.Left, infoLabel.Bottom),
                CanScroll           = true,
                ControlPadding      = new Vector2(0, 8),
                PadTopBeforeControl = true,
                Parent              = buildPanel
            };

            _namespaceLabel = new Label() {
                Text                = "_",
                ShowShadow          = true,
                Font                = GameService.Content.DefaultFont12,
                Width               = permissionList.Width,
                Location            = new Point(permissionList.Left, _acceptButton.Bottom + 6),
                AutoSizeHeight      = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = buildPanel
            };

            if (this.Presenter.Model.Manifest.ApiPermissions != null) {
                var requiredPermissions = this.Presenter.Model.Manifest.ApiPermissions.Where(p => !p.Value.Optional).Select(p => p.Key).ToArray();
                var optionalPermissions = this.Presenter.Model.Manifest.ApiPermissions.Where(p => p.Value.Optional).Select(p => p.Key).ToArray();

                void BuildPermissionSet(TokenPermission[] permissions, bool required) {
                    if (permissions.Length == 0) return;

                    _ = new Label() {
                        Text           = (required
                                              ? Strings.GameServices.ModulesService.ApiPermission_Required
                                              : Strings.GameServices.ModulesService.ApiPermission_Optional) + " -",
                        AutoSizeHeight = true,
                        AutoSizeWidth  = true,
                        ShowShadow     = true,
                        Font           = GameService.Content.DefaultFont12,
                        Parent         = permissionList
                    };

                    foreach (var permission in permissions) {
                        var permissionConsent = new PermissionItemPresenter.PermissionConsent(permission, required, (this.Presenter.Model.State.UserEnabledPermissions ?? new TokenPermission[1] {permission}).Contains(permission));
                        var permissionView    = new PermissionItemView(permissionConsent);

                        var permissionContainer = new ViewContainer() {
                            Width  = permissionList.ContentRegion.Width - STANDARD_PADDING * 4,
                            Parent = permissionList
                        };

                        permissionContainer.Show(permissionView);
                        _permissionConsents.Add(permissionConsent);
                    }
                }

                BuildPermissionSet(requiredPermissions, true);
                BuildPermissionSet(optionalPermissions, false);
            }

            _acceptButton.Click += AcceptButtonOnClick;
            _cancelButton.Click += CancelButtonOnClick;
        }

        private void AcceptButtonOnClick(object sender, MouseEventArgs e) {
            FinalizeReturn(new ApiPromptResult(true, _permissionConsents.Where(p => p.Consented).Select(p => p.Permission)));
        }

        private void CancelButtonOnClick(object sender, MouseEventArgs e) {
            FinalizeReturn(new ApiPromptResult(false, new TokenPermission[0]));
        }

        /// <inheritdoc />
        public void ReturnWith(Action<ApiPromptResult> returnAction) {
            _returnWithQueue.Enqueue(returnAction);
        }

        private void FinalizeReturn(ApiPromptResult value) {
            while (_returnWithQueue.TryDequeue(out Action<ApiPromptResult> action)) {
                action.Invoke(value);
            }
        }

    }
}
