using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.Settings {

    public sealed class SettingCollection : IEnumerable<SettingEntry> {

        public class SettingCollectionConverter : JsonConverter<SettingCollection> {

            private const string ATTR_LAZY       = "Lazy";
            private const string ATTR_RENDERINUI = "Ui";
            private const string ATTR_ENTRIES    = "Entries";

            public override void WriteJson(JsonWriter writer, SettingCollection value, JsonSerializer serializer) {
                var settingCollectionObject = new JObject();

                if (value.LazyLoaded) {
                    settingCollectionObject.Add(ATTR_LAZY, value.LazyLoaded);
                }

                if (value.RenderInUi) {
                    settingCollectionObject.Add(ATTR_RENDERINUI, value.RenderInUi);
                }

                var entryArray = value._entryTokens as JArray;
                if (value.Loaded) {
                    entryArray = new JArray();

                    foreach (var entryObject in value._entries.Where(e => !e.IsNull).Select(entry => JObject.FromObject(entry, serializer))) {
                        entryArray.Add(entryObject);
                    }
                }

                settingCollectionObject.Add(ATTR_ENTRIES, entryArray);

                settingCollectionObject.WriteTo(writer);
            }

            public override SettingCollection ReadJson(JsonReader reader, Type objectType, SettingCollection existingValue, bool hasExistingValue, JsonSerializer serializer) {
                if (reader.TokenType == JsonToken.Null) return null;

                var jObj = JObject.Load(reader);

                bool isLazy     = false;
                bool renderInUi = false;

                if (jObj[ATTR_LAZY] != null) {
                    isLazy = jObj[ATTR_LAZY].Value<bool>();
                }

                if (jObj[ATTR_RENDERINUI] != null) {
                    renderInUi = jObj[ATTR_RENDERINUI].Value<bool>();
                }

                return jObj[ATTR_ENTRIES] != null
                           ? new SettingCollection(isLazy, jObj[ATTR_ENTRIES]) { RenderInUi = renderInUi }
                           : new SettingCollection(isLazy) { RenderInUi = renderInUi };
            }

        }

        private JToken _entryTokens;

        private readonly bool _lazyLoaded;
        private List<SettingEntry> _entries;

        public bool LazyLoaded => _lazyLoaded;

        public IReadOnlyList<SettingEntry> Entries {
            get {
                if (!this.Loaded) Load();

                return _entries.AsReadOnly();
            }
        }

        public bool Loaded => _entries != null;

        public bool RenderInUi { get; set; }

        public SettingCollection(bool lazy = false) {
            _lazyLoaded  = lazy;
            _entryTokens = null;

            _entries = new List<SettingEntry>();
        }

        public SettingCollection(bool lazy, JToken entryTokens) {
            _lazyLoaded  = lazy;
            _entryTokens = entryTokens;

            if (!_lazyLoaded) {
                Load();
            }
        }

        public SettingEntry<TEntry> DefineSetting<TEntry>(string entryKey, TEntry defaultValue, string displayName = null, string description = null, SettingsService.SettingTypeRendererDelegate renderer = null) {
            // We don't need to check if we've loaded because the first check uses this[key] which
            // will load if we haven't already since it references this.Entries instead of _entries
            if (!(this[entryKey] is SettingEntry<TEntry> definedEntry)) {
                definedEntry = SettingEntry<TEntry>.InitSetting(entryKey, defaultValue);
                _entries.Add(definedEntry);
            }

            definedEntry.DisplayName    = displayName;
            definedEntry.Description    = description;
            definedEntry.Renderer       = renderer;
            definedEntry.SessionDefined = true;

            return definedEntry;
        }

        public void UndefineSetting(string entryKey) {
            if (this[entryKey] != null) {
                _entries.Remove(this[entryKey]);
            }
        }

        public SettingCollection AddSubCollection(string collectionKey, bool lazyLoaded = false) {
            return AddSubCollection(collectionKey, false, lazyLoaded);
        }

        public SettingCollection AddSubCollection(string collectionKey, bool renderInUi, bool lazyLoaded = false) {
            return DefineSetting(collectionKey, new SettingCollection(lazyLoaded) { RenderInUi = renderInUi }).Value;
        }

        private void Load() {
            if (_entryTokens == null) return;

            _entries = JsonConvert.DeserializeObject<List<SettingEntry>>(_entryTokens.ToString(), GameService.Settings.JsonReaderSettings).Where((se) => se != null).ToList();

            _entryTokens = null;
        }

        public SettingEntry this[int index] => this.Entries[index];

        public SettingEntry this[string entryKey] => this.Entries.FirstOrDefault(se => string.Equals(se.EntryKey, entryKey, StringComparison.OrdinalIgnoreCase));

        #region IEnumerable

        /// <inheritdoc />
        public IEnumerator<SettingEntry> GetEnumerator() {
            return this.Entries.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }

}
