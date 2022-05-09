using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.GameServices;
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
        private ICacheMethod              _sharedWebCache;
        private ICacheMethod              _sharedRenderCache;

        private void InitCache() {
            var bucket = new TokenBucket(300, 5);

            _sharedTokenBucketMiddleware = new TokenComplianceMiddleware(bucket);
            _sharedWebCache              = new MemoryCacheMethod();
            _sharedRenderCache           = new MemoryCacheMethod();
        }

        #endregion

        #region Init Cache, Connection, & Client

        private ManagedConnection _anonymousConnection;
        private ManagedConnection _privilegedConnection;

        private void CreateInternalConnection() {
            InitCache();

            _anonymousConnection  = new ManagedConnection(string.Empty, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache, TimeSpan.MaxValue);
            _privilegedConnection = new ManagedConnection(string.Empty, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache, TimeSpan.MaxValue);
        }

        public   ManagedConnection AnonymousConnection  => _anonymousConnection;
        internal ManagedConnection PrivilegedConnection => _privilegedConnection;

        #endregion

        private readonly ConcurrentDictionary<string, string>            _characterRepository = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, ManagedConnection> _cachedConnections   = new ConcurrentDictionary<string, ManagedConnection>();

        private SettingCollection _apiSettings;
        private SettingCollection _apiKeyRepository;

        public Gw2AuthIntegration Gw2Auth { get; }

        public Gw2WebApiService() {
            SetServiceModules(this.Gw2Auth = new Gw2AuthIntegration(this));
        }

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
            GameService.Overlay.SettingsTab.RegisterSettingMenu(new MenuItem(Strings.GameServices.Gw2ApiService.ManageApiKeysSection, GameService.Content.GetTexture("155048")),
                                                                (m) => new RegisterApiKeyView(),
                                                                int.MaxValue - 11);
        }

        private async Task UpdateBaseConnection(string apiKey) {
            if (_privilegedConnection.SetApiKey(apiKey)) {
                await Modules.Managers.Gw2ApiManager.RenewAllSubtokens();
            }
        }

        private async Task UpdateActiveApiKey() {
            if (_characterRepository.TryGetValue(Gw2Mumble.PlayerCharacter.Name, out string charApiKey)) {
                await UpdateBaseConnection(charApiKey);
                Logger.Debug($"Associated key {charApiKey} with user {Gw2Mumble.PlayerCharacter.Name}.");
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

            foreach (SettingEntry<string> key in _apiKeyRepository.Cast<SettingEntry<string>>()) {
                await UpdateCharacterList(key);
            }

            await UpdateActiveApiKey();
        }

        #region API Management

        public async Task RegisterKey(string name, string apiKey) {
            SettingEntry<string> registeredKey = _apiKeyRepository.DefineSetting(name, "");

            registeredKey.Value = apiKey;

            await UpdateCharacterList(registeredKey);
            await UpdateActiveApiKey();
        }

        public async Task UnregisterKey(string apiKey) {
            foreach (SettingEntry<string> key in _apiKeyRepository.Cast<SettingEntry<string>>()) {
                if (string.Equals(apiKey, key.Value, StringComparison.InvariantCultureIgnoreCase) || key.Value.StartsWith(apiKey, StringComparison.InvariantCultureIgnoreCase)) {
                    _apiKeyRepository.UndefineSetting(key.EntryKey);

                    await RefreshRegisteredKeys();

                    await UpdateActiveApiKey();

                    break;
                }
            }
        }

        internal string[] GetKeys() {
            return _apiKeyRepository.Cast<SettingEntry<string>>().Select((setting) => setting.Value).ToArray();
        }

        private async Task UpdateCharacterList(SettingEntry<string> definedKey) {
            try {
                List<string> characters = await GetCharacters(GetConnection(definedKey.Value));

                foreach (string characterId in characters) {
                    _characterRepository.AddOrUpdate(characterId, definedKey.Value, (k, o) => definedKey.Value);
                }

                Logger.Info("Associated API key {keyName} with characters: {charactersList}", definedKey.EntryKey, string.Join(", ", characters));
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to get list of associated characters for API key {keyName}.", definedKey.EntryKey);
            }
        }

        private async Task<List<string>> GetCharacters(ManagedConnection connection) {
            return (await connection.Client.V2.Characters.IdsAsync()).ToList();
        }

        internal async Task<string> RequestPrivilegedSubtoken(IEnumerable<TokenPermission> permissions, int days) {
            return await RequestSubtoken(_privilegedConnection, permissions, days);
        }

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
            if (string.IsNullOrWhiteSpace(accessToken)) {
                return new ManagedConnection(string.Empty, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache);
            }

            return _cachedConnections.GetOrAdd(accessToken, (token) => new ManagedConnection(token, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache));
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

    }
}