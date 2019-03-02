using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api.Cache;
using Flurl.Http;
using Newtonsoft.Json;
using ProtoBuf;

namespace Blish_HUD.BHGw2Api {

    public struct EntryPairIdentifier {
        public string Region;
        public string Entry;

        public EntryPairIdentifier(string region, string entry) {
            this.Region = region;
            this.Entry = entry;
        }
    }

    [JsonObject, Serializable, ProtoContract]
    [ProtoInclude(1, typeof(Map))]
    [ProtoInclude(2, typeof(DyeColor))]
    [ProtoInclude(3, typeof(Landmark))]
    public abstract class ApiItem {

        public const string BASE_API_URL = "https://api.guildwars2.com/";

        private const string CACHE_SUBLOCATION = "api";

        private static HybridCache apiCache =
            new HybridCache("gw2api", Path.Combine(Settings.CacheLocation, CACHE_SUBLOCATION), null);

        protected delegate void HandleCallResultDelegate<in T>(IEnumerable<T> result) where T : ApiItem;

        public abstract string CacheKey();

        // TODO: Build into base class
        //protected abstract void LoadIndex();

        public static async Task<List<T>> CallForManyAsync<T>(string endpoint, TimeSpan cacheDuration = default(TimeSpan), bool persistInMemory = true) where T : ApiItem {
            // TODO: Add "WithAuth" extension to URL building
            // TODO: Add ".WithCulture()"
            List<T> responseItems = await BASE_API_URL.WithEndpoint(endpoint).WithTimeout(Settings.TimeoutLength).GetJsonAsync<List<T>>();

            // If caching enabled for this endpoint, add all results to cache
            if (persistInMemory && responseItems != null) {
                apiCache.AddMany(responseItems, endpoint, cacheDuration);
            }

            return responseItems;
        }

        protected static async Task<T> GetAsync<T>(string endpoint, string identifier, HybridCache.GetLiveEndpointResultDelegate<T> cacheSetCall,
            DateTimeOffset cacheDuration, CacheDurationType cacheDurationType = CacheDurationType.Absolute,
            bool persistInMemory = true, bool persistOnDisk = false) where T : class {
            return await apiCache.GetEntryFromCache<T>(endpoint, identifier, cacheSetCall, cacheDuration,
                cacheDurationType, persistInMemory, persistOnDisk);
        }
    }
}
