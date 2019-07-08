using System.ComponentModel;
using Newtonsoft.Json;

namespace Blish_HUD.PersistentStore {
    [JsonObject]
    public class StoreValue<T> : StoreValue, INotifyPropertyChanged {

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
}
