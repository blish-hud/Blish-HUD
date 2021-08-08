using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class SettingsService : GameService {

        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions {
            AllowTrailingCommas = true,
            Converters = {
                new JsonStringEnumConverter(),
                new SettingCollectionConverter()
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        private static readonly Logger Logger = Logger.GetLogger<SettingsService>();

        private const int SAVE_INTERVAL = 4;

        private const string SETTINGS_FILENAME = "settings.json";

        [Obsolete]
        public delegate void SettingTypeRendererDelegate(ISettingEntry setting, Panel settingPanel);

        private string _settingsPath;

        internal SettingCollection Settings { get; private set; }

        private bool   _dirtySave;
        private double _saveBuffer;

        protected override void Initialize() {
            _settingsPath = Path.Combine(DirectoryUtil.BasePath, SETTINGS_FILENAME);

            // If settings aren't there, generate the file
            if (!File.Exists(_settingsPath)) PrepareSettingsFirstTime();

            LoadSettings();
        }

        private void LoadSettings(bool alreadyFailed = false) {
            string rawSettings = null;

            try {
                rawSettings = File.ReadAllText(_settingsPath);

                this.Settings = JsonSerializer.Deserialize<SettingCollection>(rawSettings, _jsonSerializerOptions) ?? new SettingCollection();
                this.Settings.PropertyChanged += Settings_PropertyChanged;
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
            this.Settings.PropertyChanged += Settings_PropertyChanged;
            Save(true);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Save();
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
            string rawSettings = JsonSerializer.Serialize(this.Settings, _jsonSerializerOptions);

            try {
                using (var settingsWriter = new StreamWriter(_settingsPath, false)) {
                    settingsWriter.Write(rawSettings);
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to save settings.");
                return;
            }

            _saveBuffer = 0;
            _dirtySave  = false;

            Logger.Debug("Settings were saved successfully.");
        }

        protected override void Load() { /* NOOP */ }

        internal ISettingCollection RegisterRootSettingCollection(string collectionKey) {
            return this.Settings.AddSubCollection(collectionKey);
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
