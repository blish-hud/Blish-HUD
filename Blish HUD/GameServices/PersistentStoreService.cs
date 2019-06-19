using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Blish_HUD.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD {

    public abstract class StoreValue {

        public class StoreValueConverter : JsonConverter<StoreValue> {

            public override void WriteJson(JsonWriter writer, StoreValue value, JsonSerializer serializer) {
                JToken.FromObject(value._value, serializer).WriteTo(writer);
            }

            public override StoreValue ReadJson(JsonReader reader, Type objectType, StoreValue existingValue, bool hasExistingValue, JsonSerializer serializer) {
                var jObj = JToken.Load(reader) as JValue;

                Type storeType = typeof(int); 
                switch (jObj.Type) {
                    case JTokenType.Integer:
                        storeType = typeof(int);
                        break;
                    case JTokenType.Float:
                        storeType = typeof(float);
                        break;
                    case JTokenType.String:
                        storeType = typeof(string);
                        break;
                    case JTokenType.Boolean:
                        storeType = typeof(bool);
                        break;
                    case JTokenType.Null:
                        storeType = typeof(string);
                        break;
                    case JTokenType.Date:
                        storeType = typeof(DateTime);
                        break;
                    case JTokenType.Guid:
                        storeType = typeof(Guid);
                        break;
                    case JTokenType.TimeSpan:
                        storeType = typeof(TimeSpan);
                        break;

                    default:
                        GameService.Debug.WriteWarningLine($"Persistent store value of type '{jObj.Type}' is not supported.");
                        break;
                }

                var entryGeneric = Activator.CreateInstance(typeof(StoreValue<>).MakeGenericType(storeType));
                var storeBase = entryGeneric as StoreValue;
                storeBase._value = jObj.Value;

                return storeBase;
            }

        }

        protected object _value;

        private object _defaultValue;

        [JsonIgnore]
        public bool IsDefaultValue => object.Equals(_value, _defaultValue) && _defaultValue != null;

        public StoreValue UpdateDefault(object defaultValue) {
            _defaultValue = defaultValue;

            return this;
        }

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            PersistentStoreService.StoreChanged = true;
        }

        #endregion

    }

    [JsonObject]
    public class StoreValue<T>:StoreValue, INotifyPropertyChanged {

        [JsonIgnore]
        public T Value {
            get => (T)_value;
            set {
                if (object.Equals(_value, value)) return;

                _value = value;

                OnPropertyChanged();
            }
        }

        public StoreValue() { /* NOOP */ }

        public StoreValue(T defaultValue) {
            _value = defaultValue;
        }

    }

    public class PersistentStore {

        public class PersistentStoreConverter : JsonConverter<PersistentStore> {

            public override void WriteJson(JsonWriter writer, PersistentStore value, JsonSerializer serializer) {
                JObject entryObject = new JObject();

                if (value._substores.Any()) {
                    var storesObject = new JObject();

                    foreach (var store in value._substores) {
                        storesObject.Add(store.Key, JToken.FromObject(store.Value, serializer));
                    }

                    entryObject.Add("Stores", storesObject);
                }

                var nonDefaultValues = value._values.Where(pair => !pair.Value.IsDefaultValue);
                if (nonDefaultValues.Any()) {
                    var valuesObject = new JObject();

                    foreach (var ndValue in nonDefaultValues) {
                        valuesObject.Add(ndValue.Key, JToken.FromObject(ndValue.Value, serializer));
                    }

                    entryObject.Add("Values", valuesObject);
                }

                entryObject.WriteTo(writer);
            }

            public override PersistentStore ReadJson(JsonReader reader, Type objectType, PersistentStore existingValue, bool hasExistingValue, JsonSerializer serializer) {
                JObject jObj = JObject.Load(reader);

                var loadedStore = new PersistentStore();

                serializer.Populate(jObj.CreateReader(), loadedStore);

                return loadedStore;
            }

        }

        [JsonProperty("Stores")]
        private Dictionary<string, PersistentStore> _substores = new Dictionary<string, PersistentStore>(StringComparer.OrdinalIgnoreCase);
        
        [JsonProperty("Values")]
        private Dictionary<string, StoreValue> _values = new Dictionary<string, StoreValue>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, StoreValue> _recordedValues {
            get => _values.Where(pair => !pair.Value.IsDefaultValue).ToDictionary(dict => dict.Key, dict => dict.Value);
            set => _values = value;
        }

        public PersistentStore GetSubstore(string substoreName) {
            if (!_substores.ContainsKey(substoreName)) {
                _substores.Add(substoreName, new PersistentStore());
                PersistentStoreService.StoreChanged = true;
            }

            return _substores[substoreName];
        }

        public StoreValue<T> GetOrSetValue<T>(string valueName, T defaultValue = default) {
            if (!_values.ContainsKey(valueName)) {
                _values.Add(valueName, new StoreValue<T>(defaultValue));
                PersistentStoreService.StoreChanged = true;
            }

            return _values[valueName].UpdateDefault(defaultValue) as StoreValue<T>;
        }

        public void RemoveValueByName(string valueName) {
            if (_values.ContainsKey(valueName)) {
                _values.Remove(valueName);
                PersistentStoreService.StoreChanged = true;
            }
        }

    }
    
    [JsonObject]
    public class PersistentStoreService : GameService {

        private PersistentStore _stores;

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
                    new PersistentStore.PersistentStoreConverter()
                }
            };

            _persistentStorePath = Path.Combine(GameService.Directory.BasePath, STORE_FILENAME);

            // If store isn't there, generate the file
            if (!File.Exists(_persistentStorePath)) Save();

            // Would prefer to have this under Load(), but PersistentStoreService needs to be ready for other modules and services
            try {
                string rawStore = File.ReadAllText(_persistentStorePath);

                _stores = JsonConvert.DeserializeObject<PersistentStore>(rawStore, _jsonSettings);
            } catch (System.IO.FileNotFoundException) {
                // Likely don't have access to this filesystem
            } catch (Exception e) {
                // TODO: If this fails, we may need to prompt the user to re-generate the settings (in case they were corrupted or something)
                Console.WriteLine(e.Message);
            }

            if (_stores == null) {
                Console.WriteLine("Persistent store was lost.");
                _stores = new PersistentStore();
            }
        }

        public PersistentStore RegisterStore(string storeName) {
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
            string rawStore = JsonConvert.SerializeObject(_stores, Formatting.None, _jsonSettings);

            try {
                using (var settingsWriter = new StreamWriter(_persistentStorePath, false)) {
                    settingsWriter.Write(rawStore);
                }
            } catch (Exception e) {
                Console.WriteLine("Failed to write persistent store to file!");
                // TODO: We need to try saving the file again later - something is preventing us from saving
            }

            _lastUpdate = 0;
            StoreChanged = false;
        }

    }
}
