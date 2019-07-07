using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.Settings {

    public abstract class SettingEntry : INotifyPropertyChanged {

        public class SettingEntryConverter : JsonConverter<SettingEntry> {

            public override void WriteJson(JsonWriter writer, SettingEntry value, JsonSerializer serializer) {
                JObject entryObject = new JObject();

                Type entryType = value.GetSettingType();

                entryObject.Add("T", $"{entryType.FullName}, {entryType.Assembly.GetName().Name}");
                entryObject.Add("Key", value.EntryKey);
                entryObject.Add("Value", JToken.FromObject(value.GetSettingValue(), serializer));

                entryObject.WriteTo(writer);
            }

            public override SettingEntry ReadJson(JsonReader reader, Type objectType, SettingEntry existingValue, bool hasExistingValue, JsonSerializer serializer) {
                JObject jObj = JObject.Load(reader);

                var entryTypeString = jObj["T"].Value<string>();
                var entryType = Type.GetType(entryTypeString);

                var entryGeneric = Activator.CreateInstance(typeof(SettingEntry<>).MakeGenericType(entryType));

                serializer.Populate(jObj.CreateReader(), entryGeneric);

                return entryGeneric as SettingEntry;
            }

        }

        [JsonIgnore]
        public string Description { get; set; }

        [JsonIgnore]
        public string DisplayName { get; set; }

        [JsonIgnore]
        public SettingsService.SettingTypeRendererDelegate Renderer { get; set; }

        [JsonProperty("Key")]
        /// <summary>
        /// The unique key used to identify the <see cref="SettingEntry"/> in the <see cref="SettingCollection"/>.
        /// </summary>
        public string EntryKey { get; protected set; }

        protected abstract Type GetSettingType();

        protected abstract Object GetSettingValue();

        [JsonIgnore]
        public bool IsNull => this.GetSettingValue() == null;

        [JsonIgnore]
        public Type SettingType => GetSettingType();

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
