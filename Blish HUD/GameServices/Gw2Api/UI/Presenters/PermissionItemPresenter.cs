using System;
using System.Collections.Generic;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Views;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Gw2Api.UI.Presenters {
    public class PermissionItemPresenter : Presenter<PermissionItemView, PermissionItemPresenter.PermissionConsent> {

        public class PermissionConsent {

            private readonly TokenPermission _permission;
            private readonly bool            _required;

            private bool _consented;

            public TokenPermission Permission => _permission;
            public bool            Required   => _required;

            public bool Consented {
                get => _consented;
                set => _consented = value;
            }

            public PermissionConsent(TokenPermission permission, bool required, bool consented = true) {
                _permission = permission;
                _required   = required;
                _consented  = required || consented;
            }

        }

        private static readonly Dictionary<TokenPermission, Texture2D> _iconLookup;

        private static readonly Dictionary<TokenPermission, string> _descriptionLookup;

        static PermissionItemPresenter() {
            _iconLookup = new Dictionary<TokenPermission, Texture2D> {
                [TokenPermission.Account]     = GameService.Content.GetTexture(@"common\157085"),
                [TokenPermission.Inventories] = GameService.Content.GetTexture(@"common\157098"),
                [TokenPermission.Characters]  = GameService.Content.GetTexture(@"common\156694"),
                [TokenPermission.Tradingpost] = GameService.Content.GetTexture(@"common\157088"),
                [TokenPermission.Wallet]      = GameService.Content.GetTexture(@"common\156753"),
                [TokenPermission.Unlocks]     = GameService.Content.GetTexture(@"common\156714"),
                [TokenPermission.Pvp]         = GameService.Content.GetTexture(@"common\157119"),
                [TokenPermission.Builds]      = GameService.Content.GetTexture(@"common\156720"),
                [TokenPermission.Progression] = GameService.Content.GetTexture(@"common\156722"),
                [TokenPermission.Guilds]      = GameService.Content.GetTexture(@"common\156689")
            };

            _descriptionLookup = new Dictionary<TokenPermission, string> {
                [TokenPermission.Account]     = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Account,
                [TokenPermission.Inventories] = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Inventories,
                [TokenPermission.Characters]  = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Characters,
                [TokenPermission.Tradingpost] = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Tradingpost,
                [TokenPermission.Wallet]      = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Wallet,
                [TokenPermission.Unlocks]     = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Unlocks,
                [TokenPermission.Pvp]         = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Pvp,
                [TokenPermission.Builds]      = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Builds,
                [TokenPermission.Progression] = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Progression,
                [TokenPermission.Guilds]      = Strings.GameServices.Gw2ApiService.TokenPermissionDescription_Guilds
            };
        }

        /// <inheritdoc />
        public PermissionItemPresenter(PermissionItemView view, PermissionItemPresenter.PermissionConsent model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.ConsentChanged += ViewOnConsentChanged;

            this.View.Name = this.Model.Permission.ToString();

            this.View.ShowConsent = !this.Model.Required;
            this.View.Consented   = this.Model.Consented;

            if (_iconLookup.TryGetValue(this.Model.Permission, out Texture2D permissionIcon)) {
                this.View.Icon = permissionIcon;
            } else {
                this.View.Icon = ContentService.Textures.Error;
            }

            if (_descriptionLookup.TryGetValue(this.Model.Permission, out string permissionDescription)) {
                this.View.Description = permissionDescription;
            } else {
                this.View.Description = "Failed to find permission description!";
            }
        }

        private void ViewOnConsentChanged(object sender, EventArgs e) {
            this.Model.Consented = this.View.Consented;
        }

        /// <inheritdoc />
        protected override void Unload() {
            this.View.ConsentChanged -= ViewOnConsentChanged;
        }

    }
}
