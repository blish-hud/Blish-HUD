using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal partial class DbHandler {
        private class SQLiteContext : IDisposable, IAsyncDisposable {
            private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>
                {
                    new ApiEnumConverter(),
                    new ApiFlagsConverter(),
                    new Coordinates2Converter(),
                    new Coordinates3Converter(),
                    new NullableRenderUrlConverter(),
                    new RectangleConverter(),
                    new RenderUrlConverter(),
                },
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver() {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            };

            public SQLiteAsyncConnection Connection { get; }

            public const string ID_COLUMN = "id";
            public const string DATA_COLUMN = "data";

            private static readonly ConcurrentDictionary<SQLiteConnection, int> _references =
                new ConcurrentDictionary<SQLiteConnection, int>();
            private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

            public SQLiteContext(string dbPath, bool readOnly) {
                _semaphore.Wait();
                try {
                    this.Connection = new SQLiteAsyncConnection(dbPath,
                        readOnly
                            ? SQLiteOpenFlags.ReadOnly
                            : SQLiteOpenFlags.ReadWrite);

                    _references.AddOrUpdate(this.Connection.GetConnection(), 1, (_, x) => x + 1);
                } finally {
                    _semaphore.Release();
                }
            }

            public static void Create(string dbPath, IEnumerable<ILoadCollection> collections) {
                using var connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

                foreach (var collection in collections) {
                    // TODO: Use BLOB and byte[] for Guid?
                    bool isText = Type.GetTypeCode(collection.IdType) switch {
                        TypeCode.Int32 => false,
                        TypeCode.String => true,
                        _ => throw new Exception($"Unsupported key type: {collection.IdType}"),
                    };

                    // Use INTEGER for int and TEXT for string
                    // Use WITHOUT ROWID for string id tables (see <https://www.sqlite.org/lang_createtable.html#rowid>)
                    connection.Execute(
                        $"CREATE TABLE IF NOT EXISTS `{collection.TableName}`\n (" +
                        $"{ID_COLUMN} {(isText ? "TEXT" : "INTEGER")} PRIMARY KEY NOT NULL,\n" +
                        $"{DATA_COLUMN} BLOB NOT NULL\n" +
                        $") {(isText ? "WITHOUT ROWID" : "")}");
                }
            }

            public static string Serialize<TItem>(TItem item)
                => JsonConvert.SerializeObject(item, typeof(TItem), _jsonSerializerSettings);

            public static T Deserialize<T>(string data)
                => JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings) ?? throw new Exception();

            public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

            public async ValueTask DisposeAsync() {
                await _semaphore.WaitAsync();
                try {
                    var key = Connection.GetConnection();
                    if (_references.AddOrUpdate(key, 0, (_, x) => x - 1) <= 0) {
                        _references.TryRemove(key, out int _);
                        await Connection.CloseAsync();
                    }
                } finally {
                    _semaphore.Release();
                }
            }
        }
    }
}
