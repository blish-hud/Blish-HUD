using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Presenters;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleWebApiPermissionsView : View<ModuleWebApiPermissionsPresenter> {

        public struct PermissionState {
            
            public bool Requested { get; }
            public bool Required  { get; }
            public bool Consented { get; }

            public PermissionState(bool required, bool consented, bool requested = true) {
                this.Requested = requested;
                this.Required  = required;
                this.Consented = consented;
            }

            public static PermissionState NotRequested => new PermissionState(false, false, false);

        }

        private Dictionary<TokenPermission, Label> _permissionLabels;
        
        private Label _requiredLabel;
        private Label _optionalLabel;

        private int _requiredBottom = 0;
        private int _optionalBottom = 0;

        public ModuleWebApiPermissionsView(ModuleManager module) {
            this.Presenter = new ModuleWebApiPermissionsPresenter(this, module);
        }

        private Color PermissionColorFromState(PermissionState permissionState) {
            if (permissionState.Consented) return Color.Green;

            return permissionState.Required
                       ? Control.StandardColors.Red
                       : Control.StandardColors.Yellow;
        }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            TokenPermission[] allPermissions = Enum.GetValues(typeof(TokenPermission)).Cast<TokenPermission>().ToArray();

            _permissionLabels = new Dictionary<TokenPermission, Label>(allPermissions.Length);

            foreach (var permission in allPermissions) {
                var permissionLabel = new Label() {
                    AutoSizeWidth  = true,
                    AutoSizeHeight = true,
                    Text           = permission.ToString()
                };

                _permissionLabels.Add(permission, permissionLabel);
            }

            return base.Load(progress);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            buildPanel.CanScroll = true;

            _requiredLabel = new Label() {
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Text           = $"{Strings.GameServices.ModulesService.ApiPermission_Required} -",
                Location       = new Point(10, 10),
                Font           = GameService.Content.DefaultFont12,
                TextColor      = Control.StandardColors.DisabledText,
                Parent         = buildPanel
            };

            _optionalLabel = new Label() {
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Text           = $"{Strings.GameServices.ModulesService.ApiPermission_Optional} -",
                Location       = new Point(buildPanel.Width / 2, 10),
                Font           = GameService.Content.DefaultFont12,
                TextColor      = Control.StandardColors.DisabledText,
                Parent         = buildPanel
            };

            _requiredBottom = _requiredLabel.Bottom;
            _optionalBottom = _optionalLabel.Bottom;
        }


        public void SetPermissionCheckboxState(TokenPermission permission, PermissionState permissionState) {
            var permissionCheckbox = _permissionLabels[permission];

            if (permissionState.Requested) {
                permissionCheckbox.TextColor = PermissionColorFromState(permissionState) * 0.75f;

                if (permissionState.Required) {
                    permissionCheckbox.Location = new Point(_requiredLabel.Left, _requiredBottom + 5);
                    _requiredBottom             = permissionCheckbox.Bottom;
                } else {
                    permissionCheckbox.Location = new Point(_optionalLabel.Left, _optionalBottom + 5);
                    _optionalBottom             = permissionCheckbox.Bottom;
                }

                permissionCheckbox.Parent = this.ViewTarget;
            } else {
                permissionCheckbox.Parent = null;
            }
        }

    }
}
