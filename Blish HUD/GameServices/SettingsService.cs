using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Blish_HUD.Annotations;
using Blish_HUD.Controls;
using Blish_HUD.Utils;
using Flurl.Http;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Container = Blish_HUD.Controls.Container;
using Point = Microsoft.Xna.Framework.Point;

namespace Blish_HUD {

#region Custom Setting Types

    public abstract class CustomSettingType {



    }

#endregion

    [JsonObject]
    public abstract class SettingEntry : INotifyPropertyChanged {

        [JsonIgnore]
        public bool ExposedAsSetting { get; set; }

        [JsonIgnore]
        public string Description { get; set; }

        protected abstract Type GetSettingType();

        [JsonIgnore]
        public Type SettingType => GetSettingType();

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    [JsonObject]
    public class SettingEntry<T> : SettingEntry {
        
        public delegate void SettingChangedDelegate(string settingName, T prevValue, T newValue);

        public event SettingChangedDelegate OnSettingChanged;
        
        protected override Type GetSettingType() {
            return typeof(T);
        }

        [JsonProperty]
        public T DefaultValue { get; set; }

        [JsonProperty]
        public T _value;
        
        [JsonIgnore]
        public T Value {
            get => _value;
            set {
                if (object.Equals(_value, value)) return;

                var prevValue = this.Value;
                _value = value;
                
                if (Owner.AutoSave)
                    GameService.Settings.Save();

                OnPropertyChanged();

                // TODO: Get the setting name passed along to the event at some point
                this.OnSettingChanged?.Invoke("", prevValue, this.Value);
            }
        }

        [JsonProperty]
        public readonly Settings Owner;

        public SettingEntry() { /* NOOP */ }

        protected SettingEntry(Settings owner, T defaultValue = default) {
            this.Owner = owner;
            this.DefaultValue = defaultValue;
        }

        public static SettingEntry<T> InitSetting(Settings owner, T value, T defaultValue) {
            var newSetting = new SettingEntry<T>(owner, defaultValue) {
                _value = value
            };

            return newSetting;
        }

        public static SettingEntry<T> InitSetting(Settings owner, T defaultValue) {
            return InitSetting(owner, defaultValue, defaultValue);
        }

    }

    [JsonObject]
    public class Settings {

        [JsonProperty]
        public Dictionary<string, SettingEntry> _entries = new Dictionary<string, SettingEntry>();

        [JsonIgnore]
        public IReadOnlyDictionary<string, SettingEntry> Entries => 
            _entries ?? (_entries = new Dictionary<string, SettingEntry>());

        [JsonIgnore]
        // TODO: Consider re-enabling the option to set autoSave on or off
        public bool AutoSave {
            get => true;
        }

        public Settings() { /* NOOP */ }

        public Settings(bool autoSave = false) {
            //this.AutoSave = true;
        }

        public SettingEntry<T> DefineSetting<T>(string name, T value, T defaultValue, bool exposedAsSetting = false, string description = "") {
            SettingEntry<T> actSetting;

            if (!this.Entries.ContainsKey(name)) {
                actSetting = SettingEntry<T>.InitSetting(this, value, defaultValue);
                _entries.Add(name, actSetting);
            } else {
                actSetting = (SettingEntry<T>)this.Entries[name];
                actSetting.DefaultValue = defaultValue;
            }

            actSetting.ExposedAsSetting = exposedAsSetting;
            actSetting.Description = description;

            return actSetting;
        }

        public SettingEntry<T> GetSetting<T>(string name) {
            if (!this.Entries.ContainsKey(name))
                throw new Exception("Attempted to query setting before it has been been set.");

            return (SettingEntry<T>)this.Entries[name];
        }

    }

    [JsonObject]
    public class SettingsService:GameService {

        public delegate Control SettingTypeRendererDelegate(string settingName, SettingEntry setting);

        [JsonIgnore]
        public Dictionary<Type, SettingTypeRendererDelegate> SettingTypeRenderers = new Dictionary<Type, SettingTypeRendererDelegate>();

