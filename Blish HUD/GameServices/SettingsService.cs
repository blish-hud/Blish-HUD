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

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string SETTINGS_FILENAME = "settings.json";

        public delegate Control SettingTypeRendererDelegate(SettingEntry setting);

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
            try {
                string rawSettings = File.ReadAllText(_settingsPath);

                this.Settings = JsonConvert.DeserializeObject<SettingCollection>(rawSettings, JsonReaderSettings) ?? new SettingCollection(false);
            } catch (FileNotFoundException) {
                // Likely don't have access to this filesystem
            } catch (Exception ex) {
                if (alreadyFailed) {
                    Logger.Error(ex, "Failed to load settings due to an unexpected exception while attempting to read them. Already tried creating a new settings file, so we won't try again.");
                } else {
                    Logger.Error(ex, "Failed to load settings due to an unexpected exception while attempting to read them. A new settings file will be generated.");

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
                Logger.Error(ex, "Failed to save settings.");
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
            SettingTypeRenderers.Add(typeof(bool), (setting) => {
                var strongSetting = (SettingEntry<bool>) setting;

                var settingCtrl = new Checkbox() {
                    Text             = strongSetting.DisplayName,
                    BasicTooltipText = strongSetting.Description,
                    Checked          = strongSetting.Value
                };

                Adhesive.Binding.CreateTwoWayBinding(() => settingCtrl.Checked,
                                                     () => strongSetting.Value);

                return settingCtrl;
            });

            // enum setting renderer
            SettingTypeRenderers.Add(typeof(Enum), (setting) => {
                //var strongSetting = (SettingEntry<>) setting;

                var enumType = setting.SettingType;

                var settingCtrl = new Panel() {
                    Size = new Point(128, 25),
                };

                var settingDropDown = new Dropdown() {
                    Size     = new Point(128, 25),
                    Location = new Point(0, 0),
                    BasicTooltipText = setting.Description,
                    Parent = settingCtrl,
                };

                string[] values = Enum.GetNames(enumType);

                foreach (string enumOption in values) {
                    settingDropDown.Items.Add(enumOption.ToString());
                }

                //Adhesive.Binding.CreateTwoWayBinding(() => settingDropDown.SelectedItem,
                //                                     () => strongSetting.Value,
                //                                     (enumVal) => settingDropDown.SelectedItem = Enum.GetName(enumType, Convert.ChangeType(enumVal, enumType)),
                //                                     (settingVal) => strongSetting.Value       = Enum.Parse(enumType, settingVal));

                return settingCtrl;
            });

            // hotkey setting renderer
            SettingTypeRenderers.Add(typeof(Hotkey), (setting) => {
                var strongSetting = (SettingEntry<Hotkey>) setting;

                var hotkeyAssigner = new HotkeyAssigner(strongSetting.Value);

                return hotkeyAssigner;
            });
        }

        protected override void Load() {
            LoadRenderers();

            Director.FinishedLoading += delegate {
                Director.BlishHudWindow
                        .AddTab("Settings", Content.GetTexture("155052"), BuildSettingPanel(GameService.Director.BlishHudWindow), int.MaxValue - 1);
            };
        }

        internal SettingCollection RegisterRootSettingCollection(string collectionKey) {
            return this.Settings.DefineSetting(collectionKey, new SettingCollection(false)).Value;
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

        public void RenderSettingsToPanel(Container panel, IEnumerable<SettingEntry> settings) {
            var listSettings = settings.ToList();

            Logger.Info("Rendering {numberOfSettings} settings to panel.", listSettings.Count());

            int lastBottom = 0;

            void CreateRenderer(SettingEntry settingEntry, SettingTypeRendererDelegate renderer) {
                var settingCtrl = renderer.Invoke(settingEntry);
                settingCtrl.Location = new Point(0, lastBottom + 10);
                settingCtrl.Parent   = panel;

                lastBottom = settingCtrl.Bottom;
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
                Size       = new Point(baseSettingsPanel.Width - 720 - 10 - 10 - 5 - 20, baseSettingsPanel.Height - 50 - Panel.BOTTOM_MARGIN),
                Location   = new Point(5, 50),
                CanScroll  = true,
                Title      = "Settings",
                Parent     = baseSettingsPanel
            };

            var settingsListMenu = new Menu() {
                Size           = settingsMenuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent         = settingsMenuSection,
                CanSelect      = true,
            };

            Panel cPanel = new Panel() {
                Size     = new Point(748, baseSettingsPanel.Size.Y - Panel.BOTTOM_MARGIN * 2),
                Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, Panel.BOTTOM_MARGIN),
                Parent   = baseSettingsPanel
            };

            var settingsMi_About    = settingsListMenu.AddMenuItem("About",                Content.GetTexture("440023"));
            var settingsMi_App      = settingsListMenu.AddMenuItem("Application Settings", Content.GetTexture("156736"));
            var settingsMi_Controls = settingsListMenu.AddMenuItem("Hotkey Settings",      Content.GetTexture("156734"));
            var settingsMi_Update   = settingsListMenu.AddMenuItem("Check For Updates",    Content.GetTexture("156411"));
            var settingsMi_API      = settingsListMenu.AddMenuItem("API Settings",         Content.GetTexture("156684"));
            var settingsMi_Modules  = settingsListMenu.AddMenuItem("Manage Modules",       Content.GetTexture("156764-noarrow"));

            //settingsMi_Modules.Click += (object sender, MouseEventArgs e) => { wndw.Navigate(BuildModulePanel(wndw)); };

            var moduleMi_Module_Perms = new MenuItem("Module Permissions")
            {
                Parent = settingsMi_API,
                Icon = Content.GetTexture("155048")
            };

            var moduleMi_Module_Repo = new MenuItem("Manage Sources") {
                Parent = settingsMi_Modules,
                Icon   = Content.GetTexture("156140")
            };

            settingsMi_App.Click += delegate {
                cPanel.NavigateToBuiltPanel(GameServices.Director.ApplicationSettingsUIBuilder.BuildSingleModuleSettings, null);
            };

            settingsMi_Controls.Click += delegate {
                cPanel.NavigateToBuiltPanel(GameServices.Hotkeys.HotkeysSettingsUIBuilder.BuildApplicationHotkeySettings, null);
            };

            GameService.Module.FinishedLoading += delegate {
                foreach (var module in GameService.Module.Modules) {
                    var moduleMi = new MenuItem(module.Manifest.Name) {
                        BasicTooltipText = module.Manifest.Description,
                        //Icon             = module.Enabled ? Content.GetTexture("156149") : Content.GetTexture("156142"),
                        Parent           = settingsMi_Modules
                    };

                    moduleMi.Click += delegate {
                        cPanel.NavigateToBuiltPanel(GameServices.Module.SingleModuleSettingsUIBuilder.BuildSingleModuleSettings, module);
                    };
                }
            };

            //settingsMi_API.Click += delegate
            //{
            //    cPanel?.Hide();
            //    cPanel?.Dispose();

            //    cPanel = BuildApiPanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
            //    cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
            //    cPanel.Parent = baseSettingsPanel;
            //};

            //settingsMi_About.Click += delegate {
            //    cPanel?.Hide();
            //    cPanel?.Dispose();

            //    cPanel          = BuildAboutPanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
            //    cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
            //    cPanel.Parent   = baseSettingsPanel;
            //};

            //settingsMi_Modules.Click += delegate {
            //    cPanel?.Hide();
            //    cPanel?.Dispose();

            //    cPanel = BuildModulePanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
            //    cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
            //    cPanel.Parent = baseSettingsPanel;
            //};

            //moduleMi_Module_Perms.Click += delegate
            //{
            //    cPanel?.Hide();
            //    cPanel?.Dispose();

            //    cPanel = BuildModulePermissionsPanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
            //    cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
            //    cPanel.Parent = baseSettingsPanel;
            //};

            //settingsMi_Exit.Click += delegate { ActiveOverlay.Exit(); };



            return baseSettingsPanel;
        }
        private Panel BuildApiPanel(Point size) {
            return null;

            //Dictionary<Guid, string> ApiKeys = Settings.DefineSetting()
            //    .GetSetting<Dictionary<Guid, string>>(Gw2ApiService.SETTINGS_ENTRY_APIKEYS)
            //    .Value;
            Dictionary<string, string> foolSafeKeyRepository = GameService.Gw2Api.GetKeyIdRepository();

            var apiPanel = new Panel() { CanScroll = false, Size = size };

            var keySelectionDropdown = new Dropdown()
            {
                Parent = apiPanel,
                Size = new Point(200, 30),
                Location = new Point(apiPanel.Size.X - 200 - Panel.RIGHT_MARGIN, Panel.TOP_MARGIN),
                Visible = foolSafeKeyRepository.Count > 0,
                SelectedItem = foolSafeKeyRepository.FirstOrDefault().Key
            };
            foreach (KeyValuePair<string,string> item in foolSafeKeyRepository)
            {
                keySelectionDropdown.Items.Add(item.Key);
            }
            var connectedLabel = new Label()
            {
                Parent = apiPanel,
                Size = new Point(apiPanel.Size.X, 30),
                Location = new Point(0, apiPanel.Size.Y - Panel.BOTTOM_MARGIN - 15),
                ShowShadow = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = Gw2Api.Connected ? "OK! Connected. :-)" : "Not connected.",
                TextColor = Gw2Api.Connected ? Color.LightGreen : Color.IndianRed
            };
            var apiKeyLabel = new Label()
            {
                Parent = apiPanel,
                Size = new Point(apiPanel.Size.X, 30),
                Location = new Point(0, apiPanel.Size.Y / 2 - apiPanel.Size.Y / 4 - 15),
                ShowShadow = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Insert your Guild Wars 2 API key here to unlock lots of cool features:"
            };
            var apiKeyTextBox = new TextBox() {
                Parent = apiPanel,
                Size = new Point(600, 30),
                Location = new Point(apiPanel.Size.X / 2 - 300, apiKeyLabel.Bottom),
                PlaceholderText = keySelectionDropdown.SelectedItem != null ?
                    foolSafeKeyRepository[keySelectionDropdown.SelectedItem] +
                    Gw2ApiService.PLACEHOLDER_KEY.Substring(foolSafeKeyRepository.FirstOrDefault().Value.Length - 1)
                    : Gw2ApiService.PLACEHOLDER_KEY
            };
            var apiKeyError = new Label()
            {
                Parent = apiPanel,
                Size = new Point(apiPanel.Size.X, 30),
                Location = new Point(0, apiKeyTextBox.Bottom + Panel.BOTTOM_MARGIN),
                ShowShadow = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor = Color.Red,
                Text = "Invalid API key! Try again.",
                Visible = false
            };
            var apiKeyButton = new StandardButton()
            {
                Parent = apiPanel,
                Size = new Point(30, 30),
                Location = new Point(apiKeyTextBox.Right, apiKeyTextBox.Location.Y),
                Text = "",
                BackgroundColor = Color.IndianRed,
                Visible = keySelectionDropdown.SelectedItem != null
            };
            apiKeyButton.LeftMouseButtonPressed += delegate
            {
                Gw2Api.RemoveKey(foolSafeKeyRepository[keySelectionDropdown.SelectedItem]);

                keySelectionDropdown.Items.Clear();
                foolSafeKeyRepository = GameService.Gw2Api.GetKeyIdRepository();
                foreach (KeyValuePair<string, string> item in foolSafeKeyRepository)
                {
                    keySelectionDropdown.Items.Add(item.Key);
                }
                keySelectionDropdown.Visible = foolSafeKeyRepository.Count > 0;
                keySelectionDropdown.SelectedItem = foolSafeKeyRepository.FirstOrDefault().Key;

                apiKeyTextBox.PlaceholderText = keySelectionDropdown.SelectedItem != null ?
                    foolSafeKeyRepository[keySelectionDropdown.SelectedItem] +
                    Gw2ApiService.PLACEHOLDER_KEY.Substring(foolSafeKeyRepository.FirstOrDefault().Value.Length - 1)
                    : Gw2ApiService.PLACEHOLDER_KEY;

                apiKeyError.Visible = false;
                apiKeyButton.Visible = keySelectionDropdown.Visible;
                bool valid = Gw2Api.Invalidate();
                connectedLabel.Text = valid ? "OK! Connected. :-)" : "Not connected.";
                connectedLabel.TextColor = valid ? Color.LightGreen : Color.IndianRed;
            };
            //apiKeyTextBox.OnEnterPressed += delegate
            //{
            //    string apiKey = apiKeyTextBox.Text;
            //    string errorMsg = null;
            //    if (!Gw2ApiService.IsKeyValid(apiKey))
            //    {
            //        errorMsg = "Not an API key! Invalid pattern.";
            //        apiKeyError.TextColor = Color.IndianRed;
            //    }
            //    else if (ApiKeys.Any(entry => entry.Value.Contains(apiKey)))
            //    {
            //        errorMsg = "API key already registered!";
            //        apiKeyError.TextColor = Color.LightGreen;
            //    }
            //    else if (!Gw2Api.HasPermissions(new[]
            //          {
            //            Gw2Sharp.WebApi.V2.Models.TokenPermission.Account,
            //            Gw2Sharp.WebApi.V2.Models.TokenPermission.Characters
            //        },
            //      apiKey))
            //    {
            //        errorMsg = "Insufficient permissions! Required: Account, Characters.";
            //        apiKeyError.TextColor = Color.IndianRed;
            //    }
            //    if (errorMsg != null) {

            //        apiKeyError.Visible = true;
            //        apiKeyError.Text = errorMsg;
            //        apiKeyTextBox.Text = "";

            //    } else {
            //        Gw2Api.RegisterKey(apiKeyTextBox.Text);
            //        apiKeyTextBox.PlaceholderText = apiKeyTextBox.Text;
            //        apiKeyTextBox.Text = "";
            //        apiKeyError.Text = "Success! Key added. :-)";
            //        apiKeyError.TextColor = Color.LightGreen;
            //        apiKeyError.Visible = true;
            //        keySelectionDropdown.Items.Clear();
            //        foolSafeKeyRepository = Gw2ApiService.GetKeyIdRepository();
            //        foreach (KeyValuePair<string, string> item in foolSafeKeyRepository)
            //        {
            //            keySelectionDropdown.Items.Add(item.Key);
            //        }
            //        keySelectionDropdown.SelectedItem = foolSafeKeyRepository.LastOrDefault().Key;
            //        keySelectionDropdown.Visible = true;
            //        apiKeyButton.Visible = true;
            //        bool valid = Gw2Api.Invalidate();
            //        connectedLabel.Text = valid ? "OK! Connected. :-)" : "Not connected.";
            //        connectedLabel.TextColor = valid ? Color.LightGreen : Color.IndianRed;
            //    }
            //    Overlay.ResetFocus();
            //};

            //keySelectionDropdown.ValueChanged += delegate {
            //    apiKeyTextBox.PlaceholderText = 
            //    keySelectionDropdown.SelectedItem != null ? 
            //        foolSafeKeyRepository[keySelectionDropdown.SelectedItem] + 
            //        Gw2ApiService.PLACEHOLDER_KEY.Substring(foolSafeKeyRepository.FirstOrDefault().Value.Length - 1) 
            //        : Gw2ApiService.PLACEHOLDER_KEY;

            //    apiKeyButton.Visible = true;
            //};
            //return apiPanel;
        }
        private Panel BuildModulePermissionsPanel(Point size) {
            var permissionsPanel = new Panel() { CanScroll = false, Size = size };
            // All modules that require GW2 API.
            var apiModules = Module.Modules.Where(x => x.Manifest.ApiPermissions != null);

            if (!apiModules.Any()) {
                var noApiModulesLabel = new Label() {
                    Parent = permissionsPanel,
                    Size = new Point(permissionsPanel.Size.X, 30),
                    Location = new Point(0, permissionsPanel.Size.Y / 2 - 15),
                    TextColor = Color.Orange,
                    ShowShadow = true,
                    StrokeText = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = "None of the registered modules require the API service."
                };
                return permissionsPanel;
            }
            // Key = module name, Value = module namespace
            Dictionary<string, string> nameSpaceRepository = new Dictionary<string, string>();

            var moduleSelectionDropdown = new Dropdown()
            {
                Parent = permissionsPanel,
                Size = new Point(200, 30),
                Location = new Point(Panel.LEFT_MARGIN, Panel.TOP_MARGIN)
            };
            foreach (var module in apiModules)
            {
                string name = module.Manifest.Name;
                nameSpaceRepository.Add(name, module.Manifest.Namespace);
                moduleSelectionDropdown.Items.Add(name);
            }
            moduleSelectionDropdown.SelectedItem = null;
            var permissionCheckBoxs = new List<Checkbox>();
            int boxY = 0;
            foreach (Gw2Sharp.WebApi.V2.Models.TokenPermission perm in Gw2ApiService.ALL_PERMISSIONS)
            {
                var newBox = new Checkbox()
                {
                    Parent = permissionsPanel,
                    Location = new Point(Panel.LEFT_MARGIN, moduleSelectionDropdown.Bottom + boxY + Panel.BOTTOM_MARGIN),
                    Size = new Point(100, 30),
                    Text = perm.ToString(),
                    Visible = false
                };
                permissionCheckBoxs.Add(newBox);
                boxY += 30;

                newBox.CheckedChanged += delegate {
                    var buildPermissions = new List<Gw2Sharp.WebApi.V2.Models.TokenPermission>();
                    foreach (Checkbox check in permissionCheckBoxs) {
                        if (check.Checked) {
                            buildPermissions.Add(Gw2ApiService.ALL_PERMISSIONS.First(x => x.ToString().Equals(check.Text)));
                        }
                    }
                    //string nSpace = nameSpaceRepository[moduleSelectionDropdown.SelectedItem];
                    //var saved = RegisteredSettings[nSpace]
                    //    .GetSetting<List<Gw2Sharp.WebApi.V2.Models.TokenPermission>>(Gw2ApiService.SETTINGS_ENTRY_PERMISSIONS);
                    //// Save new permissions.
                    //saved.Value = buildPermissions;
                };
            }
            moduleSelectionDropdown.ValueChanged += delegate {
                string new_value = moduleSelectionDropdown.SelectedItem;
                var module = Module
                    .Modules.First(i => i.Manifest.Namespace.Equals(nameSpaceRepository[new_value]));
                
                var permissions = Gw2ApiService.GetModulePermissions(module).Select(x => x.ToString());
                foreach (Checkbox box in permissionCheckBoxs) {
                    box.Checked = permissions.Contains(box.Text);
                    box.Visible = true;
                }
            };
            moduleSelectionDropdown.SelectedItem = moduleSelectionDropdown.Items.FirstOrDefault();
            return permissionsPanel;
        }
        // TODO: All of this needs to be somewhere else

        private const string ANET_COPYRIGHT_NOTICE =
            @"©2010–2018 ArenaNet, LLC. All rights reserved. Guild Wars, Guild Wars 2, Heart of Thorns,
            Guild Wars 2: Path of Fire, ArenaNet, NCSOFT, the Interlocking NC Logo, and all associated
            logos and designs are trademarks or registered trademarks of NCSOFT Corporation. All other
            trademarks are the property of their respective owners.";

        private string ANET_COPYRIGHT_NOTICE_CLEAN =
            "(C) 2010 - 2019 ArenaNet, LLC. All rights reserved. Guild Wars, Guild Wars 2, Heart of Thorns,|" +
            "Guild Wars 2: Path of Fire, ArenaNet, NCSOFT, the Interlocking NC Logo, and all associated|" +
            "logos and designs are trademarks or registered trademarks of NCSOFT Corporation. All other|" +
            "trademarks are the property of their respective owners.";

        private const string LICENSES_FILE = "licenses.json";

        private const string DISCORD_INVITE = "https://discord.gg/78PYm77";
        private const string SUBREDDIT_URL = "https://www.reddit.com/r/blishhud";

        private Panel BuildAboutPanel(Point size) {
            var asPanel = new Panel() {CanScroll = false, Size = size};

            //var backButton = new BackButton(wndw) {
            //    Text     = "Settings",
            //    NavTitle = "About",
            //    Parent   = asPanel,
            //    Location = new Point(20, 20),
            //};

            // TODO: Label should support multi-line strings and should still support alignment
            // I hate everything about this - there needs to be a better way.

            var cleanNotice = ANET_COPYRIGHT_NOTICE_CLEAN;

            var copyrightNotice1 = new Label() {
                Text = cleanNotice.Split('|')[0],
                AutoSizeHeight = true,
                Width               = size.X,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = asPanel
            };

            var copyrightNotice2 = new Label() {
                Text = cleanNotice.Split('|')[1],
                Location            = new Point(0, copyrightNotice1.Bottom + 5),
                AutoSizeHeight      = true,
                Width = size.X,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = asPanel
            };

            var copyrightNotice3 = new Label() {
                Text = cleanNotice.Split('|')[2],
                Location            = new Point(0, copyrightNotice2.Bottom + 5),
                AutoSizeHeight      = true,
                Width = size.X,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = asPanel
            };
            
            var copyrightNotice4 = new Label() {
                Text = cleanNotice.Split('|')[3],
                Location            = new Point(0, copyrightNotice3.Bottom + 5),
                AutoSizeHeight      = true,
                Width = size.X,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent              = asPanel
            }; 

            // OSS

            var ossNotice = new Label() {
                Text           = "Blish HUD makes use of multiple open source libraries:",
                Location       = new Point(20 + 3, copyrightNotice4.Bottom + 25),
                AutoSizeHeight = true,
                Width          = size.X - 20 - 3,
                //Parent         = asPanel
            };

            //var ossPanel = new TintedPanel() {
            //    Parent    = asPanel,
            //    Size      = new Point(asPanel.ContentRegion.Width - 60, wndw.ContentRegion.Height - ossNotice.Bottom - 76),
            //    Location  = new Point(20,                               ossNotice.Bottom + 10),
            //    CanScroll = true,
            //};

            //string licenseFilePath = Path.Combine("data", LICENSES_FILE);
            //if (File.Exists(licenseFilePath)) {
            //    string rawRefLicense = File.ReadAllText(licenseFilePath);

            //    var licenseReference = JArray.Parse(rawRefLicense);

            //    int lPos = 10;

            //    foreach (JObject project in licenseReference) {

            //        if (project.TryGetValue("project", out var name)) {
            //            string sName = name.Value<string>();

            //            var projectNameLbl = new LabelBase() {
            //                Text              = sName,
            //                Location          = new Point(ossNotice.Left, lPos),
            //                Width             = 256,
            //                Height            = 26,
            //                VerticalAlignment = VerticalAlignment.Middle,
            //                Parent            = ossPanel
            //            };

            //            if (project.TryGetValue("license", out var license)) {
            //                string sLicense = license.Value<string>();

            //                if (Uri.TryCreate(sLicense, UriKind.Absolute, out var licenseUri) && (licenseUri.Scheme == Uri.UriSchemeHttp || licenseUri.Scheme == Uri.UriSchemeHttps)) {

            //                    var licenseBttn = new StandardButton() {
            //                        Text     = "View License",
            //                        Location = new Point(projectNameLbl.Right + 10, lPos),
            //                        Width    = 128,
            //                        Height   = 26,
            //                        Parent   = ossPanel
            //                    };

            //                    licenseBttn.LeftMouseButtonReleased += (sender, args) => {
            //                        wndw.Navigate(BuildLicensePanel(wndw, sName, sLicense));
            //                    };
            //                }
            //            }

            //            if (project.TryGetValue("repository", out var repository)) {
            //                string sRepository = repository.Value<string>();

            //                if (Uri.TryCreate(sRepository, UriKind.Absolute, out var repoUri) && (repoUri.Scheme == Uri.UriSchemeHttp || repoUri.Scheme == Uri.UriSchemeHttps)) {

            //                    var repoBttn = new StandardButton() {
            //                        Text     = "Repository",
            //                        Location = new Point(projectNameLbl.Right + 128 + 15, lPos),
            //                        Width    = 128,
            //                        Height   = 26,
            //                        Parent   = ossPanel
            //                    };

            //                    repoBttn.LeftMouseButtonReleased += (sender, args) => { Process.Start(sRepository); };
            //                }
            //            }

            //            lPos = projectNameLbl.Bottom + 5;
            //        }
            //    }


            //}

            //var joinDiscordBttn = new StandardButton() {
            //    Text     = "Join the Blish HUD Discord Channel",
            //    Width    = 280,
            //    Height   = 26,
            //    Location = new Point(20, wndw.ContentRegion.Height - 26),
            //    Parent   = asPanel,
            //};

            //var subredditBttn = new StandardButton() {
            //    Text     = "Visit the Blish HUD Subreddit",
            //    Width    = 280,
            //    Height   = 26,
            //    Location = new Point(joinDiscordBttn.Right + 10, wndw.ContentRegion.Height - 26),
            //    Parent   = asPanel,
            //};

            //joinDiscordBttn.LeftMouseButtonReleased += delegate { Process.Start(DISCORD_INVITE); };
            //subredditBttn.LeftMouseButtonReleased += delegate { Process.Start(SUBREDDIT_URL); };

//            var versionLbl = new LabelBase() {
//#if SENTRY
//                Text           = $"{Program.APP_VERSION} (Sentry Enabled)",
//#else
//                Text           = Program.APP_VERSION,
//#endif
//                AutoSizeWidth  = true,
//                AutoSizeHeight = true,
//                Parent         = asPanel
//            };

//            versionLbl.Location = new Point(wndw.ContentRegion.Width - versionLbl.Width - 20, wndw.ContentRegion.Height - versionLbl.Height);

            return asPanel;
        }

        private Panel BuildLicensePanel(Controls.WindowBase wndw, string name, string licenseUrl) {
            var lPanel = new Panel() {CanScroll = false, Size = wndw.ContentRegion.Size };

            var backButton = new BackButton(wndw) {
                Text     = "License",
                NavTitle = name,
                Parent   = lPanel,
                Location = new Point(20, 20),
            };

            var textPanel = new TintedPanel() {
                Parent = lPanel,
                CanScroll = true,
                Size = new Point(lPanel.ContentRegion.Width - 40, lPanel.ContentRegion.Height - backButton.Bottom - 40),
                Location = new Point(20, backButton.Bottom + 20)
            };

            var lLoader = new LoadingSpinner() { Parent = lPanel };
            lLoader.Location = textPanel.Size / new Point(2) - lLoader.Size / new Point(2);

            int lPos = 20;

            licenseUrl.GetAsync().ContinueWith(async (httpResponseMessage) => {
                                                   string rawLicense = await httpResponseMessage.Result.Content.ReadAsStringAsync();

                                                   if (!(rawLicense.Length > 0)) {
                                                       rawLicense = "There was a problem loading the license from:" + '\n' + licenseUrl;
                                                   }

                                                   foreach (string licenseLine in rawLicense.Split('\n')) {
                                                       var lineLbl = new Label() {
                                                           Text = licenseLine,
                                                           Location = new Point(backButton.Left, lPos),
                                                           AutoSizeHeight = true,
                                                           Width = wndw.ContentRegion.Width,
                                                           Parent = textPanel
                                                       };

                                                       lPos = lineLbl.Bottom + 5;

                                                       if (!(lineLbl.Text.Trim().Length > 0))
                                                           lineLbl.Dispose();
                                                    }

                                                    lLoader.Dispose();
                                               });

            return lPanel;
        }

    }
}
