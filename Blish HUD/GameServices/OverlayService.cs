﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Blish_HUD.Contexts;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD {

    public class OverlayService : GameService {

        private static readonly Logger Logger = Logger.GetLogger(typeof(OverlayService));

        private const string APPLICATION_SETTINGS = "OverlayConfiguration";

        public event EventHandler<EventArgs> UserLocaleChanged;

        public TabbedWindow BlishHudWindow { get; protected set; }
        public CornerIcon BlishMenuIcon { get; protected set; }
        public ContextMenuStrip BlishContextMenu { get; protected set; }

        public GameTime CurrentGameTime { get; private set; }

        internal SettingCollection       _applicationSettings;
        private  SettingEntry<Gw2Locale> _userLocale;
        private  SettingEntry<bool>      _stayInTray;
        private  SettingEntry<bool>      _showInTaskbar;

        public Gw2Locale UserLocale    => _userLocale.Value;
        public bool      StayInTray    => _stayInTray.Value;
        public bool      ShowInTaskbar => _showInTaskbar.Value;


        private bool                        _checkedClient;
        private Gw2ClientContext.ClientType _clientType;

        private readonly ConcurrentQueue<Action<GameTime>> _queuedUpdates = new ConcurrentQueue<Action<GameTime>>();

        /// <summary>
        /// Allows you to enqueue a call that will occur during the next time the update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see cref="GameTime" /> as a parameter.</param>
        public void QueueMainThreadUpdate(Action<GameTime> call) {
            _queuedUpdates.Enqueue(call);
        }

        protected override void Initialize() {
            _applicationSettings = Settings.RegisterRootSettingCollection(APPLICATION_SETTINGS);

            DefineSettings(_applicationSettings);

            ApplySettings();
        }

        private void DefineSettings(SettingCollection settings) {
            _userLocale    = settings.DefineSetting("AppCulture",    GetGw2LocaleFromCurrentUICulture(), "Application & API Language",                "Determines the language used when displaying Blish HUD text and when requests are made to the GW2 web API.");
            _stayInTray    = settings.DefineSetting("StayInTray",    true,                               "Minimize to tray when Guild Wars 2 Closes", "If true, Blish HUD will automatically minimize when GW2 closes and will continue running until GW2 is launched again.\nYou can also use the Blish HUD icon in the tray to launch Guild Wars 2.");
            _showInTaskbar = settings.DefineSetting("ShowInTaskbar", false,                              "Show in Taskbar",                           "When enabled, Blish HUD will be shown in the taskbar.");

            _showInTaskbar.SettingChanged += OverlaySettingChanged;
        }

        private void ApplySettings() {
            WindowUtil.SetShowInTaskbar(BlishHud.FormHandle, _showInTaskbar.Value);
        }

        private void OverlaySettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            ApplySettings();
        }

        private Gw2Locale GetGw2LocaleFromCurrentUICulture() {
            string currLocale = CultureInfo.CurrentUICulture.EnglishName.Split(' ')[0];

            switch (currLocale) {
                case "Chinese":
                    return Gw2Locale.Chinese;
                case "French":
                    return Gw2Locale.French;
                case "German":
                    return Gw2Locale.German;
                case "Korean":
                    return Gw2Locale.Korean;
                case "Spanish":
                    return Gw2Locale.Spanish;
                case "English":
                default:
                    return Gw2Locale.English;
            }
        }

        protected override void Load() {
            this.BlishMenuIcon = new CornerIcon(Content.GetTexture("logo"), Content.GetTexture("logo-big"), Properties.Strings.General_BlishHUD) {
                Menu     = new ContextMenuStrip(),
                Priority = int.MaxValue,
                Parent   = Graphics.SpriteScreen,
            };

            this.BlishContextMenu = this.BlishMenuIcon.Menu;
            this.BlishContextMenu.AddMenuItem($"{Properties.Strings.General_Close} {Properties.Strings.General_BlishHUD}").Click += delegate { ActiveBlishHud.Exit(); };

            this.BlishHudWindow = new TabbedWindow() {
                Parent = Graphics.SpriteScreen,
                Title  = Properties.Strings.General_BlishHUD,
                Emblem = Content.GetTexture("test-window-icon9")
            };

            this.BlishMenuIcon.LeftMouseButtonReleased += delegate {
                this.BlishHudWindow.ToggleWindow();
            };

            // Center the window so that you don't have to drag it over every single time (which is really annoying)
            // TODO: Save window positions to settings so that they remember where they were last
            Graphics.SpriteScreen.Resized += delegate {
                if (!this.BlishHudWindow.Visible) {
                    this.BlishHudWindow.Location = new Point(Graphics.WindowWidth / 2 - this.BlishHudWindow.Width / 2, Graphics.WindowHeight / 2 - this.BlishHudWindow.Height / 2);
                }
            };

            this.BlishHudWindow.AddTab(Properties.Strings.Service_DirectorService_Tab_Home, Content.GetTexture("255369"), BuildHomePanel(this.BlishHudWindow), int.MinValue);

            PrepareClientDetection();
        }

        private void PrepareClientDetection() {
            GameService.GameIntegration.Gw2Closed += GameIntegrationOnGw2Closed;
            GameService.Gw2Mumble.BuildIdChanged  += Gw2MumbleOnBuildIdChanged;

            GameService.Contexts.GetContext<CdnInfoContext>().StateChanged += CdnInfoContextOnStateChanged;
        }

        private void Gw2MumbleOnBuildIdChanged(object sender, EventArgs e) {
            if (!_checkedClient) {
                DetectClientType();
            }
        }

        private void CdnInfoContextOnStateChanged(object sender, EventArgs e) {
            if (!_checkedClient && ((Context) sender).State == ContextState.Ready) {
                DetectClientType();
            }
        }

        private void GameIntegrationOnGw2Closed(object sender, EventArgs e) {
            _checkedClient = false;
        }

        private void DetectClientType() {
            if (GameService.Contexts.GetContext<Gw2ClientContext>().TryGetClientType(out var contextResult) == ContextAvailability.Available) {
                _clientType = contextResult.Value;

                if (_clientType == Gw2ClientContext.ClientType.Unknown) {
                    Logger.Warn("Failed to detect current Guild Wars 2 client version: {statusForUnknown}.", contextResult.Status);
                } else {
                    Logger.Info("Detected Guild Wars 2 client to be the {clientVersionType} version.", _clientType);
                }

                _checkedClient = true;
            } else {
                Logger.Warn("Failed to detect current Guild Wars 2 client version: {statusForUnknown}", contextResult.Status);
            }
        }

        private Panel BuildHomePanel(WindowBase wndw) {
            var hPanel = new Panel() {
                Size = wndw.ContentRegion.Size
            };

            var colPanel = new Panel() {
                Size     = new Point(450, 256),
                Location = new Point(24, 24),
                Parent = hPanel,
                Title = " ",
                ShowBorder = true,
                CanCollapse = true
            };

            var testLabel = new Label() {
                Text           = "This is a test label!",
                Parent         = colPanel,
                Location       = colPanel.Size - new Point(colPanel.Width / 2 - 50, colPanel.Height / 2 - 10),
                AutoSizeWidth  = true,
                AutoSizeHeight = true
            };

            //bttn7.Click += async delegate {
            //    //File.Move("Blish HUD.exe", "Blish HUD.exe.temp");

            //    var upgradeCheck = new Octokit.GitHubClient(new ProductHeaderValue("BlishHUD", Program.OverlayVersion.ToString()));
            //    var dir = await upgradeCheck.Repository.Content.GetAllContentsByRef("blish-hud", "Versions", @"/Blish-HUD/", "master");

            //    foreach (var d in dir) {
            //        if (d.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) {
            //            Console.WriteLine(JsonConvert.SerializeObject(d));
            //        }
            //    }
            //};

            return hPanel;
        }

        protected override void Unload() {
            this.BlishMenuIcon.Dispose();
            this.BlishHudWindow.Dispose();
        }

        private double _lastTacoCheckTime = 5;

        private void HandleEnqueuedUpdates(GameTime gameTime) {
            while (_queuedUpdates.TryDequeue(out Action<GameTime> updateCall)) {
                updateCall.Invoke(gameTime);
            }
        }

        protected override void Update(GameTime gameTime) {
            this.CurrentGameTime = gameTime;

            HandleEnqueuedUpdates(gameTime);

            if (GameService.GameIntegration.IsInGame) {
                _lastTacoCheckTime += gameTime.ElapsedGameTime.TotalSeconds;

                // TODO: Move some of this into the TacO related module
                if (_lastTacoCheckTime > 3) {
                    Process[] tacoApp = Process.GetProcessesByName("GW2TacO");

                    if (tacoApp.Length > 0) {
                        CornerIcon.LeftOffset = 36 * (_clientType == Gw2ClientContext.ClientType.Chinese ? 2 : 1);
                    } else {
                        CornerIcon.LeftOffset = _clientType == Gw2ClientContext.ClientType.Chinese ? 36 : 0;
                    }

                    _lastTacoCheckTime = 0;
                }
            }
        }

    }
}
