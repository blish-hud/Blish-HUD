using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.Content.Serialization;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD {

    [JsonObject]
    public class SettingsService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<SettingsService>();

        private const int SAVE_INTERVAL = 4;

        private const string SETTINGS_FILENAME = "settings.json";

        [Obsolete]
        public delegate void SettingTypeRendererDelegate(SettingEntry setting, Panel settingPanel);
        
        [JsonIgnore]
        internal JsonSerializerSettings JsonReaderSettings { get; private set; }
        
        [JsonIgnore]
        private string _settingsPath;

        internal SettingCollection Settings { get; private set; }

        private bool   _dirtySave;
        private double _saveBuffer;

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

            _settingsPath = Path.Combine(DirectoryUtil.BasePath, SETTINGS_FILENAME);

            // If settings aren't there, generate the file
            if (!File.Exists(_settingsPath)) PrepareSettingsFirstTime();

            LoadSettings();
        }

        private void LoadSettings(bool alreadyFailed = false) {
            string rawSettings = null;

            try {
                rawSettings = File.ReadAllText(_settingsPath);

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
            string rawSettings = JsonConvert.SerializeObject(this.Settings, Formatting.Indented, JsonReaderSettings);

            try {
                using (var settingsWriter = new StreamWriter(_settingsPath, false)) {
                    settingsWriter.Write(rawSettings);
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
