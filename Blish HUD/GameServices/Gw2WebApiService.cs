using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Gw2WebApi;
using Blish_HUD.Gw2WebApi.UI.Views;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.Exceptions;

namespace Blish_HUD {

    public class Gw2WebApiService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<Gw2WebApiService>();

        private const string GW2WEBAPI_SETTINGS = "Gw2WebApiConfiguration";

        private const string SETTINGS_ENTRY_APIKEYS = "ApiKeyRepository";

        #region Cache Handling

        private TokenComplianceMiddleware _sharedTokenBucketMiddleware;
        private ICacheMethod _sharedWebCache;
        private ICacheMethod _sharedRenderCache;

        private void InitCache() {
            var bucket = new TokenBucket(300, 5);

            _sharedTokenBucketMiddleware = new TokenComplianceMiddleware(bucket);
            _sharedWebCache = new MemoryCacheMethod();
            _sharedRenderCache = new MemoryCacheMethod();
        }

        #endregion

        #region Init Cache, Connection, & Client

        private void CreateInternalConnection() {
            InitCache();

            this.AnonymousConnection = new ManagedConnection(string.Empty, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache, TimeSpan.MaxValue);
            this.PrivilegedConnection = new ManagedConnection(string.Empty, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache, TimeSpan.MaxValue);
        }

        public ManagedConnection AnonymousConnection { get; private set; }
        internal ManagedConnection PrivilegedConnection { get; private set; }

        #endregion

        private readonly ConcurrentDictionary<string, string> _characterRepository = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, ManagedConnection> _cachedConnections = new ConcurrentDictionary<string, ManagedConnection>();

        private SettingCollection _apiSettings;
        private SettingCollection _apiKeyRepository;

        protected override void Initialize() {
            _apiSettings = Settings.RegisterRootSettingCollection(GW2WEBAPI_SETTINGS);

            DefineSettings(_apiSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            _apiKeyRepository = ((SettingEntry<SettingCollection>)settings[SETTINGS_ENTRY_APIKEYS])?.Value
                             ?? settings.AddSubCollection(SETTINGS_ENTRY_APIKEYS);
        }

        protected override void Load() {
            CreateInternalConnection();

            Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacterOnNameChanged;

            RegisterApiInSettings();
        }

        private void RegisterApiInSettings() {
            // Manage API Keys
            GameService.Overlay.SettingsTab.RegisterSettingMenu(new MenuItem(Strings.GameServices.Gw2ApiService.ManageApiKeysSection, AsyncTexture2D.FromAssetId(155048)),
                                                                (m) => new RegisterApiKeyView(),
                                                                int.MaxValue - 11);
        }

        private async Task UpdateBaseConnection(string apiKey) {
            if (this.PrivilegedConnection.SetApiKey(apiKey)) {
                await Modules.Managers.Gw2ApiManager.RenewAllSubtokens();
            }
        }

        private async Task UpdateActiveApiKey() {
            if (_characterRepository.TryGetValue(Gw2Mumble.PlayerCharacter.Name, out string charApiKey)) {
                await UpdateBaseConnection(charApiKey);
            } else {
                if (!string.IsNullOrWhiteSpace(Gw2Mumble.PlayerCharacter.Name)) {
                    // We skip the message if no user is defined yet.
                    Logger.Info("Could not find registered API key associated with character {characterName}", Gw2Mumble.PlayerCharacter.Name);
                }

                await UpdateBaseConnection(string.Empty);
            }
        }

        private async void PlayerCharacterOnNameChanged(object sender, ValueEventArgs<string> e) {
            if (!_characterRepository.ContainsKey(e.Value)) {
                // We don't currently have an API key associated to this character so we double-check the characters on each key
                await RefreshRegisteredKeys();
            } else {
                await UpdateActiveApiKey();
            }
        }

        private async Task RefreshRegisteredKeys() {
            _characterRepository.Clear();

            foreach (var key in _apiKeyRepository.Cast<SettingEntry<string>>()) {
                await UpdateCharacterList(key);
            }

            await UpdateActiveApiKey();
        }

        #region API Management

        public async Task RegisterKey(string name, string apiKey) {
            var registeredKey = _apiKeyRepository.DefineSetting(name, "");

            registeredKey.Value = apiKey;

            await UpdateCharacterList(registeredKey);
            await UpdateActiveApiKey();
        }

        public async Task UnregisterKey(string apiKey) {
            foreach (var key in _apiKeyRepository.Cast<SettingEntry<string>>()) {
                if (string.Equals(apiKey, key.Value, StringComparison.InvariantCultureIgnoreCase) || key.Value.StartsWith(apiKey, StringComparison.InvariantCultureIgnoreCase)) {
                    _apiKeyRepository.UndefineSetting(key.EntryKey);

                    await RefreshRegisteredKeys();

                    await UpdateActiveApiKey();

                    break;
                }
            }
        }

        internal string[] GetKeys() => _apiKeyRepository.Cast<SettingEntry<string>>().Select((setting) => setting.Value).ToArray();

        private async Task UpdateCharacterList(SettingEntry<string> definedKey) {
            try {
                var characters = await GetCharacters(GetConnection(definedKey.Value));

                foreach (string characterId in characters) {
                    _characterRepository.AddOrUpdate(characterId, definedKey.Value, (k, o) => definedKey.Value);
                }

                Logger.Info("Associated API key {keyName} with characters: {charactersList}", definedKey.EntryKey, string.Join(", ", characters));
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to get list of associated characters for API key {keyName}.", definedKey.EntryKey);
            }
        }

        private async Task<List<string>> GetCharacters(ManagedConnection connection) => (await connection.Client.V2.Characters.IdsAsync()).ToList();

        internal async Task<string> RequestPrivilegedSubtoken(IEnumerable<TokenPermission> permissions, int days) => await RequestSubtoken(this.PrivilegedConnection, permissions, days);

        public async Task<string> RequestSubtoken(ManagedConnection connection, IEnumerable<TokenPermission> permissions, int days) {
            var tokenPermissions = permissions as TokenPermission[] ?? permissions.ToArray();

            if (!tokenPermissions.Any() || string.IsNullOrEmpty(connection.Connection.AccessToken)) {
                return string.Empty;
            }

            try {
                return (await connection.Client
                                        .V2.CreateSubtoken
                                        .WithPermissions(tokenPermissions)
                                        .Expires(DateTime.UtcNow.AddDays(days))
                                        .GetAsync()).Subtoken;
            } catch (InvalidAccessTokenException ex) {
                Logger.Warn(ex, "The provided API token is invalid and can not be used to request a subtoken.");
            } catch (UnexpectedStatusException ex) {
                Logger.Warn(ex, "The provided API token could not be used to request a subtoken.");
            }

            return string.Empty;
        }

        #endregion

        public ManagedConnection GetConnection(string accessToken) {
            // Avoid caching connections without an API key
            return string.IsNullOrWhiteSpace(accessToken)
                ? new ManagedConnection(string.Empty, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache)
                : _cachedConnections.GetOrAdd(accessToken, (token) => new ManagedConnection(token, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache));
        }

        protected override void Unload() { /* NOOP */ }

        private double _checkFrequency = 0;

        protected override void Update(GameTime gameTime) {
            _checkFrequency += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_checkFrequency > 180000) {
                _checkFrequency = 0;

                if (string.IsNullOrEmpty(this.PrivilegedConnection.Connection.AccessToken)) {
                    RefreshRegisteredKeys();
                }
            }
        }
    }
}
