using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Blish_HUD.GameServices.Gw2Auth;
using Blish_HUD.GameServices.Gw2Auth.Models;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

namespace Blish_HUD.GameServices {
    public class Gw2AuthService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<Gw2AuthService>();

        public event EventHandler<ValueEventArgs<IEnumerable<JwtSubtokenModel>>> Success;

        private string _gw2AuthBaseAddress = "https://gw2auth.com/oauth2";
        private string _gw2AuthAuthorize   = "/authorize";
        private string _gw2AuthToken       = "/token";
        private string _gw2AuthJwks        = "/jwks";
        private string _gw2AuthRevoke      = "/revoke";
        private string _gw2AuthIntrospect  = "/introspect";
        private string _subtokenClaimType  = "gw2:tokens";

        private Gw2AuthConfig           _config;
        private CallbackListener        _listener;
        private JwtSecurityTokenHandler _tokenHandler;

        protected override void Initialize() {
            _config       = new Gw2AuthConfig();
            _listener     = new CallbackListener();
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        protected override void Load() {
            /* NOOP */
        }

        protected override void Unload() {
            /* NOOP */
        }

        public void Authorize(string displayName = "") {
            var url = (_gw2AuthBaseAddress + _gw2AuthAuthorize).SetQueryParams(
                                                                               $"response_type={_config.DefaultResponseType}",
                                                                               $"client_id={_config.Gw2AuthClientId}",
                                                                               $"state={RandomUtil.GetUniqueKey(_config.StateParamLength)}",
                                                                               $"scope={_config.DefaultScope}",
                                                                               $"redirect_uri={_config.DefaultRedirectUri}",
                                                                               (string.IsNullOrEmpty(displayName.Trim()) ? string.Empty : $"name={displayName.Trim()}"),
                                                                               $"prompt={_config.DefaultPrompt}"
                                                                              );
            _listener.Start(OnAuthorizedCallback);
            Process.Start(url);
        }

        private async void OnAuthorizedCallback(HttpListenerContext context) {
            _listener.Stop();

            var response = AuthResponseModel.FromQuery(context.Request.QueryString);
            if (response.IsError()) {
                Logger.Info(response.ErrorDescription);
            }
            
            // Here's where a backend would normally check for an active auth process that matches response.State
            // and if the returned scope still matches the initial requested scope to counter manipulation.

            await Login(response.Code);
        }

        private async Task Login(string authCode) {
            var url = (_gw2AuthBaseAddress + _gw2AuthToken).SetQueryParams(
                                                                           $"grant_type={_config.GrantTypeAuthorization}",
                                                                           $"code={authCode}",
                                                                           $"client_id={_config.Gw2AuthClientId}",
                                                                           $"client_secret={_config.Gw2AuthClientSecret}",
                                                                           $"redirect_uri={_config.DefaultRedirectUri}");

            AccessTokenModel userLogin;
            try {
                userLogin = await url.PostAsync(null).ReceiveJson<AccessTokenModel>();
            } catch (FlurlHttpException e) {
                Logger.Warn(e, e.Message);
                return;
            }

            if (!this.TryGetSubTokens(userLogin.AccessToken, out var tokens)) {
                return;
            }

            Success?.Invoke(this, new ValueEventArgs<IEnumerable<JwtSubtokenModel>>(tokens));
            Logger.Info($"Successfully authorized through GW2Auth. Expires {userLogin.ExpiresIn}");
        }

        private bool TryGetGuid(string jwt, out Guid id) {
            var jwtToken = _tokenHandler.ReadJwtToken(jwt);
            return Guid.TryParse(jwtToken.Subject, out id);
        }

        private bool TryGetSubTokens(string jwt, out IEnumerable<JwtSubtokenModel> tokens) {
            var jwtToken = _tokenHandler.ReadJwtToken(jwt);

            tokens = Enumerable.Empty<JwtSubtokenModel>();

            var claim = jwtToken.Claims.FirstOrDefault(x => x.Type.Equals(_subtokenClaimType));
            if (claim == null) {
                return false;
            }

            var users = JsonConvert.DeserializeObject<Dictionary<string, JwtSubtokenModel>>(claim.Value);
            if (users == null) {
                return false;
            }

            tokens = users.Values;
            return true;
        }

        protected override void Update(GameTime gameTime) {
            /* NOOP */
        }

    }
}
