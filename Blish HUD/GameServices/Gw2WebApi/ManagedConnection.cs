using Gw2Sharp;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Blish_HUD.Gw2WebApi {
    public sealed class ManagedConnection {

        private static readonly Logger Logger = Logger.GetLogger<ManagedConnection>();

        private readonly Connection _internalConnection;

        public IConnection Connection => _internalConnection;

        private readonly IGw2WebApiClient _internalClient;

        public IGw2WebApiClient Client => _internalClient;

        public ManagedConnection(string accessToken, TokenComplianceMiddleware tokenComplianceMiddle, ICacheMethod webApiCache, ICacheMethod renderCache = null, TimeSpan? renderCacheDuration = null) {
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

            Logger.Debug($"{nameof(ManagedConnection)} updated locale to {e.NewValue} (was {e.PrevousValue}).");
        }

        public bool SetApiKey(string apiKey) {
            if (string.Equals(_internalConnection.AccessToken, apiKey)) return false;

            _internalConnection.AccessToken = apiKey;

            return true;
        }

        public bool HasApiKey() {
            return !string.IsNullOrEmpty(_internalConnection.AccessToken);
        }

        internal async Task<string> RequestPrivilegedSubtoken(IEnumerable<TokenPermission> permissions, int days) {
            return HasApiKey() ? await GameService.Gw2WebApi.RequestSubtoken(this, permissions, days) : string.Empty;
        }

    }
}
