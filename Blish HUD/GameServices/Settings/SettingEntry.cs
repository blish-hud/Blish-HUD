using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.Settings {

    public abstract class SettingEntry : INotifyPropertyChanged {

        protected const string SETTINGTYPE_KEY  = "T";
        protected const string SETTINGNAME_KEY  = "Key";
        protected const string SETTINGVALUE_KEY = "Value";

        public class SettingEntryConverter : JsonConverter<SettingEntry> {

            private static readonly Logger Logger = Logger.GetLogger<SettingEntryConverter>();

            public override void WriteJson(JsonWriter writer, SettingEntry value, JsonSerializer serializer) {
                var entryObject = new JObject();

                var entryType = value.GetSettingType();

                entryObject.Add(SETTINGTYPE_KEY,  $"{entryType.FullName}, {entryType.Assembly.GetName().Name}");
                entryObject.Add(SETTINGNAME_KEY,  value.EntryKey);
                entryObject.Add(SETTINGVALUE_KEY, JToken.FromObject(value.GetSettingValue(), serializer));

                entryObject.WriteTo(writer);
            }

            public override SettingEntry ReadJson(JsonReader reader, Type objectType, SettingEntry existingValue, bool hasExistingValue, JsonSerializer serializer) {
                var jObj = JObject.Load(reader);

                string entryTypeString = jObj[SETTINGTYPE_KEY].Value<string>();
                var    entryType       = Type.GetType(entryTypeString);

                if (entryType == null) {
                    Logger.Warn("Failed to load setting of missing type '{settingDefinedType}'.", entryTypeString);

                    return null;
                }

                var entryGeneric = Activator.CreateInstance(typeof(SettingEntry<>).MakeGenericType(entryType));

                serializer.Populate(jObj.CreateReader(), entryGeneric);

                return entryGeneric as SettingEntry;
            }

        }

        [JsonIgnore]
        public string Description { get; set; }

        [JsonIgnore]
        public string DisplayName { get; set; }

        [JsonIgnore, Obsolete]
        public SettingsService.SettingTypeRendererDelegate Renderer { get; set; }

        /// <summary>
        /// The unique key used to identify the <see cref="SettingEntry"/> in the <see cref="SettingCollection"/>.
        /// </summary>
        [JsonProperty(SETTINGNAME_KEY)]
        public string EntryKey { get; protected set; }

        protected abstract Type GetSettingType();

        protected abstract object GetSettingValue();

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
