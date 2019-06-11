using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD {


    [JsonObject]
    public abstract class StoreValue {

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

        [JsonProperty]
        private T _value;

        [JsonIgnore]
        public T Value {
            get => _value;
            set {
                if (object.Equals(_value, value)) return;

                _value = value;

                OnPropertyChanged();
            }
        }

        public StoreValue(T defaultValue) {
            _value = defaultValue;
        }

    }

    [JsonObject]
    public class PersistentStore {
        
        [JsonProperty]
        private Dictionary<string, PersistentStore> _substores = new Dictionary<string, PersistentStore>();
        
        [JsonProperty]
        private Dictionary<string, StoreValue> _values = new Dictionary<string, StoreValue>();

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

            return (StoreValue<T>)_values[valueName];
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

        [JsonProperty]
        public PersistentStore Stores { get; private set; }

        [JsonIgnore]
        private const string STORE_FILENAME = "persistent.json";
        private const int SAVE_FREQUENCY = 10000; // Time in milliseconds (currently 10 seconds)

        [JsonIgnore]
        public static bool StoreChanged = false;

        [JsonIgnore]
        private JsonSerializerSettings _jsonSettings;

        [JsonIgnore]
        private string _settingsPath;

        protected override void Initialize() {
            this.Stores = new PersistentStore();

            _jsonSettings = new JsonSerializerSettings() {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling           = TypeNameHandling.None,
            };

            _settingsPath = Path.Combine(GameService.Directory.BasePath, STORE_FILENAME);

            // If store isn't there, generate the file
            if (!File.Exists(_settingsPath)) Save();

            // Would prefer to have this under Load(), but PersistentStoreService needs to be ready for other modules and services
            try {
                string rawSettings = File.ReadAllText(_settingsPath);

                JsonConvert.PopulateObject(rawSettings, this, _jsonSettings);
            } catch (System.IO.FileNotFoundException) {
                // Likely don't have access to this filesystem
            } catch (Exception e) {
                // TODO: If this fails, we may need to prompt the user to re-generate the settings (in case they were corrupted or something)
                Console.WriteLine(e.Message);
            }
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
            string rawSettings = JsonConvert.SerializeObject(this, Formatting.None, _jsonSettings);

            try {
                using (var settingsWriter = new StreamWriter(_settingsPath, false)) {
                    settingsWriter.Write(rawSettings);
                }
            } catch (Exception e) {
                Console.WriteLine("Failed to write settings to file!");
                // TODO: We need to try saving the file again later - something is preventing us from saving
            }

            _lastUpdate = 0;
            StoreChanged = false;
        }

    }
}
