using System;
using System.Reflection;
using Gw2Sharp;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Gw2WebApi {
    public class ManagedConnection {

        private static readonly Logger Logger = Logger.GetLogger<ManagedConnection>();

        private readonly Connection _internalConnection;

        public IConnection Connection => _internalConnection;

        private readonly IGw2WebApiClient _internalClient;

        public IGw2WebApiClient Client => _internalClient;

        public ManagedConnection(string accessToken, ICacheMethod webApiCache, ICacheMethod renderCache = null, TimeSpan? renderCacheDuration = null) {
            string ua = $"BlishHUD/{Program.OverlayVersion} ";

            _internalConnection = new Connection(accessToken,
                                                 GameService.Overlay.UserLocale.Value,
                                                 webApiCache,
                                                 renderCache,
                                                 renderCacheDuration,
                                                 ua);

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

        public void SetApiKey(string apiKey) {
            _internalConnection.AccessToken = apiKey;
        }

    }
}
