using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blish_HUD.Content.Serialization;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD {

    [JsonObject]
    public class SettingsService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<SettingsService>();

        private const int SAVE_INTERVAL = 4;

        private const string SETTINGS_FILENAME  = "settings.json";
        private const string SETTINGS_PMBACKUP  = "settings-premigration.json";
        private const string SETTINGS_DIRECTORY = "settings";

        [Obsolete]
        public delegate void SettingTypeRendererDelegate(SettingEntry setting, Panel settingPanel);
        
        [JsonIgnore]
        internal JsonSerializerSettings JsonReaderSettings { get; private set; }
        
        [JsonIgnore]
        private string _settingsPath;

        [JsonIgnore]
        private string _settingsDirectory;

        internal SettingCollection Settings { get; private set; }

        private bool   _dirtySave;
        private double _saveBuffer;

        private readonly Dictionary<string, SettingCollection> _moduleSettings = new Dictionary<string, SettingCollection>();

        protected override void Initialize() {
            JsonReaderSettings = new JsonSerializerSettings() {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling           = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>() {
                    new SettingCollection.SettingCollectionConverter(),
                    new SettingEntry.SettingEntryConverter(),

                    // Types that need help:
                    new SemVerConverter()
                }
            };

            _settingsPath      = Path.Combine(DirectoryUtil.BasePath, SETTINGS_FILENAME);
            _settingsDirectory = DirectoryUtil.RegisterDirectory(SETTINGS_DIRECTORY);

            // If settings aren't there, generate the file
            if (!File.Exists(_settingsPath)) PrepareSettingsFirstTime();

            LoadSettings();
        }

        private void MigrateModuleSettings(string rawJson) {
            // Migrates module settings from the settings.json into individual per-module files under the settings/ directory.
            try {
                var root = JObject.Parse(rawJson);

                // Root is a SettingCollection
                var rootEntries = root["Entries"] as JArray;
                if (rootEntries == null) return;

                // Find the "ModuleConfiguration" entry
                var moduleConfigValue = FindSettingEntryValue(rootEntries, "ModuleConfiguration") as JObject;
                if (moduleConfigValue == null) return;

                // Inside ModuleConfiguration, find "ModuleStates" entry
                var configEntries = moduleConfigValue["Entries"] as JArray;
                if (configEntries == null) return;

                var moduleStatesValue = FindSettingEntryValue(configEntries, "ModuleStates");
                if (moduleStatesValue == null || moduleStatesValue.Type != JTokenType.Object) return;

                // Check if any modules actually have settings to migrate
                bool hasSettingsToMigrate = moduleStatesValue.Children<JProperty>().Any(p => p.Value is JObject state && state["Settings"] != null && state["Settings"].Type != JTokenType.Null);

                if (!hasSettingsToMigrate) return;

                // Back up the original settings file before we touch anything
                // Only create the backup if one doesn't already exist (a previous
                // partial migration may have left one behind)
                string backupPath = Path.Combine(DirectoryUtil.BasePath, SETTINGS_PMBACKUP);

                if (!File.Exists(backupPath)) {
                    try {
                        File.Copy(_settingsPath, backupPath);
                        Logger.Info($"Created backup of settings file before migration at '{backupPath}'.");
                    } catch (Exception ex) {
                        Logger.Warn(ex, $"Failed to create backup of settings file at '{backupPath}' before migration. Aborting migration to avoid potential data loss.");
                        return;
                    }
                }

                bool migrated = false;

                foreach (var moduleProp in moduleStatesValue.Children<JProperty>().ToList()) {
                    var moduleState = moduleProp.Value as JObject;
                    if (moduleState == null) continue;

                    JToken settingsToken = moduleState["Settings"];
                    if (settingsToken == null || settingsToken.Type == JTokenType.Null) continue;

                    // Write settings to per-module file
                    string moduleSettingsPath = GetModuleSettingsPath(moduleProp.Name);

                    try {
                        string settingsJson = settingsToken.ToString(Formatting.None);
                        WriteFileAtomic(moduleSettingsPath, settingsJson);

                        Logger.Info($"Migrated settings for module '{moduleProp.Name}' to '{moduleSettingsPath}'.");
                    } catch (Exception ex) {
                        Logger.Warn(ex, $"Failed to migrate settings for module '{moduleProp.Name}'.");
                        continue;
                    }

                    // Remove settings from the module state in the main file.
                    moduleState.Remove("Settings");
                    migrated = true;
                }

                if (migrated) {
                    WriteFileAtomic(_settingsPath, root.ToString(Formatting.None));
                    Logger.Info("Completed migration of module settings to individual files.");
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "An error occurred while attempting to migrate module settings.");
            }
        }

        private static JToken FindSettingEntryValue(JArray entries, string key) {
            foreach (var entry in entries) {
                if (string.Equals(entry["Key"]?.ToString(), key, StringComparison.OrdinalIgnoreCase)) {
                    return entry["Value"];
                }
            }

            return null;
        }

        private void LoadSettings(bool alreadyFailed = false) {
            string rawSettings = null;

            try {
                rawSettings = File.ReadAllText(_settingsPath);

                // Migrate module settings out of settings.json into per-module files before loading
                MigrateModuleSettings(rawSettings);

                this.Settings = JsonConvert.DeserializeObject<SettingCollection>(rawSettings, JsonReaderSettings) ?? new SettingCollection(false);
            } catch (UnauthorizedAccessException) {
                Blish_HUD.Debug.Contingency.NotifyFileSaveAccessDenied(_settingsPath, Strings.GameServices.Debug.ContingencyMessages.FileSaveAccessDenied_Action_ToLoadSettings);
            } catch (Exception ex) {
                if (alreadyFailed) {
                    Logger.Warn(ex, "Failed to load settings due to an unexpected exception while attempting to read them. Already tried creating a new settings file, so we won't try again.");
                } else {
                    Logger.Warn(ex, "Failed to load settings due to an unexpected exception while attempting to read them. A new settings file will be generated.");

                    if (!string.IsNullOrEmpty(rawSettings)) {
                        Logger.Info(rawSettings);
                    } else {
                        Logger.Warn("Settings were empty or could not be read.");
                    }

                    // Refresh the settings
                    PrepareSettingsFirstTime();

                    // Try to reload the settings
                    LoadSettings(true);
                }
            }
        }

        private void PrepareSettingsFirstTime() {
            Logger.Info("Preparing default settings file.");
            this.Settings = new SettingCollection();
            Save(true);
        }

        public void Save(bool forceSave = false) {
            if (!Loaded && !forceSave) return;

            if (forceSave) {
                PerformSave();
            } else {
                _dirtySave = true;
            }
        }

        private void PerformSave() {
            string rawSettings = JsonConvert.SerializeObject(this.Settings, Formatting.None, JsonReaderSettings);

            try {
                // Save settings.json
                WriteFileAtomic(_settingsPath, rawSettings);

                // Save all registered module settings to their individual files
                foreach (var mkp in _moduleSettings) {
                    PerformModuleSettingsSave(mkp.Key, mkp.Value);
                }
            } catch (UnauthorizedAccessException) {
                Blish_HUD.Debug.Contingency.NotifyFileSaveAccessDenied(_settingsPath, Strings.GameServices.Debug.ContingencyMessages.FileSaveAccessDenied_Action_ToSaveSettings);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to save settings.");
                return;
            }

            _saveBuffer = 0;
            _dirtySave  = false;

            Logger.Debug("Settings were saved successfully.");
        }

        private static void WriteFileAtomic(string filePath, string content) {
            string tempPath = $"{filePath}.new";

            using (var writer = new StreamWriter(tempPath, false)) {
                writer.Write(content);
            }

            if (File.Exists(filePath)) {
                File.Replace(tempPath, filePath, null);
            } else {
                File.Move(tempPath, filePath);
            }
        }

        #region Module Settings

        private void PerformModuleSettingsSave(string moduleNamespace, SettingCollection settings) {
            string moduleSettingsPath = GetModuleSettingsPath(moduleNamespace);

            try {
                string rawModuleSettings = JsonConvert.SerializeObject(settings, Formatting.None, JsonReaderSettings);
                WriteFileAtomic(moduleSettingsPath, rawModuleSettings);
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed to save settings for module '{moduleNamespace}'.");
            }
        }

        private string GetModuleSettingsPath(string moduleNamespace) {
            return Path.Combine(_settingsDirectory, $"{moduleNamespace}.json");
        }

        internal SettingCollection LoadModuleSettings(string moduleNamespace) {
            string path = GetModuleSettingsPath(moduleNamespace);

            if (!File.Exists(path)) return null;

            try {
                string rawSettings = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<SettingCollection>(rawSettings, JsonReaderSettings);
            } catch (Exception ex) {
                Logger.Warn(ex, path, $"Failed to load settings for module '{moduleNamespace}'.");
                return null;
            }
        }

        internal void RegisterModuleSettings(string moduleNamespace, SettingCollection settings) {
            _moduleSettings[moduleNamespace] = settings;
        }

        internal void UnregisterModuleSettings(string moduleNamespace) {
            if (_moduleSettings.TryGetValue(moduleNamespace, out var settings)) {
                PerformModuleSettingsSave(moduleNamespace, settings);
                _moduleSettings.Remove(moduleNamespace);
            }
        }

        #endregion

        protected override void Load() { /* NOOP */ }

        internal SettingCollection RegisterRootSettingCollection(string collectionKey) {
            return this.Settings.AddSubCollection(collectionKey, false);
        }

        protected override void Unload() {
            Save(true);
        }

        protected override void Update(GameTime gameTime) {
            if (_dirtySave) {
                _saveBuffer += gameTime.ElapsedGameTime.TotalSeconds;

                if (_saveBuffer > SAVE_INTERVAL) {
                    PerformSave();
                }
            }
        }
        
    }
}