        private const string SETTINGS_FILENAME = "settings.json";

        //private string _apikey;
        //public string ApiKey { get { return _apikey; } set { _apikey = value; } }

        [JsonProperty]
        public Settings CoreSettings { get; private set; }

        [JsonProperty]
        public Dictionary<string, Settings> _registeredSettings;

        [JsonIgnore]
        private JsonSerializerSettings _jsonSettings;

        [JsonIgnore]
        private string _settingsPath;

        public Settings RegisterSettings(string setName, bool autoSave = false) {
            if (_registeredSettings.ContainsKey(setName))
                return _registeredSettings[setName];

            var aSettings = new Settings(autoSave);

            _registeredSettings.Add(setName, aSettings);

            return aSettings;
        }

        protected override void Initialize() {
            _registeredSettings      = new Dictionary<string, Settings>();
            this.CoreSettings        = new Settings();

            _jsonSettings = new JsonSerializerSettings() {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling           = TypeNameHandling.Auto
            };

            _settingsPath = Path.Combine(GameService.FileSrv.BasePath, SETTINGS_FILENAME);

            // If settings aren't there, generate the file
            if (!File.Exists(_settingsPath)) Save();

            // Would prefer to have this under Load(), but SettingsService needs to be ready for other modules and services

            try {
                string rawSettings = File.ReadAllText(_settingsPath);

                JsonConvert.PopulateObject(rawSettings, this, _jsonSettings);
            } catch (System.IO.FileNotFoundException) {
                // Likely don't have access to this filesystem
            } catch (Exception e) {
                // TODO: If this fails, we may need to prompt the user to re-generate the settings (in case they were corrupted or something)
                Console.WriteLine(e.Message);
            }
        }

        public void Save() {
            string rawSettings = JsonConvert.SerializeObject(this, Formatting.Indented, _jsonSettings);

            try {
                using (var settingsWriter = new StreamWriter(_settingsPath, false)) {
                    settingsWriter.Write(rawSettings);
                }
            } catch (Exception e) {
                Console.WriteLine("Failed to write settings to file!");
                // TODO: We need to try saving the file again later - something is preventing us from saving
            }
        }

        private void LoadRenderers() {
            SettingTypeRenderers.Clear();

            // bool setting renderer
            SettingTypeRenderers.Add(typeof(bool), (name, setting) => {
                var strongSetting = (SettingEntry<bool>)setting;

                var settingCtrl = new Checkbox() {
                    Text = name,
                    BasicTooltipText = strongSetting.Description,
                    Checked = strongSetting.Value
                };

                //Binding.Create(() => settingCtrl.Checked == strongSetting.Value);
                Adhesive.Binding.CreateTwoWayBinding(() => settingCtrl.Checked,
                                                     () => strongSetting.Value);

                return settingCtrl;
            });
        }

        protected override void Load() {
            LoadRenderers();

            Director.OnLoad += delegate {
                Director.BlishHudWindow
                        .AddTab("Settings", Content.GetTexture("155052"), BuildSettingPanel(GameService.Director.BlishHudWindow), int.MaxValue - 1);
            };
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

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
                Size     = settingsMenuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent   = settingsMenuSection
            };

            Panel cPanel = null;

            var settingsMi_App = settingsListMenu.AddMenuItem("Application Settings", Content.GetTexture("156736"));
            var settingsMi_Controls = settingsListMenu.AddMenuItem("Control Settings", Content.GetTexture("156734"));
            var settingsMi_Sound = settingsListMenu.AddMenuItem("Sound Settings", Content.GetTexture("156738"));
            var settingsMi_Modules = settingsListMenu.AddMenuItem("Manage Modules", Content.GetTexture("156764-noarrow"));
            var settingsMi_API = settingsListMenu.AddMenuItem("API Settings", Content.GetTexture("156684"));
            var settingsMi_Update = settingsListMenu.AddMenuItem("Check For Updates", Content.GetTexture("156411"));
            var settingsMi_SupportUs = settingsListMenu.AddMenuItem("Support the Project", Content.GetTexture("156331"));
            var settingsMi_About = settingsListMenu.AddMenuItem("About", Content.GetTexture("440023"));
            var settingsMi_Exit = settingsListMenu.AddMenuItem("Close Blish HUD", Content.GetTexture("155049"));

