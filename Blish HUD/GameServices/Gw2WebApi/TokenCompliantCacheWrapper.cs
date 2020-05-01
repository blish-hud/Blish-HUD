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

        public async Task<CacheItem<T>> TryGetAsync<T>(string category, object id) {
            return await _cache.TryGetAsync<T>(category, id).ConfigureAwait(false);
        }

        public async Task SetAsync<T>(CacheItem<T> item) {
            await _cache.SetAsync(item).ConfigureAwait(false);
        }

        public async Task SetAsync<T>(string category, object id, T item, DateTimeOffset expiryTime) {
            await _cache.SetAsync(category, id, item, expiryTime).ConfigureAwait(false);
        }

        public async Task<IDictionary<object, CacheItem<T>>> GetManyAsync<T>(string category, IEnumerable<object> ids) {
            return await _cache.GetManyAsync<T>(category, ids).ConfigureAwait(false);
        }

        public async Task SetManyAsync<T>(IEnumerable<CacheItem<T>> items) {
            await _cache.SetManyAsync(items).ConfigureAwait(false);
        }

        public async Task<CacheItem<T>> GetOrUpdateAsync<T>(string category, object id, Func<Task<(T, DateTimeOffset)>> updateFunc) {
            return await _cache.GetOrUpdateAsync<T>(category, id, async () => await _bucket.ConsumeCompliant(updateFunc)).ConfigureAwait(false);
        }

        public async Task<CacheItem<T>> GetOrUpdateAsync<T>(string category, object id, DateTimeOffset expiryTime, Func<Task<T>> updateFunc) {
            return await _cache.GetOrUpdateAsync(category, id, expiryTime, async () => await _bucket.ConsumeCompliant(updateFunc)).ConfigureAwait(false);
        }

        public async Task<IList<CacheItem<T>>> GetOrUpdateManyAsync<T>(string category, IEnumerable<object> ids, Func<IList<object>, Task<(IDictionary<object, T>, DateTimeOffset)>> updateFunc) {
            return await _cache.GetOrUpdateManyAsync(category, ids, updateFunc).ConfigureAwait(false);
        }

        public async Task<IList<CacheItem<T>>> GetOrUpdateManyAsync<T>(string category, IEnumerable<object> ids, DateTimeOffset expiryTime, Func<IList<object>, Task<IDictionary<object, T>>> updateFunc) {
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
