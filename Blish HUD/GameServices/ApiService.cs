using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using System.Text.RegularExpressions;
namespace Blish_HUD
{
    public class ApiService : GameService
    {
        public static string SETTINGS_ENTRY = "ApiKeyRepository";
        private static string SETTINGS_ENTRY_PERMS = "ModulePermissions";
        public static string PLACEHOLDER_KEY = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXXXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
        /// <summary>
        /// The Globally Unique Identifier from the currently connected api key.
        /// </summary>
        public Guid GUID { get; private set; }
        /// <summary>
        /// Checks if the ApiService has a main api client up and running that can be used to create subtokens.
        /// </summary>
        public bool Connected { get => this.Client != null; }

        private Gw2WebApiClient Client;
        private Dictionary<string, string> CharacterRepository;

        protected override void Initialize() {
            CharacterRepository = new Dictionary<string, string>();
            if (!SettingsService.Settings.CoreSettings.Entries.ContainsKey(SETTINGS_ENTRY))
            {
                SettingsService.Settings.CoreSettings.DefineSetting(SETTINGS_ENTRY, new Dictionary<Guid,string>(), new Dictionary<Guid, string>(), false, "Stored Guild Wars 2 API Keys per Guid.");
            }
            else
            {
                foreach (KeyValuePair<Guid, string> entry in SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>> (SETTINGS_ENTRY).Value)
                {
                    this.RegisterCharacters(entry.Value);
                }
            }
        }
        protected override void Update(GameTime time) {
            if (Gw2Mumble.Available)
            {
                string currentCharacter = Gw2Mumble.MumbleBacking.Identity.Name;
                if (CharacterRepository.ContainsKey(currentCharacter)) {
                    if (!Connected)
                        this.StartClient(CharacterRepository[currentCharacter]);
                    else if (CharacterRepository[currentCharacter] != Client.Connection.AccessToken)
                        this.StartClient(CharacterRepository[currentCharacter]);
                }
            };
        }
        protected override void Unload() { /* NOOP */ }
        protected override void Load() { /* NOOP */ }
        private void StartClient(string apiKey)
        {
            if (!IsKeyValid(apiKey)) return;

            this.Client = new Gw2WebApiClient(new Connection(apiKey, Locale.English));
            this.GUID = GetGuid(apiKey);

        }
        private void RegisterCharacters(string apiKey)
        {
            var new_characterRepository = new Dictionary<string, string>();
            foreach (string name in GetCharacters(apiKey))
            {
                // Bind characters to the api key.
                new_characterRepository.Add(name, apiKey);
            }
            // Add newly fetched characters to the repository.
            DictionaryExtension.MergeIn<string, string>(CharacterRepository, true, new_characterRepository);
        }
        private void RemoveCharacters(string apiKey)
        {
            foreach (string name in GetCharacters(apiKey))
            {
                CharacterRepository.Remove(name);
            }
        }
        private static List<string> GetCharacters(string apiKey)
        {
            Gw2WebApiClient testClient = new Gw2WebApiClient(new Connection(apiKey, Locale.English));

            var charactersResponse = testClient.V2.Characters.IdsAsync();
            charactersResponse.Wait();
            return charactersResponse.Result.ToList();
        }
        private List<TokenPermission> GetPermissions(string apiKey)
        {
            List<TokenPermission> enumList = new List<TokenPermission>();
            foreach (TokenPermission x in GetTokenInfo(apiKey).Permissions.List)
            {
                enumList.Add(x);
            };
            return enumList;
        }
        private static TokenInfo GetTokenInfo(string apiKey)
        {
            if (IsKeyValid(apiKey)) throw new ArgumentException("Invalid API key!");
            Gw2WebApiClient tempClient = new Gw2WebApiClient(new Connection(apiKey));
            var tokenInfoResponse = tempClient.V2.TokenInfo.GetAsync();
            tokenInfoResponse.Wait();
            return tokenInfoResponse.Result;
        }
        private static Account GetAccount(string apiKey)
        {
            if (IsKeyValid(apiKey)) throw new ArgumentException("Invalid API key!");
            Gw2WebApiClient tempClient = new Gw2WebApiClient(new Connection(apiKey));
            var accountResponse = tempClient.V2.Account.GetAsync();
            accountResponse.Wait();
            return accountResponse.Result;
        }
        public Guid GetGuid(string apiKey)
        {
            Gw2WebApiClient tempClient = new Gw2WebApiClient(new Connection(apiKey, Locale.English));
            if (!HasPermissions(new[] { TokenPermission.Account }, apiKey))
                throw new ArgumentException("Insufficient permissions for retrieving Guid! Required: Account.");
            var accountResponse = tempClient.V2.Account.GetAsync();
            accountResponse.Wait();
            return accountResponse.Result.Id;
        }
        /// <summary>
        /// Checks if active api key conforms a character name in the repository.
        /// </summary>
        public bool Invalidate()
        {
            if (!Connected) return false;
            foreach (string name in GetCharacters(Client.Connection.AccessToken))
            {
                if (CharacterRepository.ContainsKey(name))
                    continue;
                return false;
            }
            this.Client = null;
            return true;
        }
        /// <summary>
        /// Checks if the provided string is a valid Guild Wars 2 API key.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns>bool</returns>
        public static bool IsKeyValid(string apiKey)
        {
            return apiKey != null ? Regex.IsMatch(apiKey, @"^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{20}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$") : false;
        }
        /// <summary>
        /// Finds the api key matching the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetKeyById(string id)
        {
            if (!Regex.IsMatch(id, "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"))
                throw new ArgumentException("Pattern mismatch! Not an Id of an Guild Wars 2 API key.");

            Dictionary<Guid, string> apiKeys = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>>(ApiService.SETTINGS_ENTRY).Value;

            return apiKeys.FirstOrDefault(i => i.Value.Contains(id)).Value;
        }
        /// <summary>
        /// Returns a fool safe dictionary containing name and the first halfs of the actual keys currently in the settings.
        /// </summary>
        /// <returns>The fool safe dictionary.</returns>
        public static Dictionary<string, string> GetKeyIdRepository()
        {
            Dictionary<string, string> foolApiKeys = new Dictionary<string, string>();
            if (SettingsService.Settings.CoreSettings.Entries.ContainsKey(ApiService.SETTINGS_ENTRY))
            {
                Dictionary<Guid, string> keyRepo = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>>(ApiService.SETTINGS_ENTRY).Value;

                foreach (KeyValuePair<Guid, string> entry in keyRepo)
                {
                    if (!IsKeyValid(entry.Value)) continue;
                    TokenInfo tokenInfo = GetTokenInfo(entry.Value);
                    string new_entry = tokenInfo.Name + " (" + GetAccount(entry.Value).Name + ')';
                    if (!foolApiKeys.ContainsKey(new_entry))
                        foolApiKeys.Add(new_entry, tokenInfo.Id);
                }

            }
            return foolApiKeys;
        }
        /// <summary>
        /// Checks if the Api Client has the given permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="apiKey">Optional: An api key to check permissions off.</param>
        /// <returns>bool</returns>
        public bool HasPermissions(TokenPermission[] permissions, string apiKey = null)
        {
            var savedPermissions = this.GetPermissions(
                apiKey != null ? 
                apiKey 
                : this.Client.Connection.AccessToken);

            foreach (TokenPermission x in permissions)
            {
                if (!savedPermissions.Contains(x)) return false;
            }
            return true;
        }
        /// <summary>
        /// Finds a key where the first half matchs the given id and removes it from the settings.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveKey(string id)
        {
            if (SettingsService.Settings.CoreSettings.Entries.ContainsKey(ApiService.SETTINGS_ENTRY))
            {
                Dictionary<Guid, string> apiKeys = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>>(ApiService.SETTINGS_ENTRY).Value;
                string key = GetKeyById(id);
                if (key != null)
                {
                    apiKeys.Remove(apiKeys.First(i => i.Value.Contains(key)).Key);
                    RemoveCharacters(key);
                }
            }
        }
        /// <summary>
        /// Registers a new key that is not already saved in the settings.
        /// </summary>
        /// <param name="apiKey">The api key to register.</param>
        public void RegisterKey(string apiKey)
        {
            if (!IsKeyValid(apiKey)) return;

            SettingEntry<Dictionary<Guid, string>> entry = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>>(ApiService.SETTINGS_ENTRY);

            Dictionary<Guid, string> value = entry.Value;
            Dictionary<Guid, string> defaultValue = entry.DefaultValue;
            bool exposed = entry.ExposedAsSetting;
            string description = entry.Description;

            Guid guid = GetGuid(apiKey);

            if (value.ContainsKey(guid)){
                value[guid] = apiKey;
            }
            else
            {
                value.Add(guid, apiKey);
            }

            SettingsService.Settings.CoreSettings.DefineSetting(ApiService.SETTINGS_ENTRY, value, defaultValue, exposed, description);
            RegisterCharacters(apiKey);

        }
        private string CreateSubtoken(TokenPermission[] permissions, int days)
        {
            if (!Connected) return null;
            var subTokenResponse = this.Client.V2.CreateSubtoken
                .WithPermissions(permissions)
                .Expires(DateTime.Now.AddDays((days < 1) ? 1 : (days > 7) ? 7 : days))
                .GetAsync();
            subTokenResponse.Wait();
            return subTokenResponse.Result.Subtoken;
        }
        /// <summary>
        /// Gets a subtoken with finite life cycle and fewer or equal permissions as the active API connection.
        /// </summary>
        /// <param name="moduleNamespace">Namespace of the module.</param>
        /// <param name="days">Expiration in days from the moment of its creation (max. 7 days).</param>
        /// <returns>A subtoken.</returns>
        /*public string GetSubtoken()
        {
            if (!SettingsService.Settings.CoreSettings.Entries.ContainsKey(SETTINGS_ENTRY_PERMS))
            {
                SettingsService.Settings.CoreSettings.DefineSetting(SETTINGS_ENTRY, new Dictionary<string, TokenPermission[]>(), new Dictionary<string, TokenPermission[]>(), false, "Module API Permissions");
            }
            return null;
        }*/
    }
}