            //settingsMi_Modules.Click += (object sender, MouseEventArgs e) => { wndw.Navigate(BuildModulePanel(wndw)); };

            var moduleMi_Module_Repo = new MenuItem("Manage Sources") {
                Parent = settingsMi_Modules,
                Icon   = Content.GetTexture("156140")
            };
            GameService.Module.OnLoad += delegate {
                foreach (var module in GameService.Module.AvailableModules) {
                    var moduleInfo = module.GetModuleInfo();

                    var moduleMi = new MenuItem(moduleInfo.Name) {
                        BasicTooltipText = moduleInfo.Description,
                        Icon             = module.Enabled ? Content.GetTexture("156149") : Content.GetTexture("156142"),
                        Parent           = settingsMi_Modules
                    };
                }
            };

            settingsMi_API.Click += delegate
            {
                cPanel?.Hide();
                cPanel?.Dispose();

                cPanel = BuildApiPanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
                cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
                cPanel.Parent = baseSettingsPanel;
            };

            settingsMi_About.Click += delegate {
                cPanel?.Hide();
                cPanel?.Dispose();

                cPanel          = BuildAboutPanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
                cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
                cPanel.Parent   = baseSettingsPanel;
            };

            settingsMi_Modules.Click += delegate {
                cPanel?.Hide();
                cPanel?.Dispose();

                cPanel          = BuildModulePanel(new Point(748, baseSettingsPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN));
                cPanel.Location = new Point(baseSettingsPanel.Width - 720 - 10 - 20, 50);
                cPanel.Parent   = baseSettingsPanel;
            };

            //var settingsMenu = new Menu() {
            //    Size = new Point(256, 32 * 8),
            //    MenuItemHeight = 32,
            //    Location = new Point(20, 20),
            //    Parent = ssPanel,
            //};

            ////var settingsMi_BlishHud = settingsMenu.AddMenuItem("Blish HUD Settings");
            ////var settingsMi_Api = settingsMenu.AddMenuItem("API Settings");
            //var settingsMi_Module = settingsMenu.AddMenuItem("Module Settings");
            ////var setItemHotkeys = settingsMenu.AddMenuItem("Hotkeys");
            ////var settingsMi_Integration = settingsMenu.AddMenuItem("Integration Settings");
            ////var settingsMi_Update = settingsMenu.AddMenuItem("Update Settings");
            //var aboutMi = settingsMenu.AddMenuItem("About");
            //var exitMi = settingsMenu.AddMenuItem("Exit");

            //settingsMi_Module.LeftMouseButtonReleased += (object sender, MouseEventArgs e) => { wndw.Navigate(BuildModulePanel(wndw)); };
            ////setItemHotkeys.OnLeftMouseButtonReleased += (object sender, MouseEventArgs e) => { wndw.Navigate(GameServices.GetService<HotkeysService>().BuildHotkeysPanel(wndw)); };
            //aboutMi.LeftMouseButtonReleased += (object sender, MouseEventArgs e) => { wndw.Navigate(BuildAboutPanel(wndw)); };

            //// TODO: Add an "are you sure?" prompt
            //exitMi.LeftMouseButtonReleased += (object sender, MouseEventArgs e) => { Overlay.Exit(); };

