using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Blish_HUD.LocalDb {
    /// <summary>
    /// Generic collection access.
    /// </summary>
    public interface IDbCollection {
        /// <summary>
        /// Count the number of items in this collection.
        /// </summary>
        /// <returns>The number of items.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Count the number of items in this collection.
        /// </summary>
        /// <returns>The number of items.</returns>
        Task<long> LongCountAsync();
    }

    /// <summary>
    /// Typed collection access.
    /// </summary>
    /// <typeparam name="TId">The ID type.</typeparam>
    /// <typeparam name="TItem">The item type.</typeparam>
    public interface IDbCollection<TId, TItem> : IDbCollection
        where TId : notnull
        where TItem : class
    {
        /// <summary>
        /// Fast item lookup by ID.
        /// </summary>
        /// <param name="id">The ID to look for.</param>
        /// <returns>
        /// The item in question, or <c>null</c> if the item is not found. Will also return null if the collection is
        /// not available.
        /// </returns>
        Task<TItem?> GetAsync(TId id);

        /// <summary>
        /// Get all items.
        /// </summary>
        /// <returns>All items in the collection.</returns>
        Task<IEnumerable<TItem>> AllAsync();

        /// <summary>
        /// Slower item lookup by json query.
        /// </summary>
        /// <param name="field">The path to the json field to query. E.g.: <c>"facts[0].icon"</c>.</param>
        /// <param name="value">The value to look for.</param>
        /// <returns>All matching items.</returns>
        Task<IEnumerable<TItem>> JsonQueryAsync(string field, object value);
    }

    /// <summary>
    /// Collection metadata access.
    /// </summary>
    public interface IMetaCollection {
        /// <summary>
        /// <c>true</c> if the collection has loaded, successfully or not.
        /// To verify whether the collection loaded successfully or not, check <see cref="IsAvailable"/> instead.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// <c>true</c> if the collection has loaded successfully.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Waits until <see cref="IsLoaded"/> is <c>true</c>.
        /// </summary>
        /// <returns></returns>
        Task WaitUntilLoaded();
    }

    internal partial class DbHandler {
        private class LocalCollection<TId, TItem> : IDbCollection<TId, TItem>
            where TId : notnull
            where TItem : class
        {
            private readonly SQLiteAsyncConnection _db;
            private readonly string _tableName;

            public LocalCollection(SQLiteAsyncConnection db, string tableName) {
                _db = db;
                _tableName = tableName;
            }

            private async Task<IEnumerable<TItem>> SelectWhere(string where, params object[] args) {
                var json = await _db.QueryScalarsAsync<string>(
                    $"SELECT json({SQLiteContext.DATA_COLUMN})\n" +
                    $"FROM `{_tableName}`\n" +
                    $"WHERE {where}", args);
                return json.Select(SQLiteContext.Deserialize<TItem>);
            }

            public async Task<int> CountAsync()
                => await _db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM `{_tableName}`");

            public async Task<long> LongCountAsync()
                => await _db.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM `{_tableName}`");

            public async Task<TItem?> GetAsync(TId id)
                => (await SelectWhere($"{SQLiteContext.ID_COLUMN} = ?", id)).FirstOrDefault();

            public async Task<IEnumerable<TItem>> AllAsync()
                => await SelectWhere("1 = 1");

            public async Task<IEnumerable<TItem>> JsonQueryAsync(string path, object value)
                => await SelectWhere($"json_extract({SQLiteContext.DATA_COLUMN}, '$.{path}') LIKE ?", value);
        }
    }
}
