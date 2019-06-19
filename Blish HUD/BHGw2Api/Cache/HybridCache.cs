using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Blish_HUD.BHGw2Api.Cache {

    public enum CacheDurationType {
        Absolute,
        Sliding
    }

    public class HybridCache : MemoryCache {

        private const string CACHE_ENDPOINT_EXTENSION = "";
        private const string CACHE_ENTRY_EXTENSION = ".bin";

        public delegate Task<T> GetLiveEndpointResultDelegate<T>(string identifier, string @namespace) where T : class;
        public delegate Task<IEnumerable<T>> GetLiveEndpointResultsDelegate<T>() where T : class;

        public string CacheDirectory { get; set; }

        public HybridCache(string name, string cacheDirectory, NameValueCollection config = null) : base(name, config) {
            this.CacheDirectory = cacheDirectory;
        }

        public async Task<T> GetFromFsCache<T>(string endpoint, string identifier, GetLiveEndpointResultDelegate<T> cacheSetCall, DateTimeOffset cacheExpiration, CacheDurationType cacheDurationType = CacheDurationType.Absolute) where T : class {
            string niceEndpointName = GetEndpointNiceName(endpoint);

            // Cache it to the file system, if that's enabled or available
            if (cacheExpiration.Offset.TotalSeconds > 0 && !string.IsNullOrEmpty(niceEndpointName) && !string.IsNullOrEmpty(this.CacheDirectory)) {
                string endpointCacheRoot = $"{niceEndpointName}{CACHE_ENDPOINT_EXTENSION}";

                // TODO: For now while we aren't updating fs cache in this function
                if (!File.Exists(Path.Combine(this.CacheDirectory, endpointCacheRoot))) return null;

                using (var endpointFileStream = new FileStream(Path.Combine(this.CacheDirectory, endpointCacheRoot), FileMode.Open)) {
                    using (var endpointArchive = new ZipArchive(endpointFileStream, ZipArchiveMode.Update)) {
                        string entryName = identifier + CACHE_ENTRY_EXTENSION;

                        var entryFind = endpointArchive.GetEntry(entryName);

                        // Where we will store the result
                        T result;

                        if (entryFind == null || entryFind.LastWriteTime.Subtract(DateTime.Now) > cacheExpiration.Offset) {
                            entryFind?.Delete();

                            result = await cacheSetCall.Invoke(identifier, endpoint);
                            
                            // TODO: Instead of doing this now, queue it and do them all at once to avoid IO errors
                            // Add new cache entry into zip
                            //entryFind = endpointArchive.CreateEntry(entryName, CompressionLevel.Fastest);

                            //using (var entryStream = entryFind.Open()) {
                            //    ProtoBuf.Serializer.Serialize(entryStream, result);
                            //}
                        } else {
                            result = ProtoBuf.Serializer.Deserialize<T>(entryFind.Open());

                            // If cache duration mode is sliding, then touch the timestamp to update it from this access
                            if (cacheDurationType == CacheDurationType.Sliding)
                                entryFind.LastWriteTime = DateTimeOffset.Now;
                        }

                        return result;
                    }
                }
            }
            
            return null;
        }

        public async Task<T> GetEntryFromCache<T>(string endpoint, string identifier, GetLiveEndpointResultDelegate<T> cacheSetCall, DateTimeOffset cacheExpiration, CacheDurationType cacheDurationType = CacheDurationType.Absolute, bool persistInMemory = true, bool persistOnDisk = false) where T : class {
            // Check memory cache first
            var responseItem = persistInMemory ? Get(identifier, endpoint) as T : null;

            // If not in memory cache, check file system cache
            if (responseItem == null) {
                responseItem = persistOnDisk ? await GetFromFsCache(endpoint, identifier, cacheSetCall, cacheExpiration, cacheDurationType) : null; 

                if (responseItem == null)
                    return await cacheSetCall.Invoke(identifier, endpoint);

                if (persistInMemory) {
                    var policy = new CacheItemPolicy();

                    if (cacheDurationType == CacheDurationType.Absolute)
                        policy.AbsoluteExpiration = cacheExpiration;
                    else if (cacheDurationType == CacheDurationType.Sliding)
                        policy.SlidingExpiration = cacheExpiration.Offset;

                    base.Set(GetCacheItemFqn(identifier, endpoint), responseItem, policy);
                }
            }

            return responseItem;
        }

        public override void Set(CacheItem item, CacheItemPolicy policy) {
            Set(item.Key, item.Value, policy, item.RegionName);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null) {
            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null) {
            base.Set(GetCacheItemFqn(key, regionName), value, policy);
        }

        public override CacheItem GetCacheItem(string key, string regionName = null) {
            var temporary = base.GetCacheItem(GetCacheItemFqn(key, regionName));
            return new CacheItem(key, temporary.Value, regionName);
        }

        public override object Get(string key, string regionName = null) {
            return base.Get(GetCacheItemFqn(key, regionName));
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities => (base.DefaultCacheCapabilities | DefaultCacheCapabilities.CacheRegions);

        public bool AddMany(IEnumerable<ApiItem> items, string endpoint = null, TimeSpan endpointCacheDuration = default) {
            var cacheDuration = new DateTimeOffset(DateTime.UtcNow).AddSeconds(endpointCacheDuration.Seconds);

            endpoint = GetEndpointNiceName(endpoint);

            // Cache it to the file system, if that's enabled or available
            if (endpointCacheDuration.TotalMilliseconds > 0 && !string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(this.CacheDirectory)) {
                //BinaryFormatter binSer = new BinaryFormatter();

                string endpointCacheRoot = $"{endpoint}{CACHE_ENDPOINT_EXTENSION}";

                using (var endpointFileStream = new FileStream(Path.Combine(this.CacheDirectory, endpointCacheRoot), FileMode.OpenOrCreate)) {
                    using (var endpointArchive = new ZipArchive(endpointFileStream, ZipArchiveMode.Update)) {

                        foreach (var apiItem in items) {
                            string entryKey = apiItem.CacheKey() + CACHE_ENTRY_EXTENSION;

                            // Delete existing cache value if it already exists
                            endpointArchive.GetEntry(entryKey)?.Delete();

                            // Add new cache entry into zip
                            var entryCache = endpointArchive.CreateEntry(entryKey, CompressionLevel.Fastest);
                            using (var entryStream = entryCache.Open()) {
                                // Proto-buf serialization cache
                                ProtoBuf.Serializer.Serialize(entryStream, apiItem);

                                // Binary serialization cache
                                //binSer.Serialize(entryStream, apiItem);

                                // Cache in JSON format
                                //byte[] rawEntry = ToJson(apiItem).GetBytes();
                                //entryStream.Write(rawEntry, 0, rawEntry.Length);
                            }

                            base.Add(
                                GetCacheItemFqn(apiItem.CacheKey(), endpoint),
                                apiItem,
                                cacheDuration
                            );
                        }

                    }

                }
            }

            // TODO: Base this return off of all of the values
            return true;
        }

        //public bool Add(ApiItem value, string endpoint = null, TimeSpan endpointCacheDuration = default(TimeSpan)) {
        //    return AddMany(new[] {value}, endpoint, endpointCacheDuration);
        //}

        private static string GetEndpointNiceName(string endpoint) {
            return
                (endpoint.Contains("?") ? endpoint.Split('?')[0] : endpoint)
                .Replace(@"/", ".")
                .Trim('.');
        }

        private static string GetCacheItemFqn(string itemName, string @namespace) {
            return $"r:{@namespace ?? "core"};k:{itemName}";
        }

    }
}
