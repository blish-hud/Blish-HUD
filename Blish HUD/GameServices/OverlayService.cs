using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Blish_HUD.Contexts;
using Blish_HUD.Controls;
using Blish_HUD.Overlay;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using ContextMenuStrip = Blish_HUD.Controls.ContextMenuStrip;
using MenuItem = Blish_HUD.Controls.MenuItem;

namespace Blish_HUD {

    public class OverlayService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<OverlayService>();

        private const string APPLICATION_SETTINGS = "OverlayConfiguration";

        internal const int FORCE_EXIT_TIMEOUT = 4000;

        public event EventHandler<ValueEventArgs<CultureInfo>> UserLocaleChanged;

        public TabbedWindow     BlishHudWindow   { get; private set; }
        public CornerIcon       BlishMenuIcon    { get; private set; }
        public ContextMenuStrip BlishContextMenu { get; private set; }

        public GameTime CurrentGameTime { get; private set; }

        internal SettingCollection OverlaySettings { get; private set; }

        public SettingEntry<Locale> UserLocale    { get; private set; }
        public SettingEntry<bool>   StayInTray    { get; private set; }
        public SettingEntry<bool>   ShowInTaskbar { get; private set; }

        private readonly ConcurrentQueue<Action<GameTime>> _queuedUpdates = new ConcurrentQueue<Action<GameTime>>();

        public OverlaySettingsTab SettingsTab { get; private set; }

        /// <summary>
        /// Allows you to enqueue a call that will occur during the next time the update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see cref="GameTime" /> as a parameter.</param>
        public void QueueMainThreadUpdate(Action<GameTime> call) {
            _queuedUpdates.Enqueue(call);
        }

        protected override void Initialize() {
            this.OverlaySettings = Settings.RegisterRootSettingCollection(APPLICATION_SETTINGS);

            DefineSettings(this.OverlaySettings);

            PrepareSettingsTab();
        }

        private void PrepareSettingsTab() {
            this.SettingsTab = new OverlaySettingsTab(this);

            // About
            this.SettingsTab.RegisterSettingMenu(new MenuItem(Strings.GameServices.OverlayService.AboutSection, GameService.Content.GetTexture("440023")),
                                                 (m) => new AboutView(),
                                                 int.MinValue);

            // Overlay Settings
            this.SettingsTab.RegisterSettingMenu(new MenuItem(Strings.GameServices.OverlayService.OverlaySettingsSection, GameService.Content.GetTexture("156736")),
                                                 (m) => new OverlaySettingsView(),
                                                 int.MaxValue - 12);
        }

        private void DefineSettings(SettingCollection settings) {
            this.UserLocale    = settings.DefineSetting("AppCulture",    GetGw2LocaleFromCurrentUICulture(), Strings.GameServices.OverlayService.Setting_AppCulture_DisplayName,    Strings.GameServices.OverlayService.Setting_AppCulture_Description);
            this.StayInTray    = settings.DefineSetting("StayInTray",    true,                               Strings.GameServices.OverlayService.Setting_StayInTray_DisplayName,    Strings.GameServices.OverlayService.Setting_StayInTray_Description);
            this.ShowInTaskbar = settings.DefineSetting("ShowInTaskbar", false,                              Strings.GameServices.OverlayService.Setting_ShowInTaskbar_DisplayName, Strings.GameServices.OverlayService.Setting_ShowInTaskbar_Description);

            // TODO: See https://github.com/blish-hud/Blish-HUD/issues/282
            this.UserLocale.SetExcluded(Locale.Chinese);

            this.ShowInTaskbar.SettingChanged += ShowInTaskbarOnSettingChanged;
            this.UserLocale.SettingChanged    += UserLocaleOnSettingChanged;

            ApplyInitialSettings();
        }

        private void ApplyInitialSettings() {
            UserLocaleOnSettingChanged(this.UserLocale, new ValueChangedEventArgs<Locale>(GetGw2LocaleFromCurrentUICulture(), this.UserLocale.Value));
        }

