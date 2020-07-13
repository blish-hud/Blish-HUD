using System;
using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Modules.UI.Presenters;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public class ModulePermissionView : TitledDetailView {

        public event EventHandler<KeyedValueChangedEventArgs<TokenPermission, bool>> PermissionStateChanged;

        private FlowPanel _permissionFlowPanel;
        private Label     _messageLabel;

        public ModulePermissionView() { /* NOOP */ }

        public ModulePermissionView(ModuleManager model) {
            if (model == null) throw new ArgumentNullException(nameof(model));

            this.WithPresenter(new ModulePermissionPresenter(this, model));
        }

        protected override void BuildDetailView(Panel buildPanel) {
            this.Title = Strings.GameServices.ModulesService.ModuleManagement_ApiPermissions;

            _permissionFlowPanel = new FlowPanel() {
                Size                = buildPanel.ContentRegion.Size,
                Visible             = false,
                FlowDirection       = ControlFlowDirection.TopToBottom,
                ControlPadding      = new Vector2(14, 15),
                OuterControlPadding = new Vector2(15, 13),
                Parent              = buildPanel
            };

            _messageLabel = new Label() {
                Size                = buildPanel.ContentRegion.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text                = Strings.GameServices.ModulesService.ApiPermission_NoPermissionsRequested,
                StrokeText          = true,
                Font                = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic),
                Parent              = buildPanel
            };
        }

        public void SetPermissions(IEnumerable<(TokenPermission Permission, bool Optional, string Description, bool Set)> permissions) {
            _permissionFlowPanel.ClearChildren();
            _permissionFlowPanel.Hide();

            foreach ((var permission, bool optional, string description, bool set) in permissions) {
                var permissionCheckbox = new Checkbox() {
                    Text             = permission.ToString(),
                    Checked          = set || !optional,
                    Enabled          = optional,
                    BasicTooltipText = description,
                    Parent           = _permissionFlowPanel
                };

                permissionCheckbox.CheckedChanged += delegate (object sender, CheckChangedEvent e) {
                    this.PermissionStateChanged?.Invoke(this, new KeyedValueChangedEventArgs<TokenPermission, bool>(permission, e.Checked));
                };
            }

            // Show "No permissions requested" if there are none
            _messageLabel.Visible = !(_permissionFlowPanel.Visible = _permissionFlowPanel.Children.Count > 0);
        }

    }
}
