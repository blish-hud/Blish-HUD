using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Blish_HUD.GameServices;
using Blish_HUD.Properties;

namespace Blish_HUD.GameIntegration {
    public class WinFormsIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<WinFormsIntegration>();

        private Form _formWrapper;

        private NotifyIcon _trayIcon;

        private ToolStripItem _launchGw2Tsi;
        private ToolStripItem _launchGw2AutoTsi;
        private ToolStripItem _openBlishSettingsFolder;
        private ToolStripItem _exitTsi;

        /// <summary>
        /// The menu displayed when the tray icon is right-clicked.
        /// </summary>
        public ContextMenuStrip TrayIconMenu { get; private set; }

        public WinFormsIntegration(GameIntegrationService service) : base(service) { /* NOOP */ }

        public override void Load() {
            WrapMainForm();
            BuildTrayIcon();
            AutoLaunchGame();
        }

        private void WrapMainForm() {
            _formWrapper = new Form();
            BlishHud.Instance.Form.Hide();
            BlishHud.Instance.Form.Show(_formWrapper);
            BlishHud.Instance.Form.Visible = false;
        }

        internal void SetShowInTaskbar(bool showInTaskbar) {
            WindowUtil.SetShowInTaskbar(BlishHud.Instance.FormHandle, showInTaskbar);
        }

        private void BuildTrayIcon() {
            string trayIconText = Strings.Common.BlishHUD;

            if (ApplicationSettings.Instance.MumbleMapName != null) {
                trayIconText += $" ({ApplicationSettings.Instance.MumbleMapName})";
            }

            this.TrayIconMenu = new ContextMenuStrip();
            
            _trayIcon = new NotifyIcon() {
                Icon             = Resources.Ico2039771,
                Text             = trayIconText,
                Visible          = true,
                ContextMenuStrip = this.TrayIconMenu
            };

            // Populate TrayIconMenu items
            _launchGw2AutoTsi = this.TrayIconMenu.Items.Add($"{Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2} - {Strings.GameServices.GameIntegrationService.TrayIcon_Autologin}");
            _launchGw2Tsi = this.TrayIconMenu.Items.Add(Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2);

            _launchGw2AutoTsi.Click += delegate { LaunchGw2(true); };
            _launchGw2Tsi.Click     += delegate { LaunchGw2(false); };

            _trayIcon.DoubleClick += delegate {
                if (!_service.Gw2Instance.Gw2IsRunning) {
                    LaunchGw2(true);
                }
            };

            this.TrayIconMenu.Items.Add(new ToolStripSeparator());

            _openBlishSettingsFolder = this.TrayIconMenu.Items.Add(Strings.GameServices.GameIntegrationService.TrayIcon_OpenSettingsFolder);

            _openBlishSettingsFolder.Click += delegate {
                Process.Start(DirectoryUtil.BasePath);
            };

            this.TrayIconMenu.Items.Add(new ToolStripSeparator());
            _exitTsi = this.TrayIconMenu.Items.Add(string.Format(Strings.Common.Action_Exit,  Strings.Common.BlishHUD));

            _exitTsi.Click += delegate { GameService.Overlay.Exit(); };

            this.TrayIconMenu.Opening += TrayIconMenuOnOpening;
        }

        private void TrayIconMenuOnOpening(object sender, CancelEventArgs e) {
            _launchGw2Tsi.Enabled = _launchGw2AutoTsi.Enabled = !_service.Gw2Instance.Gw2IsRunning
                                                             && File.Exists(_service.Gw2Instance.Gw2ExecutablePath);

            _launchGw2AutoTsi.Visible = !_service.Gw2Instance.IsSteamVersion;
        }

        private void AutoLaunchGame() {
            if (ApplicationSettings.Instance.StartGw2 > 0) {
                LaunchGw2(ApplicationSettings.Instance.StartGw2 > 1);
            }
        }

        private void LaunchGw2(bool autologin = false) {
            var args = new List<string>();

            // Auto login
            if (autologin && !_service.Gw2Instance.IsSteamVersion) {
                // FYI: Steam doesn't do anything with autologin
                args.Add("-autologin");
            }

            // Mumble target name
            if (ApplicationSettings.Instance.MumbleMapName != null) {
                args.Add($"-mumble \"{ApplicationSettings.Instance.MumbleMapName}\"");
            }

            if (_service.Gw2Instance.IsSteamVersion) {
                Process.Start("steam://run/1284210//" + string.Join(" ", args));
                return;
            }

            if (File.Exists(_service.Gw2Instance.Gw2ExecutablePath)) {
                var gw2Proc = new Process {
                    StartInfo = {
                        FileName  = _service.Gw2Instance.Gw2ExecutablePath,
                        Arguments = string.Join(" ", args)
                    }
                };

                Logger.Info("Blish HUD starting Guild Wars 2 with args '{gw2Args}'", string.Join(" ", args));

                gw2Proc.Start();
            } else {
                Logger.Warn("Blish HUD failed to launch Guild Wars 2.  The path we have is null or does not exist.  Try again after the game has been successfully detected by Blish HUD.");
            }
        }

        public override void Unload() {
            if (_trayIcon != null) {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }

    }
}
