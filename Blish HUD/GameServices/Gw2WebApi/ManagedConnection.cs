using System;
using Gw2Sharp;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;

namespace Blish_HUD.Gw2WebApi {
    public sealed class ManagedConnection {

        private static readonly Logger Logger = Logger.GetLogger<ManagedConnection>();

        private readonly Connection _internalConnection;

        public IConnection Connection => _internalConnection;

        private readonly IGw2WebApiClient _internalClient;

        public IGw2WebApiClient Client => _internalClient;

        internal ManagedConnection(string accessToken, TokenComplianceMiddleware tokenComplianceMiddle, ICacheMethod webApiCache, ICacheMethod renderCache = null, TimeSpan? renderCacheDuration = null) {
            string ua = $"BlishHUD/{Program.OverlayVersion}";

            _internalConnection = new Connection(accessToken,
                                                 GameService.Overlay.UserLocale.Value,
                                                 webApiCache,
                                                 renderCache,
                                                 renderCacheDuration ?? TimeSpan.MaxValue,
                                                 ua);
            
            _internalConnection.Middleware.Add(tokenComplianceMiddle);

            _internalClient = new Gw2Client(_internalConnection).WebApi;

            Logger.Debug("Created managed Gw2Sharp connection {useragent}.", ua);

            SetupListeners();
        }

        private void SetupListeners() {
            GameService.Overlay.UserLocale.SettingChanged += UserLocaleOnSettingChanged;
        }

        private void UserLocaleOnSettingChanged(object sender, ValueChangedEventArgs<Locale> e) {
            _internalConnection.Locale = e.NewValue;

            Logger.Debug($"{nameof(ManagedConnection)} updated locale to {e.NewValue} (was {e.PreviousValue}).");
        }

        public bool SetApiKey(string apiKey) {
            if (string.Equals(_internalConnection.AccessToken, apiKey)) return false;

            _internalConnection.AccessToken = apiKey;

            Logger.Debug(apiKey == string.Empty
                             ? $"{_internalConnection.UserAgent} cleared API token."
                             : $"{_internalConnection.UserAgent} updated API token to {apiKey}.");

            return true;
        }

        public bool HasApiKey() {
            return !string.IsNullOrEmpty(_internalConnection.AccessToken);
        }
    }
}
