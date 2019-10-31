using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Blish_HUD.PersistentStore;

namespace Blish_HUD {

    [JsonObject]
    public class PersistentStoreService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<PersistentStoreService>();

        private Store _stores;

        [JsonIgnore]
        private const string STORE_FILENAME = "persistent.json";
        private const int SAVE_FREQUENCY = 5000; // Time in milliseconds (currently 5 seconds)

        [JsonIgnore]
        public static bool StoreChanged = false;

        [JsonIgnore]
        private JsonSerializerSettings _jsonSettings;

        [JsonIgnore]
        private string _persistentStorePath;

        protected override void Initialize() {
            _jsonSettings = new JsonSerializerSettings() {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling           = TypeNameHandling.None,
                Converters = new List<JsonConverter>() {
                    new StoreValue.StoreValueConverter(),
                    new Store.PersistentStoreConverter()
                }
            };

            _persistentStorePath = Path.Combine(DirectoryUtil.BasePath, STORE_FILENAME);

            // If store isn't there, generate the file
            if (!File.Exists(_persistentStorePath)) Save();

            // Would prefer to have this under Load(), but PersistentStoreService needs to be ready for other modules and services
            try {
                string rawStore = File.ReadAllText(_persistentStorePath);

                _stores = JsonConvert.DeserializeObject<PersistentStore.Store>(rawStore, _jsonSettings);
            } catch (System.IO.FileNotFoundException) {
                // Likely don't have access to this filesystem
            } catch (Exception e) {
                Logger.Warn(e, "There was an unexpected error trying to read the persistent store data file.");
            }

            if (_stores == null) {
                Logger.Warn("Persistent store could not be read, so a new one will be generated.");
                _stores = new PersistentStore.Store();
            }
        }

        public PersistentStore.Store RegisterStore(string storeName) {
            return _stores.GetSubstore(storeName);
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() { /* NOOP */ }

        private double _lastUpdate = 0;
        protected override void Update(GameTime gameTime) {
            _lastUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_lastUpdate > SAVE_FREQUENCY) {
                if (StoreChanged)
                    Save();
                _lastUpdate = 0;
            }
        }

        public void Save() {
            try {
                string rawStore = JsonConvert.SerializeObject(_stores, Formatting.None, _jsonSettings);

                using (var settingsWriter = new StreamWriter(_persistentStorePath, false)) {
                    settingsWriter.Write(rawStore);
                }
            } catch (InvalidOperationException e) {
                // Likely that the collection was modified while we were attempting to serialize the stores
                Logger.Warn(e, "Failed to save persistent store.");
            } catch (Exception e) {
                Logger.Warn(e, "Failed to write persistent store to file!");
            }

            _lastUpdate = 0;
            StoreChanged = false;
        }

    }
}
