using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

                    foreach (var entryObject in value.Entries.Where(e => !e.IsNull).Select(entry => JObject.FromObject(entry, serializer))) {
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

        private readonly ReaderWriterLockSlim _entryLock = new ReaderWriterLockSlim();

        private readonly List<SettingEntry> _definedEntries = new List<SettingEntry>();
        private          List<SettingEntry> _undefinedEntries;

        public bool LazyLoaded { get; }

        public IReadOnlyList<SettingEntry> Entries {
            get {
                if (!this.Loaded) Load();

                _entryLock.EnterReadLock();
                var combinedEntries = _definedEntries.Concat(_undefinedEntries).ToList().AsReadOnly();
                _entryLock.ExitReadLock();

                return combinedEntries;
            }
        }

        public bool Loaded => _undefinedEntries != null;

        public bool RenderInUi { get; set; }

        public SettingCollection(bool lazy = false) {
            this.LazyLoaded  = lazy;
            _entryTokens = null;

            _undefinedEntries = new List<SettingEntry>();
        }

        public SettingCollection(bool lazy, JToken entryTokens) {
            this.LazyLoaded  = lazy;
            _entryTokens = entryTokens;

            if (!this.LazyLoaded) {
                Load();
            }
        }

        public SettingEntry<TEntry> DefineSetting<TEntry>(string entryKey, TEntry defaultValue, Func<string> displayNameFunc = null, Func<string> descriptionFunc = null) {
            // We don't need to check if we've loaded because the first check uses this[key] which
            // will load if we haven't already since it references this.Entries instead of _entries
            if (!(this[entryKey] is SettingEntry<TEntry> definedEntry)) {
                definedEntry = SettingEntry<TEntry>.InitSetting(entryKey, defaultValue);
            }

            definedEntry.GetDisplayNameFunc = displayNameFunc ?? (() => null);
            definedEntry.GetDescriptionFunc = descriptionFunc ?? (() => null);
            definedEntry.SessionDefined     = true;

            _entryLock.EnterWriteLock();
            _undefinedEntries.Remove(definedEntry);
            _definedEntries.Remove(definedEntry);
            _definedEntries.Add(definedEntry);
            _entryLock.ExitWriteLock();

            return definedEntry;
        }

        [Obsolete("This function does not produce a localization friendly SettingEntry.")]
        public SettingEntry<TEntry> DefineSetting<TEntry>(string entryKey, TEntry defaultValue, string displayName, string description, SettingsService.SettingTypeRendererDelegate renderer = null) {
            return DefineSetting(entryKey, defaultValue, () => displayName, () => description);
        }

        public void UndefineSetting(string entryKey) {
            var entryToRemove = this[entryKey];

            if (entryToRemove != null) {
                _entryLock.EnterWriteLock();
                _undefinedEntries.Remove(entryToRemove);
                _definedEntries.Remove(entryToRemove);
                _entryLock.ExitWriteLock();
            }
        }

        public SettingCollection AddSubCollection(string collectionKey, bool lazyLoaded = false) {
            return AddSubCollection(collectionKey, false, lazyLoaded);
        }

        public SettingCollection AddSubCollection(string collectionKey, bool renderInUi, bool lazyLoaded = false) {
            return DefineSetting(collectionKey, new SettingCollection(lazyLoaded) { RenderInUi = renderInUi }).Value;
        }

        public bool ContainsSetting(string entryKey) {
            return (this.Entries.Any(entry => string.Equals(entry.EntryKey, entryKey, StringComparison.OrdinalIgnoreCase)));
        }

        public bool TryGetSetting(string entryKey, out SettingEntry settingEntry) {
            settingEntry = this[entryKey];

            return settingEntry != null;
        }

        public bool TryGetSetting<T>(string entryKey, out SettingEntry<T> settingEntry) {
            settingEntry = this[entryKey] as SettingEntry<T>;

            return settingEntry != null;
        }

        private void Load() {
            if (_entryTokens == null) return;

            _entryLock.EnterWriteLock();
            _undefinedEntries = JsonConvert.DeserializeObject<List<SettingEntry>>(_entryTokens.ToString(), GameService.Settings.JsonReaderSettings).Where((se) => se != null).ToList();
            _entryLock.ExitWriteLock();

            _entryTokens = null;
        }

        public SettingEntry this[int index] => this.Entries[index];

        public SettingEntry this[string entryKey] => GetSettingByName(this.Entries, entryKey);

        private SettingEntry GetSettingByName(IEnumerable<SettingEntry> entries, string entryKey) {
            _entryLock.EnterReadLock();
            var resultingEntry = entries.FirstOrDefault(se => string.Equals(se.EntryKey, entryKey, StringComparison.OrdinalIgnoreCase));
            _entryLock.ExitReadLock();

            return resultingEntry;
        }

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
