using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.V2.Models;
using System.Text.RegularExpressions;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.Http;
using Gw2Sharp.WebApi.V2;

namespace Blish_HUD {

    public class Gw2WebApiService : GameService {

        private static Logger Logger = Logger.GetLogger<Gw2WebApiService>();

        private ICacheMethod _sharedWebCache;

        internal ICacheMethod GetWebCacheMethod() {
            return _sharedWebCache ?? (_sharedWebCache = new MemoryCacheMethod());
        }

        public IGw2WebApiV2Client SharedWebClient => GameService.Gw2Api.SharedApiClient.WebApi.V2;


        private Dictionary<string, string> _characterRepository;

        private SettingCollection                      _apiSettings;
        private SettingEntry<Dictionary<Guid, string>> _apiKeyRepository;

        protected override void Initialize() {
            //_apiSettings = Settings.RegisterRootSettingCollection(GW2API_SETTINGS);
        }

        private void DefineSettings(SettingCollection settings) {

        }

        protected override void Load() {
            
        }

        protected override void Unload() {
            
        }

        protected override void Update(GameTime gameTime) {
            
        }

    }
}