            return baseSettingsPanel;
        }
        private Panel BuildApiPanel(Point size)
        {
            List<string> ApiKeys = this.CoreSettings.GetSetting<List<string>>(ApiService.SETTINGS_ENTRY).Value;
            Dictionary<string, string> FoolSafeKeyRepository = ApiService.GetKeyRepository();
            var apiPanel = new Panel() { CanScroll = false, Size = size };

            var keySelectionDropdown = new Dropdown()
            {
                Parent = apiPanel,
                Size = new Point(200, 30),
                Location = new Point(apiPanel.Size.X - 200 - Panel.RIGHT_MARGIN, Panel.TOP_MARGIN),
                Visible = FoolSafeKeyRepository.Count > 0,
                SelectedItem = FoolSafeKeyRepository.FirstOrDefault().Key
            };
            foreach (KeyValuePair<string,string> item in FoolSafeKeyRepository)
            {
                keySelectionDropdown.Items.Add(item.Key);
            }
            var connectedLabel = new Label()
            {
                Parent = apiPanel,
                Size = new Point(apiPanel.Size.X, 30),
                Location = new Point(0, apiPanel.Size.Y - Panel.BOTTOM_MARGIN - 15),
                ShowShadow = true,
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Center,
                Text = Gw2Api.Connected ? "OK! Connected. :-)" : "Not connected.",
                TextColor = Gw2Api.Connected ? Color.LightGreen : Color.Red
            };
            var apiKeyLabel = new Label()
            {
                Parent = apiPanel,
                Size = new Point(apiPanel.Size.X, 30),
                Location = new Point(0, apiPanel.Size.Y / 2 - apiPanel.Size.Y / 4 - 15),
                ShowShadow = true,
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Center,
                Text = "Insert your Guild Wars 2 API key here to unlock lots of cool features:"
            };
            var apiKeyTextBox = new TextBox() {
                Parent = apiPanel,
                Size = new Point(600, 30),
                Location = new Point(apiPanel.Size.X / 2 - 300, apiKeyLabel.Bottom),
                PlaceholderText = ApiService.PLACEHOLDER_KEY
            };
            var apiKeyError = new Label()
            {
                Parent = apiPanel,
                Size = new Point(apiPanel.Size.X, 30),
                Location = new Point(0, apiKeyTextBox.Bottom + Panel.BOTTOM_MARGIN),
                ShowShadow = true,
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Center,
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
                Visible = false
            };
            apiKeyButton.LeftMouseButtonPressed += delegate
            {
                ApiService.RemoveKey(FoolSafeKeyRepository[keySelectionDropdown.SelectedItem]);
                FoolSafeKeyRepository.Remove(FoolSafeKeyRepository[keySelectionDropdown.SelectedItem]);
                keySelectionDropdown.SelectedItem = FoolSafeKeyRepository.FirstOrDefault().Key;

                apiKeyTextBox.PlaceholderText =
                    FoolSafeKeyRepository.FirstOrDefault().Value + ApiService.PLACEHOLDER_KEY.Substring(FoolSafeKeyRepository.FirstOrDefault().Value.Length - 1);

                keySelectionDropdown.Visible = FoolSafeKeyRepository.Count > 0;
                apiKeyError.Visible = false;
                apiKeyButton.Visible = keySelectionDropdown.Visible;
                apiPanel.Invalidate();
            };
            apiKeyTextBox.OnEnterPressed += delegate
            {
                string apiKey = apiKeyTextBox.Text;
                string errorMsg = null;
                if (ApiKeys.Contains(apiKey))
                {
                    errorMsg = "API key already registered!";
                }
                else if (!Gw2Api.HasPermissions(new[]
                      {
                        Gw2Sharp.WebApi.V2.Models.TokenPermission.Account,
                        Gw2Sharp.WebApi.V2.Models.TokenPermission.Characters
                    },
                  apiKey))
                {
                    errorMsg = "Insufficient permissions! Required: Account, Characters.";
                }
                if (errorMsg != null) {
                    apiKeyError.Visible = true;
                    apiKeyError.Text = errorMsg;
                    apiKeyTextBox.Text = "";
                } else {
                    apiKeyError.Visible = true;
                    apiKeyTextBox.PlaceholderText = apiKeyTextBox.Text;

                    this.CoreSettings.GetSetting<List<string>>(ApiService.SETTINGS_ENTRY).Value.Add(apiKeyTextBox.Text);
                    apiKeyError.Text = "Success! Key added. :-)";
                    apiKeyError.TextColor = Color.LightGreen;
                    apiKeyTextBox.Text = "";
                    Gw2Api.UpdateCharacters(apiKey);
                }
                // Reset focus.
                Overlay.Form.ActiveControl = null;
                GameIntegration.FocusGw2();
                Overlay.Form.Focus();
                apiPanel.Invalidate();
            };
            keySelectionDropdown.ValueChanged += delegate {

                apiKeyTextBox.PlaceholderText = 
                keySelectionDropdown.SelectedItem != null ? 
                    FoolSafeKeyRepository[keySelectionDropdown.SelectedItem] + 
                    ApiService.PLACEHOLDER_KEY.Substring(FoolSafeKeyRepository.FirstOrDefault().Value.Length - 1) 
                    : ApiService.PLACEHOLDER_KEY;

                apiKeyButton.Visible = keySelectionDropdown.SelectedItem != null;
                apiPanel.Parent.UpdateContainer();
            };
            return apiPanel;
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
                HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
                Parent              = asPanel
            };

