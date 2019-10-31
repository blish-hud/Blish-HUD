using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.PersistentStore {
    public abstract class StoreValue {

        public class StoreValueConverter : JsonConverter<StoreValue> {

            private static readonly Logger Logger = Logger.GetLogger<StoreValueConverter>();

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
                        Logger.Warn("Persistent store value of type {storeValueType} is not supported by the PersistentStoreService.", jObj.Type);
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            PersistentStoreService.StoreChanged = true;
        }

        #endregion

    }
}
