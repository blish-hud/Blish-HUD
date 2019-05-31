using System;
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
                SettingsService.Settings.CoreSettings.DefineSetting(SETTINGS_ENTRY, new List<string>(), new List<string>(), false, "Characters corresponding to an Guild Wars 2 API key.");
            }
            else
            {
                foreach (string key in SettingsService.Settings.CoreSettings.GetSetting<List<string>>(SETTINGS_ENTRY).Value)
                {
                    this.UpdateCharacters(key);
                }
            }
        }
        protected override void Update(GameTime time) {
            if (Gw2Mumble.Available)
            {
                string currentCharacter = Gw2Mumble.MumbleBacking.Identity.Name;
                if (CharacterRepository.ContainsKey(currentCharacter)) {
                    if (!Connected || CharacterRepository[currentCharacter] != Client.Connection.AccessToken)
                        this.StartClient(CharacterRepository[currentCharacter]);
                }
            };
            base.DoUpdate(time);
        }
        protected override void Unload() { /* NOOP */ }
        protected override void Load() { /* NOOP */ }

        /// <summary>
        /// Creates Key (character) Value (api key) pairs from the given api key and merges them into the repository.
        /// </summary>
        /// <param name="apiKey">An api key.</param>
        /// <returns></returns>
        public bool UpdateCharacters(string apiKey)
        {
            if (!IsKeyValid(apiKey)) return false;

            if (!HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }, apiKey)) return false;

            Gw2WebApiClient testClient = new Gw2WebApiClient(new Connection(apiKey, Locale.English));

            var charactersResponse = testClient.V2.Characters.IdsAsync();
            charactersResponse.Wait();
            var new_characterRepository = new Dictionary<string, string>();
            foreach (string name in charactersResponse.Result)
            {
                // Bind characters to the api key.
                new_characterRepository.Add(name, apiKey);
            }
            // Add newly fetched characters to the repository.
            DictionaryExtension.MergeIn<string, string>(CharacterRepository, true, new_characterRepository);
            return true;
        }
        /// <summary>
        /// Creates a new main client API connection for the given api key.
        /// </summary>
        /// <param name="apiKey">Main API key.</param>
        private void StartClient(string apiKey)
        {
            if (IsKeyValid(apiKey))
            {
                this.Client = new Gw2WebApiClient(new Connection(apiKey, Locale.English));
                if (HasPermissions(new[]{ TokenPermission.Account }))
                {
                    // There is nothing wrong with having a Guid up immediately.
                    var accountResponse = this.Client.V2.Account.GetAsync();
                    accountResponse.Wait();
                    this.GUID = accountResponse.Result.Id;
                }
            }
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
        public static Dictionary<string, string> GetKeyRepository()
        {
            Dictionary<string, string> apiKeys = new Dictionary<string, string>();
            if (SettingsService.Settings.CoreSettings.Entries.ContainsKey(ApiService.SETTINGS_ENTRY))
            {
                List<string> keyRepo = SettingsService.Settings.CoreSettings.GetSetting<List<string>>(ApiService.SETTINGS_ENTRY).Value;

                foreach (string key in keyRepo)
                {
                    if (!IsKeyValid(key)) continue;
                    Gw2WebApiClient testClient = new Gw2WebApiClient(new Connection(key));
                    var tokeninfoResponse = testClient.V2.TokenInfo.GetAsync();
                    tokeninfoResponse.Wait();
                    string tokenName = tokeninfoResponse.Result.Name;
                    var accountResponse = testClient.V2.Account.GetAsync();
                    accountResponse.Wait();
                    string accountName = accountResponse.Result.Name;
                    tokenName = tokenName + " (" + accountName + ')';
                    if (!apiKeys.ContainsKey(tokenName))
                        apiKeys.Add(tokenName, tokeninfoResponse.Result.Id);
                }

            }
            return apiKeys;
        }
        /// <summary>
        /// Gets all the permissions of the provided Api Client.
        /// </summary>
        /// <param name="client">The api client.</param>
        /// <returns></returns>
        private List<TokenPermission> GetPermissions(Gw2WebApiClient client)
        {
            var tokenInfoResponse = client.V2.TokenInfo.GetAsync();
            tokenInfoResponse.Wait();
            List<TokenPermission> enumList = new List<TokenPermission>();
            foreach (TokenPermission x in tokenInfoResponse.Result.Permissions.List){
                enumList.Add(x);
            };
            return enumList;
        }
        /// <summary>
        /// Checks if the Api Client has the given permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="apiKey">Optional: An api key to check permissions off.</param>
        /// <returns>bool</returns>
        public bool HasPermissions(TokenPermission[] permissions, string apiKey = null)
        {
            Gw2WebApiClient testClient = this.Client;

            if (IsKeyValid(apiKey))
                testClient = new Gw2WebApiClient(new Connection(apiKey));

            var savedPermissions = this.GetPermissions(testClient);
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
        public static void RemoveKey(string id)
        {
            if (SettingsService.Settings.CoreSettings.Entries.ContainsKey(ApiService.SETTINGS_ENTRY))
            {
                List<string> apiKeys = SettingsService.Settings.CoreSettings.GetSetting<List<string>>(ApiService.SETTINGS_ENTRY).Value;
                string key = apiKeys.Find(x => x.Contains(id));
                if (key != null) apiKeys.Remove(key);
            }
        }
        /// <summary>
        /// Gets a subtoken with finite life cycle and fewer or equal permissions as the main key provided to BlishHUD.
        /// </summary>
        /// <param name="permissions">Endpoint permissions using Gw2Sharp.WebApi.V2.Models.</param>
        /// <param name="days">Expiration in days from the moment of its creation (max. 7 days).</param>
        /// <returns>A subtoken.</returns>
        public string CreateSubtoken(TokenPermission[] permissions, int days)
        {
            if (!Connected) return null;
            var subTokenResponse = this.Client.V2.CreateSubtoken
                .WithPermissions(permissions)
                .Expires(DateTime.Now.AddDays((days < 1) ? 1 : (days > 7) ? 7 : days))
                .GetAsync();
            subTokenResponse.Wait();
            return subTokenResponse.Result.Subtoken;
        }
    }
}