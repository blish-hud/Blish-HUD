﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;
using System.Threading.Tasks;
using Blish_HUD.Gw2WebApi;
using Blish_HUD.Settings;
using Gw2Sharp;
using Gw2Sharp.WebApi.Http;

namespace Blish_HUD {

    public class Gw2WebApiService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<Gw2WebApiService>();

        private const string GW2WEBAPI_SETTINGS = "Gw2ApiConfiguration";

        private const string SETTINGS_ENTRY_APIKEYS     = "ApiKeyRepository";
        private const string SETTINGS_ENTRY_PERMISSIONS = "Permissions";

        #region Cache Handling

        private ICacheMethod _sharedWebCache;
        private ICacheMethod _sharedRenderCache;

        private void InitCache() {
            _sharedWebCache    = new MemoryCacheMethod();
            _sharedRenderCache = new MemoryCacheMethod();
        }

        #endregion

        #region Init Cache, Connection, & Client

        private ManagedConnection _anonymousConnection;
        private ManagedConnection _privilegedConnection;

        private void CreateInternalConnection() {
            InitCache();

            _anonymousConnection  = new ManagedConnection(string.Empty, _sharedWebCache, _sharedRenderCache, TimeSpan.MaxValue);
            _privilegedConnection = new ManagedConnection(string.Empty, _sharedWebCache, _sharedRenderCache, TimeSpan.MaxValue);
        }

        public ManagedConnection AnonymousConnection => _anonymousConnection;

        #endregion

        private ConcurrentDictionary<string, string>            _characterRepository;
        private ConcurrentDictionary<string, TokenPermission[]> _permissionDetails;

        private ConcurrentDictionary<string, ManagedConnection> _cachedConnections;

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

            _characterRepository = new ConcurrentDictionary<string, string>();

            _cachedConnections = new ConcurrentDictionary<string, ManagedConnection>();

            Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacterOnNameChanged;
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
            foreach (var key in _apiKeyRepository) {
                UpdateCharacterList((SettingEntry<string>)key);
            }
        }

        #region API Management

        public void RegisterKey(string name, string key) {
            SettingEntry<string> registeredKey = _apiKeyRepository.DefineSetting(name, key);

            registeredKey.Value = key;

            UpdateCharacterList(registeredKey);
        }

        private void UpdateCharacterList(SettingEntry<string> definedKey) {
            GetCharacters(GetConnection(definedKey.Value)).ContinueWith((charactersResponse) => {
                if (charactersResponse.Result != null) {
                    foreach (string characterId in charactersResponse.Result) {
                        _characterRepository.AddOrUpdate(characterId, definedKey.Value, (k, o) => definedKey.Value);
                    }

                    Logger.Info("Associated API key {keyName} with characters: {charactersList}", definedKey.EntryKey, string.Join(", ", charactersResponse.Result));
                } else {
                    Logger.Warn(charactersResponse.Exception, "Failed to get list of associated characters for API key {keyName}.", definedKey.EntryKey);
                }

                UpdateActiveApiKey();
            });
        }

        private async Task<List<string>> GetCharacters(ManagedConnection connection) {
            return (await connection.Client.V2.Characters.IdsAsync()).ToList();
        }

        private async Task<TokenInfo> GetTokenInfo(ManagedConnection connection) {
            return await connection.Client.V2.TokenInfo.GetAsync();
        }

        private async Task<Account> GetAccount(ManagedConnection connection) {
            return await connection.Client.V2.Account.GetAsync();
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
            return _cachedConnections.GetOrAdd(accessToken, (token) => new ManagedConnection(token, _sharedWebCache, _sharedRenderCache));
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            
        }

    }
}