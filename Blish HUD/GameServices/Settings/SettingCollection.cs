﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.Settings {

    public sealed class SettingCollection : IEnumerable<SettingEntry> {

        public class SettingCollectionConverter : JsonConverter<SettingCollection> {

            public override void WriteJson(JsonWriter writer, SettingCollection value, JsonSerializer serializer) {
                var settingCollectionObject = new JObject();

                if (value.LazyLoaded)
                    settingCollectionObject.Add("Lazy", value.LazyLoaded);

                var entryArray = value._entryTokens as JArray;
                if (value.Loaded) {
                    entryArray = new JArray();

                    foreach (var entry in value._entries.Where(e => !e.IsNull)) {
                        var entryObject = JObject.FromObject(entry, serializer);
                        entryArray.Add(entryObject);
                    }
                }

                settingCollectionObject.Add("Entries", entryArray);

                settingCollectionObject.WriteTo(writer);
            }

            public override SettingCollection ReadJson(JsonReader reader, Type objectType, SettingCollection existingValue, bool hasExistingValue, JsonSerializer serializer) {
                if (reader.TokenType == JsonToken.Null) return null;

                JObject jObj = JObject.Load(reader);

                var isLazy = false;

                if (jObj["Lazy"] != null) {
                    isLazy = jObj["Lazy"].Value<bool>();
                }

                if (jObj["Entries"] != null)
                    return new SettingCollection(isLazy, jObj["Entries"]);

                return new SettingCollection(isLazy);
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
            var existingEntry = this[entryKey];

            var definedValue = defaultValue;

            if (existingEntry is SettingEntry<TEntry> matchingEntry) {
                definedValue = matchingEntry.Value;
            }

            _entries.Remove(existingEntry);

            SettingEntry<TEntry> definedEntry = SettingEntry<TEntry>.InitSetting(entryKey, definedValue);

            _entries.Add(definedEntry);

            definedEntry.DisplayName = displayName;
            definedEntry.Description = description;
            definedEntry.Renderer = renderer;

            return definedEntry;
        }

        public SettingCollection AddSubCollection(string collectionKey, bool lazyLoaded = false) {
            var subCollection = new SettingCollection(lazyLoaded);

            this.DefineSetting(collectionKey, subCollection);

            return subCollection;
        }

        private void Load() {
            if (_entryTokens == null) return;

            _entries = JsonConvert.DeserializeObject<List<SettingEntry>>(_entryTokens.ToString(), GameService.Settings.JsonReaderSettings);

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
