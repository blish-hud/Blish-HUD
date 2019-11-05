using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Contexts;
using Blish_HUD.Controls;
using Blish_HUD.Overlay.UI.Presenters;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Overlay.UI.Views.Widgets;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class OverlayService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<OverlayService>();

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
        }

        private void DefineSettings(SettingCollection settings) {
            _userLocale    = settings.DefineSetting("AppCulture",    GetGw2LocaleFromCurrentUICulture(), Strings.GameServices.OverlayService.Setting_AppCulture_DisplayName,    Strings.GameServices.OverlayService.Setting_AppCulture_Description);
            _stayInTray    = settings.DefineSetting("StayInTray",    true,                               Strings.GameServices.OverlayService.Setting_StayInTray_DisplayName,    Strings.GameServices.OverlayService.Setting_StayInTray_Description);
            _showInTaskbar = settings.DefineSetting("ShowInTaskbar", false,                              Strings.GameServices.OverlayService.Setting_ShowInTaskbar_DisplayName, Strings.GameServices.OverlayService.Setting_ShowInTaskbar_Description);

            _showInTaskbar.SettingChanged += ShowInTaskbarOnSettingChanged;
            _userLocale.SettingChanged    += UserLocaleOnSettingChanged;

            ApplyInitialSettings();
        }

        private void ApplyInitialSettings() {
            ShowInTaskbarOnSettingChanged(_showInTaskbar, new ValueChangedEventArgs<bool>(true, _showInTaskbar.Value));
            UserLocaleOnSettingChanged(_userLocale, new ValueChangedEventArgs<Gw2Locale>(GetGw2LocaleFromCurrentUICulture(), _userLocale.Value));
        }

        private void ShowInTaskbarOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            WindowUtil.SetShowInTaskbar(BlishHud.FormHandle, e.NewValue);
        }

        private void UserLocaleOnSettingChanged(object sender, ValueChangedEventArgs<Gw2Locale> e) {
            CultureInfo.CurrentUICulture = GetCultureFromGw2Locale(e.NewValue);
        }

        private CultureInfo GetCultureFromGw2Locale(Gw2Locale gw2Locale) {
            switch (gw2Locale) {
                case Gw2Locale.German:
                    return CultureInfo.GetCultureInfo(7); // German (de-DE)
                    break;

                case Gw2Locale.English:
                    return CultureInfo.GetCultureInfo(9); // English (en-US)
                    break;

                case Gw2Locale.Spanish:
                    return CultureInfo.GetCultureInfo(10); // Spanish (es-ES)
                    break;

                case Gw2Locale.French:
                    return CultureInfo.GetCultureInfo(12); // French (fr-FR)
                    break;

                case Gw2Locale.Korean:
                    return CultureInfo.GetCultureInfo(18); // Korean (ko-KR)
                    break;

                case Gw2Locale.Chinese:
                    return CultureInfo.GetCultureInfo(30724); // Chinese (zh-CN)
                    break;
            }

            return CultureInfo.GetCultureInfo(9); // English (en-US)
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
            this.BlishMenuIcon = new CornerIcon(Content.GetTexture("logo"), Content.GetTexture("logo-big"), Strings.Common.BlishHUD) {
                Menu     = new ContextMenuStrip(),
                Priority = int.MaxValue,
                Parent   = Graphics.SpriteScreen,
            };

            this.BlishContextMenu = this.BlishMenuIcon.Menu;
            this.BlishContextMenu.AddMenuItem($"Restart {Strings.Common.BlishHUD}").Click += delegate { System.Windows.Forms.Application.Restart(); Environment.Exit(0); };
            this.BlishContextMenu.AddMenuItem($"{Strings.Common.Action_Exit} {Strings.Common.BlishHUD}").Click += delegate { ActiveBlishHud.Exit(); };

            this.BlishHudWindow = new TabbedWindow() {
                Parent = Graphics.SpriteScreen,
                Title  = Strings.Common.BlishHUD,
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

            this.BlishHudWindow.AddTab(Strings.GameServices.OverlayService.HomeTab, Content.GetTexture("255369"), BuildHomePanel(this.BlishHudWindow), int.MinValue);

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

        private Panel BuildHomePanel(WindowBase window) {
            var hPanel = new ViewContainer() {
                Size = window.ContentRegion.Size
            };

            var homeWidgets = new List<IHomeWidgetView> {
                new RssWidgetView("Game Release Notes",     @"https://en-forum.guildwars2.com/categories/game-release-notes/feed.rss"),
                new RssWidgetView("News and Announcements", @"https://en-forum.guildwars2.com/categories/news-and-announcements/feed.rss"),
                new RssWidgetView("Community News",         @"https://www.guildwars2.com/en/feed/")
            };

            var homeWidgetsView = new RepeatedView<IEnumerable<IHomeWidgetView>>() {
                FlowDirection = ControlFlowDirection.LeftToRight
            };

            homeWidgetsView.Presenter = new HomeWidgetPresenter(homeWidgetsView, homeWidgets);

            hPanel.Show(homeWidgetsView);

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
