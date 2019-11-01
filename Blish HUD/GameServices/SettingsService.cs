using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Flurl.Http;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Container = Blish_HUD.Controls.Container;
using Point = Microsoft.Xna.Framework.Point;

namespace Blish_HUD {

    [JsonObject]
    public class SettingsService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<SettingsService>();

        private const string SETTINGS_FILENAME = "settings.json";

        public delegate void SettingTypeRendererDelegate(SettingEntry setting, Panel settingPanel);

        [JsonIgnore]
        public Dictionary<Type, SettingTypeRendererDelegate> SettingTypeRenderers = new Dictionary<Type, SettingTypeRendererDelegate>();

        [JsonIgnore]
        internal JsonSerializerSettings JsonReaderSettings;

        [JsonIgnore]
        internal JsonSerializer SettingsReader;

        [JsonIgnore]
        private string _settingsPath;

        internal SettingCollection Settings { get; private set; }

        protected override void Initialize() {
            JsonReaderSettings = new JsonSerializerSettings() {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling           = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>() {
                    new SettingCollection.SettingCollectionConverter(),
                    new SettingEntry.SettingEntryConverter()
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

            string rawSettings = JsonConvert.SerializeObject(this.Settings, Formatting.Indented, JsonReaderSettings);

            try {
                using (var settingsWriter = new StreamWriter(_settingsPath, false)) {
                    settingsWriter.Write(rawSettings);
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to save settings.");
                return;
            }

            Logger.Info("Settings were saved successfully.");
        }

        internal void SettingSave() {
            this.Save();
        }

        private void LoadRenderers() {
            SettingTypeRenderers.Clear();

            // bool setting renderer
            SettingTypeRenderers.Add(typeof(bool), (setting, panel) => {
                var strongSetting = (SettingEntry<bool>) setting;

                var settingCtrl = new Checkbox() {
                    Text             = strongSetting.DisplayName,
                    BasicTooltipText = strongSetting.Description,
                    Checked          = strongSetting.Value,
                    Parent           = panel
                };

                Adhesive.Binding.CreateTwoWayBinding(() => settingCtrl.Checked,
                                                     () => strongSetting.Value);
            });
        }

        protected override void Load() {
            LoadRenderers();

            Overlay.FinishedLoading += delegate {
                Overlay.BlishHudWindow.AddTab(Strings.GameServices.SettingsService.SettingsTab, Content.GetTexture("155052"), BuildSettingPanel(GameService.Overlay.BlishHudWindow), int.MaxValue - 1);
            };
        }

        internal SettingCollection RegisterRootSettingCollection(string collectionKey) {
            return this.Settings.DefineSetting(collectionKey, new SettingCollection(false)).Value;
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

        public void RenderSettingsToPanel(Container panel, IEnumerable<SettingEntry> settings, int width = 325) {
            var listSettings = settings.ToList();

            Logger.Info("Rendering {numberOfSettings} settings to panel.", listSettings.Count());

            int lastBottom = 0;

            void CreateRenderer(SettingEntry settingEntry, SettingTypeRendererDelegate renderer) {
                var settingPanel = new Panel() {
                    Size     = new Point(width, 25),
                    Location = new Point(0,     lastBottom + 10)
                };

                renderer.Invoke(settingEntry, settingPanel);

                settingPanel.Parent = panel;

                lastBottom = settingPanel.Bottom;
            }

            foreach (var settingEntry in listSettings) {
                if (settingEntry.Renderer != null) {
                    CreateRenderer(settingEntry, settingEntry.Renderer);
                    continue;
                }

                if (this.SettingTypeRenderers.TryGetValue(settingEntry.SettingType, out var matchingRenderer)) {
                    CreateRenderer(settingEntry, matchingRenderer);
                    continue;
                }


                var subTypeRenderer = this.SettingTypeRenderers.FirstOrDefault(kv => settingEntry.SettingType.IsSubclassOf(kv.Key)).Value;
                if (subTypeRenderer != null) {
                    CreateRenderer(settingEntry, subTypeRenderer);
                    continue;
                }

                Logger.Warn("Could not identify a setting renderer for setting {settingName} of type {settingType}, so it will not be displayed.", settingEntry.EntryKey, settingEntry.SettingType.FullName);
            }
        }

        private Panel BuildSettingPanel(Controls.WindowBase wndw) {
            var baseSettingsPanel = new Panel() {
                Size = wndw.ContentRegion.Size
            };

            var settingsMenuSection = new Panel() {
                ShowBorder = true,
                Size       = new Point(baseSettingsPanel.Width - 720 - 10 - 10 - 5 - 20, baseSettingsPanel.Height - 50 - 24),
                Location   = new Point(5,                                                50),
                Title      = Strings.GameServices.SettingsService.SettingsTab,
                Parent     = baseSettingsPanel,
                CanScroll  = true,
            };

            var settingsListMenu = new Menu() {
                Size           = settingsMenuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent         = settingsMenuSection,
                CanSelect      = true,
            };

            var cPanel = new ViewContainer() {
                Size     = new Point(748, baseSettingsPanel.Size.Y - 24 * 2),
                Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 24),
                Parent   = baseSettingsPanel
            };

            var settingsMiAbout   = settingsListMenu.AddMenuItem(Strings.GameServices.OverlayService.AboutSection,           Content.GetTexture("440023"));
            var settingsMiOverlay = settingsListMenu.AddMenuItem(Strings.GameServices.OverlayService.OverlaySettingsSection, Content.GetTexture("156736"));
            var settingsMiModules = settingsListMenu.AddMenuItem(Strings.GameServices.ModulesService.ManageModulesSection,   Content.GetTexture("156764-noarrow"));

            settingsMiAbout.Click += delegate {
                cPanel.NavigateToBuiltPanel(Blish_HUD.Settings.UI.AboutUIBuilder.BuildAbout, null);
            };

            settingsMiOverlay.Click += delegate {
                cPanel.NavigateToBuiltPanel(Blish_HUD.Settings.UI.OverlaySettingsUIBuilder.BuildOverlaySettings, null);
            };

            GameService.Module.FinishedLoading += delegate {
                foreach (var module in GameService.Module.Modules) {
                    var moduleMi = new MenuItem(module.Manifest.Name) {
                        BasicTooltipText = module.Manifest.Description,
                        Parent           = settingsMiModules
                    };

                    moduleMi.Click += delegate {
                        cPanel.NavigateToBuiltPanel(Blish_HUD.Settings.UI.SingleModuleSettingsUIBuilder.BuildSingleModuleSettings, module);
                    };
                }
            };

            return baseSettingsPanel;
        }

    }
}
