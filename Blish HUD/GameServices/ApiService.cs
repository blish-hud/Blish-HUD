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
        public static string SETTINGS_ENTRY_PERMS = "ModulePermissions";
        public static string PLACEHOLDER_KEY = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXXXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
        public static TokenPermission[] ALL_PERMISSIONS = {
            TokenPermission.Account,
            TokenPermission.Inventories,
            TokenPermission.Characters,
            TokenPermission.Tradingpost,
            TokenPermission.Wallet,
            TokenPermission.Unlocks,
            TokenPermission.Pvp,
            TokenPermission.Builds,
            TokenPermission.Progression,
            TokenPermission.Guilds
        };

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
        protected override void Load() {
            if (!SettingsService.Settings.CoreSettings.Entries.ContainsKey(SETTINGS_ENTRY))
            {
                SettingsService.Settings.CoreSettings.DefineSetting(SETTINGS_ENTRY, new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), false, "Stored Guild Wars 2 API Keys per Guid.");
            }
            foreach (KeyValuePair<Guid, string> entry in SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>>(SETTINGS_ENTRY).Value)
            {
                this.RegisterCharacters(entry.Value);
            }
            this.AddNewApiModules();
        }
        private void AddNewApiModules()
        {
            if (!SettingsService.Settings.CoreSettings.Entries.ContainsKey(SETTINGS_ENTRY_PERMS))
            {
                SettingsService.Settings.CoreSettings.DefineSetting(SETTINGS_ENTRY_PERMS, new Dictionary<string, TokenPermission[]>(), new Dictionary<string, TokenPermission[]>(), false, "Module API permissions");
            }
            var apiModules = ModuleService.Module.AvailableModules.Where(x => x.GetModuleInfo().Permissions != null);
            if (apiModules.Count() <= 0) {
                System.Console.WriteLine(
                    "╔════════════════════╣ ApiService ╠══════════════════╗\n║\n" +
                    "║None of the registered modules require the ApiService.\n║\n" +
                    "╚════════════════════════════════════════════════════╝"
                ); return; }

            var saved = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<string, TokenPermission[]>>(ApiService.SETTINGS_ENTRY_PERMS);
            var value = saved.Value;
            foreach (var entry in apiModules)
            {
                string nSpace = entry.GetModuleInfo().Namespace;
                if (!value.ContainsKey(nSpace))
                {
                    value.Add(nSpace, entry.GetModuleInfo().Permissions);
                }
            }
            var defaultValue = saved.DefaultValue;
            bool exposeAsSetting = saved.ExposedAsSetting;
            string description = saved.Description;

            SettingsService.Settings.CoreSettings.DefineSetting(ApiService.SETTINGS_ENTRY_PERMS, value, defaultValue, exposeAsSetting, description);
        }
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
        private TokenPermission[] GetPermissions(string apiKey)
        {
            var permissions = GetTokenInfo(apiKey).Permissions.List;
            var _out = new TokenPermission[permissions.Count()];
            for (int i = 0; i < _out.Length - 1; i++)
            {
                _out[i] = permissions[i];
            }
            return _out;
        }
        private static List<string> GetCharacters(string apiKey)
        {
            Gw2WebApiClient tempClient = new Gw2WebApiClient(new Connection(apiKey, Locale.English));
            var charactersResponse = tempClient.V2.Characters.IdsAsync();
            charactersResponse.Wait();
            return charactersResponse.Result.ToList();
        }
        private static TokenInfo GetTokenInfo(string apiKey)
        {
            if (!IsKeyValid(apiKey)) throw new ArgumentException("Invalid API key!");
            Gw2WebApiClient tempClient = new Gw2WebApiClient(new Connection(apiKey));
            var tokenInfoResponse = tempClient.V2.TokenInfo.GetAsync();
            tokenInfoResponse.Wait();
            return tokenInfoResponse.Result;
        }
        private static Account GetAccount(string apiKey)
        {
            if (!IsKeyValid(apiKey)) throw new ArgumentException("Invalid API key!");
            Gw2WebApiClient tempClient = new Gw2WebApiClient(new Connection(apiKey));
            var accountResponse = tempClient.V2.Account.GetAsync();
            accountResponse.Wait();
            return accountResponse.Result;
        }
        private string CreateSubtoken(TokenPermission[] permissions, int days)
        {
            if (!Connected) return null;
            if (!HasPermissions(permissions)) return null;

            var subTokenResponse = this.Client.V2.CreateSubtoken
                .WithPermissions(permissions)
                .Expires(DateTime.Now.AddDays((days < 1) ? 1 : (days > 7) ? 7 : days))
                .GetAsync();
            subTokenResponse.Wait();
            return subTokenResponse.Result.Subtoken;
        }
        private static string GetKeyById(string id)
        {
            if (!Regex.IsMatch(id, "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"))
                throw new ArgumentException("Pattern mismatch! Not an Id of an Guild Wars 2 API key.");

            Dictionary<Guid, string> apiKeys = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<Guid, string>>(ApiService.SETTINGS_ENTRY).Value;

            return apiKeys.FirstOrDefault(i => i.Value.Contains(id)).Value;
        }
        /// <summary>
        /// Returns the Guid of the specified Guild Wars 2 API key. Required permission: Account.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
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
        /// Returns an array containing the USER set permissions of the specified module.
        /// </summary>
        /// <param name="nSpace"></param>
        /// <returns></returns>
        public static TokenPermission[] GetModulePermissions(Modules.IModule module)
        {
            string nSpace = module.GetModuleInfo().Namespace;
            var saved = SettingsService.Settings.CoreSettings.GetSetting<Dictionary<string, TokenPermission[]>>(ApiService.SETTINGS_ENTRY_PERMS).Value;
            return saved.ContainsKey(nSpace) ? saved[nSpace] : null;
        }
        /// <summary>
        /// Checks if the active API client still conforms a character name in the repository. If not, disposes the client.
        /// </summary>
        /// <returns>False, if the client has been disposed off.</returns>
        public bool Invalidate()
        {
            if (!Connected) return false;
            foreach (string name in GetCharacters(Client.Connection.AccessToken))
            {
                if (!CharacterRepository.ContainsKey(name))
                {
                    this.Client = null;
                    return false;
                }
            }
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
        /// Registers a new Guild Wars 2 API key, overwriting an existing key of the same account.
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
        /// <summary>
        /// Gets a subtoken for the specified module.
        /// </summary>
        /// <param name="module">The module to get the subtoken for.</param>
        /// <param name="days">Expiration in days from the moment of its creation (max. 7 days).</param>
        /// <returns>A subtoken.</returns>
        public string GetModuleToken(Modules.IModule module, int days)
        {
            return this.CreateSubtoken(GetModulePermissions(module), days);
        }
    }
}