            var copyrightNotice2 = new Label() {
                Text = cleanNotice.Split('|')[1],
                Location            = new Point(0, copyrightNotice1.Bottom + 5),
                AutoSizeHeight      = true,
                Width = size.X,
                HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
                Parent              = asPanel
            };

            var copyrightNotice3 = new Label() {
                Text = cleanNotice.Split('|')[2],
                Location            = new Point(0, copyrightNotice2.Bottom + 5),
                AutoSizeHeight      = true,
                Width = size.X,
                HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
                Parent              = asPanel
            };
            
            var copyrightNotice4 = new Label() {
                Text = cleanNotice.Split('|')[3],
                Location            = new Point(0, copyrightNotice3.Bottom + 5),
                AutoSizeHeight      = true,
                Width = size.X,
                HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
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
            //                VerticalAlignment = DrawUtil.VerticalAlignment.Middle,
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

        #region Module Settings UI

        // TODO: Cleanup where this is stored
        private List<Control> LstSettings = new List<Control>();
        private Panel BuildModulePanel(Point destinationSize) {
            var mPanel = new Panel() {
                Size      = destinationSize,
                CanScroll = true
            };

            //var backButton = new BackButton(wndw) {
            //    Text = "Settings",
            //    NavTitle = "Modules",
            //    Parent = mPanel,
            //    Location = new Point(20, 20),
            //};

            var moduleSelectLbl = new Label() {
                Text           = "Module",
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = mPanel,
                Location       = new Point(20, 25),
            };

            var moduleDropdown = new Dropdown() {
                Parent = mPanel,
                Location = new Point(moduleSelectLbl.Right + 5, moduleSelectLbl.Top),
                Width = 350,
            };

            moduleDropdown.Top -= moduleSelectLbl.Height - moduleDropdown.Height / 2;

            // Display module information
            var lblModuleName = new Label() {
                Text = "Module Name: ",
                Parent = mPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(moduleSelectLbl.Left + 15, moduleSelectLbl.Bottom + 15),
            };
            var lblModuleAuthor = new Label() {
                Text = "Author: ",
                Parent = mPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(lblModuleName.Left, lblModuleName.Bottom + 3),
            };
            var lblModuleVersion = new Label() {
                Text = "Version: ",
                Parent = mPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(lblModuleAuthor.Left, lblModuleAuthor.Bottom + 3),
            };
            var lblModuleDescription = new Label() {
                Text = "Description: ",
                Parent = mPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(lblModuleVersion.Left, lblModuleVersion.Bottom + 3),
            };
            var cbModuleEnabled = new Checkbox() {
                Text = "Module Enabled",
                Parent = mPanel,
                Location = new Point(moduleDropdown.Right + 10, moduleDropdown.Top)
            };
            var lblModuleNamespace = new Label() {
                Text = "Module Namespace: ",
                Parent = mPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };

            cbModuleEnabled.Top += -cbModuleEnabled.Height / 2 + moduleDropdown.Height / 2;

            lblModuleNamespace.Left = lblModuleDescription.Left;
            lblModuleNamespace.Bottom = mPanel.Height - 5;
            
            // Wire events
            // TODO: This should likely just have its value bound to the ModuleState setting (and the setting should be bound to the module's "Enabled" property)
            cbModuleEnabled.CheckedChanged += delegate {
                var selectedModule =
                    GameService.Module.AvailableModules.First(m => m.GetModuleInfo().Name == moduleDropdown.SelectedItem);

                selectedModule.Enabled = cbModuleEnabled.Checked;
                GameService.Module.ModuleStates.Value[selectedModule.GetModuleInfo().Namespace] =
                    cbModuleEnabled.Checked;

                LstSettings.ForEach(s => s.Enabled = cbModuleEnabled.Checked);

                // Save since module states aren't currently handled by an observed property type
                Save();
            };


            // TODO: Calculate this instead of specifying this statically (or better yet, modify labels to have wordwrap support)
            int lineLength = 115;
            moduleDropdown.ValueChanged += (Object sender, Dropdown.ValueChangedEventArgs e) => {
                var selectedModule =
                    Module.AvailableModules.First(m => m.GetModuleInfo().Name == moduleDropdown.SelectedItem);

                if (selectedModule != null) {
                    // Populate module info labels
                    lblModuleName.Text = $"Module Name: {Utils.String.SplitText(selectedModule.GetModuleInfo().Name, lineLength)}";
                    lblModuleAuthor.Text = $"Author: {Utils.String.SplitText(selectedModule.GetModuleInfo().Author, lineLength)}";
                    lblModuleVersion.Text = $"Version: {Utils.String.SplitText(selectedModule.GetModuleInfo().Version, lineLength)}";
                    lblModuleDescription.Text = $"Description: {Utils.String.SplitText(selectedModule.GetModuleInfo().Description, lineLength)}";
                    lblModuleNamespace.Text = $"Module Namespace: {Utils.String.SplitText(selectedModule.GetModuleInfo().Namespace, lineLength)}";

                    cbModuleEnabled.Checked = selectedModule.Enabled;

                    // Clear out old setting controls
                    LstSettings.ToList().ForEach(s => s.Dispose());
                    LstSettings.Clear();

                    int lastControlBottom = lblModuleDescription.Bottom + 50;

                    // Display settings registered by module
                    foreach (KeyValuePair<string, SettingEntry> setting in selectedModule.Settings.Entries.Where(setting => setting.Value.ExposedAsSetting)) {
                        Control settingCtrl;

                        if (SettingTypeRenderers.ContainsKey(setting.Value.SettingType)) {
                            settingCtrl = SettingTypeRenderers[setting.Value.SettingType]
                               .Invoke(setting.Key, setting.Value);
                            settingCtrl.Parent  = mPanel;
                            settingCtrl.Enabled = cbModuleEnabled.Checked;

                            settingCtrl.Location = new Point(lblModuleDescription.Left, lastControlBottom + 10);

                            lastControlBottom = settingCtrl.Bottom;
                        } else {
                            // This setting type is not supported for automatic display
                            // Write this out to the log so that devs can see
                            // TODO: This needs to use the debug service to output it correctly
                            Console.WriteLine($"Module `{selectedModule.GetModuleInfo().Name}` has a setting `{setting.Key}` of type `{setting.Value.SettingType.ToString()}` which the Settings Service does not currently support the automatic exposure of.");
                            settingCtrl = null;
                        }

                        if (settingCtrl != null)
                            LstSettings.Add(settingCtrl);
                    }
                }
            };

            //backButton.LeftMouseButtonReleased += (object sender, MouseEventArgs e) => { wndw.NavigateHome(); };

            // Populate data
            foreach (var module in Module.AvailableModules) {
                moduleDropdown.Items.Add(module.GetModuleInfo().Name);
            }

            return mPanel;
        }

        #endregion

    }
}
