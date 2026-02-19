using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal partial class DbHandler {
        private interface ILoadCollection : IMetaCollection {
            Type IdType { get; }
            string TableName { get; }
            Version? CurrentVersion { get; }

            Task Unload(SQLiteContext db, CancellationToken ct);
            Task<bool> Load(SQLiteContext db, CancellationToken ct);
        }

        private partial class Collection<TId, TItem> : ILoadCollection
            where TId : notnull
            where TItem : class {
            private static readonly Logger _logger = Logger.GetLogger<Collection<TId, TItem>>();

            public Type IdType { get; } = typeof(TId);
            public string TableName { get; }

            public Version? CurrentVersion => _handler._meta.GetVersion(TableName);

            public bool IsLoaded => CurrentVersion != null;
            public bool IsAvailable => CurrentVersion?.IsValid == true;

            private readonly DbHandler _handler;
            private readonly Func<CancellationToken, Task<IEnumerable<(TId id, TItem item)>>> _load;

            public Collection(
                DbHandler handler,
                string tableName,
                Func<CancellationToken, Task<IEnumerable<(TId id, TItem item)>>> load
            ) {
                _handler = handler;
                TableName = tableName;
                _load = load;
            }

            public Task WaitUntilLoaded() {
                var tcs = new TaskCompletionSource<object>();

                _handler.CollectionLoaded += loaded;

                void loaded(IMetaCollection collection) {
                    if (collection != this) {
                        return;
                    }

                    _handler.CollectionLoaded -= loaded;
                    tcs.SetResult(null!);
                }

                return IsLoaded
                    ? Task.CompletedTask
                    : tcs.Task;
            }

            public IDbCollection<TId, TItem> Access(SQLiteAsyncConnection db) {
                return new LocalCollection<TId, TItem>(db, TableName);
            }

            public async Task Unload(SQLiteContext db, CancellationToken _) {
                await db.Connection.ExecuteAsync($"DELETE FROM `{TableName}`");
            }

            public async Task<bool> Load(SQLiteContext db, CancellationToken ct) {
                try {
                    var values = await _load(ct);
                    await db.Connection.RunInTransactionAsync(transaction => {
                        transaction.ExecuteScalar<string>("PRAGMA locking_mode = EXCLUSIVE");
                        transaction.ExecuteScalar<string>("PRAGMA journal_mode = MEMORY");

                        // Delete all content from table first
                        transaction.Execute($"DELETE FROM `{TableName}`");

                        // Insert all items into the table
                        foreach (var (id, value) in values.OrderBy(x => x.id)) {
                            transaction.Execute(
                                $"INSERT INTO `{TableName}` ({SQLiteContext.ID_COLUMN}, {SQLiteContext.DATA_COLUMN})\n" +
                                $"VALUES (?, jsonb(?))", id, SQLiteContext.Serialize(value));
                        }
                    });

                    return true;
                } catch (Exception e) {
                    _logger.Warn(e, $"Failed to load cache for {TableName}");
                    return false;
                }
            }
        }
    }
}
