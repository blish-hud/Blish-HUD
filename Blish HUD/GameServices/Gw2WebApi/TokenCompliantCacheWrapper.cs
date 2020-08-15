using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blish_HUD.Gw2WebApi;
using Gw2Sharp.WebApi.Caching;

namespace Blish_HUD.Gw2WebApi {
    public class TokenCompliantCacheWrapper : ICacheMethod {

        private readonly ICacheMethod _cache;
        private readonly TokenBucket _bucket;

        public TokenCompliantCacheWrapper(ICacheMethod cacheMethod, TokenBucket tokenBucket) {
            _cache  = cacheMethod;
            _bucket = tokenBucket;
        }

        public async Task<CacheItem<T>> TryGetAsync<T>(string category, string id) {
            return await _cache.TryGetAsync<T>(category, id).ConfigureAwait(false);
        }

        public async Task SetAsync<T>(CacheItem<T> item) {
            await _cache.SetAsync(item).ConfigureAwait(false);
        }

        public async Task SetAsync<T>(string category, string id, T item, DateTimeOffset expiryTime) {
            await _cache.SetAsync(category, id, item, expiryTime).ConfigureAwait(false);
        }

        public async Task<IDictionary<string, CacheItem<T>>> GetManyAsync<T>(string category, IEnumerable<string> ids) {
            return await _cache.GetManyAsync<T>(category, ids).ConfigureAwait(false);
        }

        public async Task SetManyAsync<T>(IEnumerable<CacheItem<T>> items) {
            await _cache.SetManyAsync(items).ConfigureAwait(false);
        }

        public async Task<CacheItem<T>> GetOrUpdateAsync<T>(string category, string id, Func<Task<(T, DateTimeOffset)>> updateFunc) {
            return await _cache.GetOrUpdateAsync<T>(category, id, async () => await _bucket.ConsumeCompliant(updateFunc)).ConfigureAwait(false);
        }

        public async Task<CacheItem<T>> GetOrUpdateAsync<T>(string category, string id, DateTimeOffset expiryTime, Func<Task<T>> updateFunc) {
            return await _cache.GetOrUpdateAsync(category, id, expiryTime, async () => await _bucket.ConsumeCompliant(updateFunc)).ConfigureAwait(false);
        }

        public async Task<IList<CacheItem<T>>> GetOrUpdateManyAsync<T>(string category, IEnumerable<string> ids, Func<IList<string>, Task<(IDictionary<string, T>, DateTimeOffset)>> updateFunc) {
            return await _cache.GetOrUpdateManyAsync(category, ids, updateFunc).ConfigureAwait(false);
        }

        public async Task<IList<CacheItem<T>>> GetOrUpdateManyAsync<T>(string category, IEnumerable<string> ids, DateTimeOffset expiryTime, Func<IList<string>, Task<IDictionary<string, T>>> updateFunc) {
            return await _cache.GetOrUpdateManyAsync(category, ids, expiryTime, updateFunc).ConfigureAwait(false);
        }

        public async Task ClearAsync() {
            await _cache.ClearAsync().ConfigureAwait(false);
        }

        public void Dispose() {
            _cache.Dispose();
        }
    }
}