        private void ShowInTaskbarOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            GameIntegration.WinForms.SetShowInTaskbar(e.NewValue);
        }

        private void UserLocaleOnSettingChanged(object sender, ValueChangedEventArgs<Locale> e) {
            var culture = GetCultureFromGw2Locale(e.NewValue);

            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentUICulture              = culture;

            this.UserLocaleChanged?.Invoke(this, new ValueEventArgs<CultureInfo>(culture));
        }

        /// <summary>
        /// Instructs Blish HUD to unload and exit.
        /// </summary>
        public void Exit() {
            this.BeginExit(FORCE_EXIT_TIMEOUT);

            ActiveBlishHud.Exit();
        }

        private void BeginExit(int timeout) {
            GameService.Settings.Save(true);

            if (timeout > 0) {
                (new Thread(() => {
                                Thread.Sleep(timeout);
                                Logger.Warn($"Unload took too long (longer than {timeout} ms). Forcing exit.");
                                Environment.Exit(0);
                            }) {IsBackground = true}).Start();
            }
        } 
        
        /// <summary>
        /// Instructs Blish HUD to unload and then restart.
        /// </summary>
        public void Restart() {
            this.BeginExit(0);

            // REF: https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/Application.cs,1447

            var arguments = Environment.GetCommandLineArgs()
                                                      .Skip(1)
                                                      .Where(arg => !string.Equals(arg, $"--{ApplicationSettings.OPTION_RESTARTSKIPMUTEX}", StringComparison.OrdinalIgnoreCase))
                                                      .Append($"--{ApplicationSettings.OPTION_RESTARTSKIPMUTEX}")
                                                      .Select(arg => $"\"{arg}\"");

            var currentStartInfo = Process.GetCurrentProcess().StartInfo;
            currentStartInfo.FileName  = Application.ExecutablePath;
            currentStartInfo.Arguments = string.Join(" ", arguments);

            Process.Start(currentStartInfo);

            ActiveBlishHud.Exit();
        }

        private CultureInfo GetCultureFromGw2Locale(Locale locale) {
            switch (locale) {
                case Locale.German:
                    return CultureInfo.GetCultureInfo(7); // German (de-DE)
                case Locale.English:
                    return CultureInfo.GetCultureInfo(9); // English (en-US)
                case Locale.Spanish:
                    return CultureInfo.GetCultureInfo(10); // Spanish (es-ES)
                case Locale.French:
                    return CultureInfo.GetCultureInfo(12); // French (fr-FR)
                case Locale.Korean:
                    return CultureInfo.GetCultureInfo(18); // Korean (ko-KR)
                case Locale.Chinese:
                    return CultureInfo.GetCultureInfo(30724); // Chinese (zh-CN)
            }

            return CultureInfo.GetCultureInfo(9); // English (en-US)
        }

        private Locale GetGw2LocaleFromCurrentUICulture() {
            string currLocale = CultureInfo.CurrentUICulture.EnglishName.Split(' ')[0];

            switch (currLocale) {
                case "Chinese":
                    return Locale.Chinese;
                case "French":
                    return Locale.French;
                case "German":
                    return Locale.German;
                case "Korean":
                    return Locale.Korean;
                case "Spanish":
                    return Locale.Spanish;
                case "English":
                default:
                    return Locale.English;
            }
        }

        protected override void Load() {
            BuildMainWindow();
            BuildCornerIcon();
        }

        private void BuildMainWindow() {
            this.BlishHudWindow = new TabbedWindow() {
                Parent = Graphics.SpriteScreen,
                Title  = Strings.Common.BlishHUD,
                Emblem = Content.GetTexture("blishhud-emblem")
            };

            // Center the window so that you don't have to drag it over every single time (which is really annoying)
            // TODO: Save window positions to settings so that they remember where they were last
            Graphics.SpriteScreen.Resized += delegate {
                if (!this.BlishHudWindow.Visible) {
                    this.BlishHudWindow.Location = new Point(Graphics.WindowWidth / 2 - this.BlishHudWindow.Width / 2,
                                                             Graphics.WindowHeight / 2 - this.BlishHudWindow.Height / 2);
                }
            };

            BuildSettingTab();
        }

        private void BuildCornerIcon() {
            this.BlishMenuIcon = new CornerIcon(Content.GetTexture("logo"), Content.GetTexture("logo-big"), Strings.Common.BlishHUD) {
                Menu     = new ContextMenuStrip(),
                Priority = int.MaxValue,
                Parent   = Graphics.SpriteScreen,
            };

            this.BlishContextMenu                                                                                          =  this.BlishMenuIcon.Menu;
            this.BlishContextMenu.AddMenuItem(string.Format(Strings.Common.Action_Restart, Strings.Common.BlishHUD)).Click += delegate { Restart(); };
            this.BlishContextMenu.AddMenuItem(string.Format(Strings.Common.Action_Exit,    Strings.Common.BlishHUD)).Click += delegate { Exit(); };

            this.BlishMenuIcon.LeftMouseButtonReleased += delegate {
                this.BlishHudWindow.ToggleWindow();
            };
        }

        private void BuildSettingTab() {
            this.BlishHudWindow.AddTab(Strings.GameServices.SettingsService.SettingsTab,
                                       Content.GetTexture("155052"),
                                       () => new SettingsMenuView(this.SettingsTab),
                                       int.MaxValue - 10);
        }

        protected override void Unload() {
            this.BlishMenuIcon.Dispose();
            this.BlishHudWindow.Dispose();
        }

        private void HandleEnqueuedUpdates(GameTime gameTime) {
            while (_queuedUpdates.TryDequeue(out Action<GameTime> updateCall)) {
                updateCall.Invoke(gameTime);
            }
        }

        protected override void Update(GameTime gameTime) {
            this.CurrentGameTime = gameTime;

            HandleEnqueuedUpdates(gameTime);

            if (GameIntegration.IsInGame) {
                int offset = /* Offset +1 if Chinese client */ (GameService.GameIntegration.ClientType.ClientType == Gw2ClientContext.ClientType.Chinese ? 1 : 0)
                           + /* Offset +1 if running TacO   */ (GameIntegration.TacO.TacOIsRunning ? 1 : 0);

                CornerIcon.LeftOffset = offset * 36;
            }
        }

    }
}
