using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blish_HUD.Settings {

    internal sealed class SettingCollection : ISettingCollection {

        private const string JSON_PROPERTY_NAME_ENTRY_KEY = "Key";
        private const string JSON_PROPERTY_NAME_RENDER_IN_UI = "Ui";
        private const string JSON_PROPERTY_NAME_ENTRIES = "Entries";

        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions {
            AllowTrailingCommas = true,
            Converters = {
                new JsonStringEnumConverter(),
                new SettingCollectionConverter(),
                new RuntimeTypeJsonConverter<ISettingEntry>()
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
        
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingCollection() {
            RawEntries = new List<JsonElement>();
        }


        [JsonPropertyName(JSON_PROPERTY_NAME_RENDER_IN_UI)]
        public bool RenderInUi { get; private set; }

        [JsonInclude]
        [JsonPropertyName(JSON_PROPERTY_NAME_ENTRIES)]
        public List<JsonElement> RawEntries { get; private set; }

        private readonly Dictionary<string, ISettingEntry> _cachedEntries = new Dictionary<string, ISettingEntry>(StringComparer.InvariantCultureIgnoreCase);


        public ISettingCollection AddSubCollection(string collectionKey, bool renderInUi = false) {
            var newCollection = new SettingCollection { RenderInUi = renderInUi };
            return DefineSetting(collectionKey, (ISettingCollection)newCollection).Value;
        }

        public bool TryGetSubCollection(string collectionKey, out ISettingCollection collection) {
            var result = TryGetSetting<SettingCollection>(collectionKey, out var collectionEntry);
            collection = collectionEntry?.Value;
            return result;
        }

        public ISettingCollection GetSubCollection(string collectionKey) =>
            TryGetSubCollection(collectionKey, out var subCollection) ? subCollection : throw new KeyNotFoundException($"Sub collection {collectionKey} was not found");


        public ISettingEntry<T> DefineSetting<T>(string entryKey, T defaultValue) {
            var settingEntry = DefineGenericSetting<SettingEntry<T>, T>(entryKey, defaultValue);

            if (!_cachedEntries.ContainsKey(entryKey)) {
                _cachedEntries[entryKey] = settingEntry;
            }

            return settingEntry;
        }

        public IUiSettingEntry<T> DefineUiSetting<T>(string entryKey, T defaultValue, Func<string> displayNameFunc, Func<string> descriptionFunc) {
            var settingEntry = DefineGenericSetting<UiSettingEntry<T>, T>(entryKey, defaultValue);
            settingEntry.GetDisplayNameFunc = displayNameFunc;
            settingEntry.GetDescriptionFunc = descriptionFunc;

            if (!_cachedEntries.ContainsKey(entryKey)) {
                _cachedEntries[entryKey] = settingEntry;
            }

            return settingEntry;
        }

        private TEntry DefineGenericSetting<TEntry, TValue>(string entryKey, TValue defaultValue) where TEntry : ISettingEntry<TValue>, new() {
            var settingElement = TryGetSettingElement(entryKey);

            TEntry settingEntry;
            if (!settingElement.HasValue) {
                // Create a new instance
                settingEntry = new TEntry {
                    EntryKey = entryKey,
                    Value = defaultValue
                };

                SettingEntry_SettingUpdated(settingEntry, new EventArgs());
            } else {
                // The setting already exists, so populate it
                settingEntry = new TEntry {
                    EntryKey = entryKey
                };
                LoadSettingIntoObject(entryKey, settingEntry);
            }

            // Hook onto changes
            settingEntry.SettingUpdated += SettingEntry_SettingUpdated;

            return settingEntry;
        }

        [Obsolete("This function does not produce a localization friendly SettingEntry.")]
        public IUiSettingEntry<T> DefineSetting<T>(string entryKey, T defaultValue, string displayName, string description, SettingsService.SettingTypeRendererDelegate renderer) =>
            DefineUiSetting(entryKey, defaultValue, () => displayName, () => description);

        public void UndefineSetting(string entryKey) {
            if (_cachedEntries.TryGetValue(entryKey, out var settingEntry)) {
                settingEntry.SettingUpdated -= SettingEntry_SettingUpdated;
                _cachedEntries.Remove(entryKey);
            }

            RemoveSetting(entryKey);
        }

        public bool ContainsSetting(string entryKey) =>
            TryGetSettingElement(entryKey).HasValue;

        public bool TryGetSetting<T>(string entryKey, out ISettingEntry<T> settingEntry) {
            var loadedSettingEntry = new SettingEntry<T>();
            if (LoadSettingIntoObject(entryKey, loadedSettingEntry)) {
                settingEntry = loadedSettingEntry;
                return true;
            }

            settingEntry = null;
            return false;
        }

        public ISettingEntry<T> GetSetting<T>(string entryKey) =>
            TryGetSetting<T>(entryKey, out var settingEntry) ? settingEntry : throw new KeyNotFoundException($"Setting {entryKey} was not found");

        public IEnumerable<ISettingEntry> GetDefinedSettings(bool uiOnly = false) {
            var genericUiType = typeof(IUiSettingEntry<>);

            foreach (var settingEntry in _cachedEntries.Values) {
                if (uiOnly) {
                    if (settingEntry is ISettingEntry<ISettingCollection> collection && collection.Value.RenderInUi) {
                        yield return settingEntry;
                    }

                    if (genericUiType.IsAssignableFrom(settingEntry.GetType().GetGenericTypeDefinition())) {
                        yield return settingEntry;
                    }
                } else {
                    yield return settingEntry;
                }
            }
        }


        private bool LoadSettingIntoObject<T>(string entryKey, ISettingEntry<T> settingEntry) {
            if (!ContainsSetting(entryKey)) {
                return false;
            }

            settingEntry.Value = LoadSetting<T>(entryKey).Value;
            return true;
        }


        private JsonElement? TryGetSettingElement(string entryKey) {
            foreach (var element in RawEntries) {
                // We check whether this element is the setting we're looking for

                if (element.ValueKind != JsonValueKind.Object) {
                    // Not a proper type
                    continue;
                }

                if (!element.TryGetProperty(JSON_PROPERTY_NAME_ENTRY_KEY, out var keyElement)) {
                    // Does not have the key defined
                    continue;
                }

                if (keyElement.ValueKind != JsonValueKind.String) {
                    // The key is not a string
                    continue;
                }

                if (!keyElement.GetString().Equals(entryKey, StringComparison.OrdinalIgnoreCase)) {
                    // The key is not the key we're looking for
                    continue;
                }

                // Found the setting, stop
                return element;
            }

            return null;
        }

        private ISettingEntry<T> LoadSetting<T>(string entryKey) {
            var settingElement = TryGetSettingElement(entryKey);
            if (!settingElement.HasValue) {
                return default;
            }
            
            // System.Text.Json 6.0 adds support for JsonSerializer.Deserialize with a JsonElement.
            // We can use that once it's in preview 7, or out of preview.
            var bufferWriter = new ArrayBufferWriter<byte>();
            using var buffer = new Utf8JsonWriter(bufferWriter);
            settingElement.Value.WriteTo(buffer);
            buffer.Flush();

            return JsonSerializer.Deserialize<SettingEntry<T>>(bufferWriter.WrittenSpan, _jsonSerializerOptions);
        }

        private void StoreSetting(ISettingEntry settingEntry) {
            RemoveSetting(settingEntry.EntryKey);

            // System.Text.Json 6.0 adds support for JsonSerializer.Serialize to a JsonElement.
            // We can use that once it's in preview 7, or out of preview.
            using var document = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(settingEntry, settingEntry.GetType(), _jsonSerializerOptions));
            var setting = document.RootElement.Clone();
            RawEntries.Add(setting);
        }

        private void RemoveSetting(string entryKey) {
            var elementToDelete = TryGetSettingElement(entryKey);
            if (elementToDelete.HasValue) {
                RawEntries.Remove(elementToDelete.Value);
            }
        }


        private void SettingEntry_SettingUpdated(object sender, EventArgs e) {
            if (!(sender is ISettingEntry settingEntry)) {
                return;
            }

            StoreSetting(settingEntry);
            OnPropertyChanged(settingEntry.EntryKey);
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
