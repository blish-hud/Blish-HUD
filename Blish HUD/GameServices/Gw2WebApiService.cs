using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Gw2WebApi;
using Blish_HUD.Gw2WebApi.UI.Views;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.Exceptions;
using Blish_HUD.Modules;

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

        private ISettingCollection _apiSettings;
        private ISettingEntry<Dictionary<string, string>> _apiKeyRepository;

        protected override void Initialize() {
            _apiSettings = Settings.RegisterRootSettingCollection(GW2WEBAPI_SETTINGS);

            DefineSettings(_apiSettings);
        }

        private void DefineSettings(ISettingCollection settings) {
            _apiKeyRepository = settings.DefineSetting(SETTINGS_ENTRY_APIKEYS, new Dictionary<string, string>());
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

        private void UpdateBaseConnection(string apiKey) {
            if (_privilegedConnection.SetApiKey(apiKey)) {
                Modules.Managers.Gw2ApiManager.RenewAllSubtokens();
            }
        }

        private void UpdateActiveApiKey() {
            if (_characterRepository.TryGetValue(Gw2Mumble.PlayerCharacter.Name, out string charApiKey)) {
                UpdateBaseConnection(charApiKey);
                Logger.Debug($"Associated key {charApiKey} with user {Gw2Mumble.PlayerCharacter.Name}.");
            } else {
                UpdateBaseConnection(string.Empty);
                Logger.Info("Could not find registered API key associated with character {characterName}", Gw2Mumble.PlayerCharacter.Name);
            }
        }

        private void PlayerCharacterOnNameChanged(object sender, ValueEventArgs<string> e) {
            if (!_characterRepository.ContainsKey(e.Value)) {
                // We don't currently have an API key associated to this character so we double-check the characters on each key
                RefreshRegisteredKeys();
            } else {
                UpdateActiveApiKey();
            }
        }

        private void RefreshRegisteredKeys() {
            foreach (var kvp in _apiKeyRepository.Value) {
                UpdateCharacterList(kvp.Key, kvp.Value);
            }
        }

        #region API Management

        public void RegisterKey(string name, string apiKey) {
            _apiKeyRepository.Value[name] = apiKey;
            
            UpdateCharacterList(name, apiKey);
        }

        public void UnregisterKey(string apiKey) {
            var keyToRemove = _apiKeyRepository.Value.FirstOrDefault(x =>
                x.Value.Equals(apiKey, StringComparison.InvariantCultureIgnoreCase) ||
                x.Value.StartsWith(apiKey, StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrEmpty(keyToRemove.Key)) {
                _apiKeyRepository.Value.Remove(keyToRemove.Key);
            }
        }

        internal string[] GetKeys() {
            return _apiKeyRepository.Value.Select((setting) => setting.Value).ToArray();
        }

        private void UpdateCharacterList(string keyName, string keyValue) {
            GetCharacters(GetConnection(keyValue)).ContinueWith((charactersResponse) => {
                if (charactersResponse.Result != null) {
                    foreach (string characterId in charactersResponse.Result) {
                        _characterRepository.AddOrUpdate(characterId, keyValue, (k, o) => keyValue);
                    }

                    Logger.Info("Associated API key {keyName} with characters: {charactersList}", keyName, string.Join(", ", charactersResponse.Result));
                } else {
                    Logger.Warn(charactersResponse.Exception, "Failed to get list of associated characters for API key {keyName}.", keyName);
                }

                UpdateActiveApiKey();
            });
        }

        private async Task<List<string>> GetCharacters(ManagedConnection connection) {
            return (await connection.Client.V2.Characters.IdsAsync()).ToList();
        }

        internal async Task<string> RequestPrivilegedSubtoken(IEnumerable<TokenPermission> permissions, int days) {
            return await RequestSubtoken(_privilegedConnection, permissions, days);
        }

        public async Task<string> RequestSubtoken(ManagedConnection connection, IEnumerable<TokenPermission> permissions, int days) {
            try {
                return (await connection.Client
                                        .V2.CreateSubtoken
                                        .WithPermissions(permissions)
                                        .Expires(DateTime.UtcNow.AddDays(days))
                                        .GetAsync()).Subtoken;
            } catch (InvalidAccessTokenException ex) {
                Logger.Warn(ex, "The provided API token is invalid and can not be used to request a subtoken.");
            } catch (UnexpectedStatusException ex) {
                Logger.Warn(ex, "The provided API token could not be used to request a subtoken.");
            }

            return "";
        }

        #endregion

        public ManagedConnection GetConnection(string accessToken) {
            return _cachedConnections.GetOrAdd(accessToken, (token) => new ManagedConnection(token, _sharedTokenBucketMiddleware, _sharedWebCache, _sharedRenderCache));
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

    }
}