using Blish_HUD.GameServices;
using Blish_HUD.Gw2WebApi.Gw2Auth.Models;
using Flurl;
using Flurl.Http;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Gw2Sharp.WebApi;
using Newtonsoft.Json;

namespace Blish_HUD.Gw2WebApi.Gw2Auth {
    public class Gw2AuthIntegration : ServiceModule<Gw2WebApiService> {

        private static readonly Logger Logger = Logger.GetLogger<Gw2AuthIntegration>();

        public event EventHandler<EventArgs> Login;

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
            _listener = new CallbackListener(_config.ClientRedirectUri);
        }

        public override void Unload() {
            _listener.Dispose();
        }

        public void Authorize(string displayName = "") {
            _listener.Stop();

            var url = (_gw2AuthBaseAddress + _gw2AuthAuthorize).SetQueryParams($"response_type={_config.DefaultResponseType}",
                                                                               $"client_id={_config.Gw2AuthClientId}",
                                                                               $"state={RandomUtil.GetUniqueKey(_config.StateParamLength)}",
                                                                               $"scope={_config.DefaultScope}",
                                                                               $"redirect_uri={_config.ClientRedirectUri}",
                                                                               string.IsNullOrEmpty(displayName.Trim()) ? string.Empty : $"name={displayName.Trim()}",
                                                                               $"prompt={_config.DefaultPrompt}");

            Logger.Info("Starting authorization through GW2Auth.");

            _listener.Start(OnAuthorizedCallback);

            Process.Start(url);
        }

        private async void OnAuthorizedCallback(HttpListenerContext context) {
            context.Response.KeepAlive = false; // Required for redirect.

            var response = AuthResponseModel.FromQuery(context.Request.QueryString);

            if (response.IsError()) {
                Exit(context, response.Error, response.ErrorDescription);
                return;
            }
            
            // Here's where a web-hosted service would normally reignite the previously stored auth process by matching the state parameter.
            // We would then verify that the permissions were not altered before continuing to log in.
            // However, since we are a local application we trust ourselfs that this won't be the case.

            await TryLogIn(response.Code, context);
        }

        private async Task TryLogIn(string authCode, HttpListenerContext context) {
            var url = (_gw2AuthBaseAddress + _gw2AuthToken).SetQueryParams($"grant_type={_config.GrantTypeAuthorization}",
                                                                           $"code={authCode}",
                                                                           $"client_id={_config.Gw2AuthClientId}",
                                                                           $"client_secret={_config.Gw2AuthClientSecret}",
                                                                           // Not an actual redirect. Required for verifying integrity.
                                                                           $"redirect_uri={_config.ClientRedirectUri}");

            var response = await url.PostAsync(null);

            if (!response.IsSuccessStatusCode) {
                Exit(context, response.StatusCode.ToString(), response.ReasonPhrase);
                return;
            }

            AccessTokenModel userLogin;
            try {
                userLogin = JsonConvert.DeserializeObject<AccessTokenModel>(await response.Content.ReadAsStringAsync());
                if (userLogin == null) {
                    throw new NullReferenceException($"Deserialization failed. '{nameof(userLogin)}' was null.");
                }
            } catch (Exception e) when (e is FlurlHttpException || e is NullReferenceException) {
                Logger.Error(e, e.Message);
                Exit(context, "server_error", "Unexpected error while processing request.");
                return;
            }

            // We require the "characters" scope.
            if (userLogin.GetTokenPermissions().Intersect(new []{ TokenPermission.Account, TokenPermission.Characters }).Count() != 2) {
                Exit(context, "invalid_scope", "Missing required permission \"characters\".");
                return;
            }

            Logger.Info("Processing subtokens from GW2Auth.");

            if (!userLogin.TryGetSubTokens(out var tokens)) { 
                Exit(context, "invalid_request", "No API keys received.");
                return;
            }

            bool hasToken = false;
            foreach (var token in tokens) {
                if (token.IsError()) {
                    Logger.Warn($"Skipped corrupted subtoken \"{token.Name} - {token.Error}\".");
                    continue;
                }

                if (!token.Verified) {
                    Logger.Warn($"Skipped unverified subtoken \"{token.Name} - {token.Token.Substring(0,5)}***\".");
                    continue;
                }

                hasToken = true;

                Logger.Info($"Registering subtoken \"{token.Name} - {token.Token.Substring(0,5)}***\".");
                await _service.RegisterKey(token.Name, token.Token);
            }

            // All tokens were ineligible.
            if (!hasToken) {
                Exit(context, "unauthorized_client", "Unable to register faulty or unverified API keys.");
                return;
            }

            Login?.Invoke(this, EventArgs.Empty);
            Exit(context);
            Logger.Info($"Successfully authorized through GW2Auth. Expires {userLogin.ExpiresAt} (UTC).");
        }

        private void Exit(HttpListenerContext context, string error = null, string description = null) {
            string redirect = _config.ResultRedirectUri.SetQueryParam($"lang={GetUserLocaleShort()}");
            if (!string.IsNullOrEmpty(error)) {
                Logger.Error(description);
                redirect = redirect.SetQueryParams($"error={error}", 
                                                   $"error_description={description ?? string.Empty}");
            }
            context.Response.Redirect(redirect);
            context.Response.Close();
            _listener.Stop();
        }

        private string GetUserLocaleShort() {
            return GameService.Overlay.UserLocale.Value switch {
                Locale.English => "en",
                Locale.Spanish => "es",
                Locale.German  => "de",
                Locale.French  => "fr",
                Locale.Korean  => "kr",
                Locale.Chinese => "zh",
                _              => "en"
            };
        }
    }
}
