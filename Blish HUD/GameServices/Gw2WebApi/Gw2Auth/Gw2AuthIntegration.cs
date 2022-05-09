using Blish_HUD.GameServices;
using Blish_HUD.Gw2WebApi.Gw2Auth.Models;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Blish_HUD.Gw2WebApi.Gw2Auth {
    public class Gw2AuthIntegration : ServiceModule<Gw2WebApiService> {

        private static readonly Logger Logger = Logger.GetLogger<Gw2AuthIntegration>();

        public event EventHandler<ValueEventArgs<IEnumerable<JwtSubtokenModel>>> Success;

        private string _gw2AuthBaseAddress = "https://gw2auth.com/oauth2";
        private string _gw2AuthAuthorize   = "/authorize";
        private string _gw2AuthToken       = "/token";
        private string _gw2AuthJwks        = "/jwks";
        private string _gw2AuthRevoke      = "/revoke";
        private string _gw2AuthIntrospect  = "/introspect";

        private Gw2AuthConfig           _config;
        private CallbackListener        _listener;

        public Gw2AuthIntegration(Gw2WebApiService service) : base(service) {
            _config = new Gw2AuthConfig();
        }

        public override void Load() {
            _listener = new CallbackListener(_config.DefaultRedirectUri);
        }

        public override void Unload() {
            _listener.Dispose();
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
            
            // Here's where a backend would normally check for an active auth process that state matches response.State
            // We would then compare the auth response against the saved auth process and check for modifications.
            
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

            if (!userLogin.TryGetSubTokens(out var tokens)) {
                return;
            }

            Success?.Invoke(this, new ValueEventArgs<IEnumerable<JwtSubtokenModel>>(tokens));
            Logger.Info($"Successfully authorized through GW2Auth.com. Expires {userLogin.ExpiresAt} (UTC)");
        }
    }
}
