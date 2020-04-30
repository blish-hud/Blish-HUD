using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows.Forms;
using Blish_HUD.Settings;
using Microsoft.Win32;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class GameIntegrationService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<GameIntegrationService>();

        public event EventHandler<EventArgs> Gw2Closed;
        public event EventHandler<EventArgs> Gw2Started;

        public event EventHandler<EventArgs> Gw2AcquiredFocus;
        public event EventHandler<EventArgs> Gw2LostFocus;

        // How long, in seconds, between each
        // check to see if GW2 is running
        private const int GW2_EXE_CHECKRATE = 15;

        private const string GW2_REGISTRY_KEY     = @"SOFTWARE\ArenaNet\Guild Wars 2";
        private const string GW2_REGISTRY_PATH_SV = "Path";

        private const string GW2_64_BIT_PROCESSNAME = "Gw2-64";

        // TODO: Confirm this is actually what the 32-bit process is called
        private const string GW2_32_BIT_PROCESSNAME = "Gw2";

        private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
        private const string GW2_GAMEWINDOW_NAME  = "ArenaNet_Dx_Window_Class";

        private const string GAMEINTEGRATION_SETTINGS = "GameIntegrationConfiguration";

        public NotifyIcon TrayIcon { get; private set; }
        public ContextMenuStrip TrayIconMenu { get; private set; }

        public bool IsInGame { get; private set; } = false;

        private bool _gw2HasFocus = false;
        public bool Gw2HasFocus {
            get => _gw2HasFocus;
            private set {
                if (_gw2HasFocus == value) return;

                _gw2HasFocus = value;

                if (_gw2HasFocus) {
                    Gw2AcquiredFocus?.Invoke(this, EventArgs.Empty);
                } else {
                    Gw2LostFocus?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool _gw2IsRunning = false;
        public bool Gw2IsRunning {
            get => _gw2IsRunning;
            private set {
                if (_gw2IsRunning == value) return;

                _gw2IsRunning = value;

                if (_gw2IsRunning) {
                    this.Gw2Started?.Invoke(this, EventArgs.Empty);
                } else {
                    this.Gw2Closed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IntPtr Gw2WindowHandle { get; private set; }

        private Process _gw2Process;
        public Process Gw2Process {
            get => _gw2Process;
            set {
                if (_gw2Process == value) return;

                _gw2Process = value;

                if (value == null || _gw2Process.MainWindowHandle == IntPtr.Zero) {
                    BlishHud.Form.Invoke((MethodInvoker) (() => { BlishHud.Form.Visible = false; }));

                    _gw2Process = null;
                } else {
                    this.Gw2WindowHandle = _gw2Process.MainWindowHandle;

                    if (_gw2Process.MainModule != null) {
                        _gw2ExecutablePath.Value = _gw2Process.MainModule.FileName;
                    }
                }

                // GW2 is running if the "_gw2Process" isn't null and the class name of process' 
                // window is the game window name (so we know we are passed the login screen)
                this.Gw2IsRunning = _gw2Process != null
                                 && WindowUtil.GetClassNameOfWindow(this.Gw2Process.MainWindowHandle) == (ApplicationSettings.Instance.WindowName
                                                                                                       ?? GW2_GAMEWINDOW_NAME);
            }
        }

        private System.Windows.Forms.Form _formWrapper;

        private SettingCollection _gameIntegrationSettings;

        private SettingEntry<string> _gw2ExecutablePath;

        public string Gw2ExecutablePath => _gw2ExecutablePath.Value;

        protected override void Initialize() {
            _formWrapper = new Form();
            BlishHud.Form.Hide();
            BlishHud.Form.Show(_formWrapper);
            BlishHud.Form.Visible = false;

            _gameIntegrationSettings = Settings.RegisterRootSettingCollection(GAMEINTEGRATION_SETTINGS);

            DefineSettings(_gameIntegrationSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            const string UNDEFINED_EXECPATH = "NotDetected";

            _gw2ExecutablePath = settings.DefineSetting("Gw2ExecutablePath", UNDEFINED_EXECPATH, "Gw2-64.exe Path", "The path to the game's executable. This is auto-detected, so don't change this unless you know what you're doing.");

            // We do this to avoid trying to detect in the registry
            // unless we have never detected the true path
            if (_gw2ExecutablePath.Value == UNDEFINED_EXECPATH) {
                _gw2ExecutablePath.Value = GetGw2PathFromRegistry();
            }
        }

        private string GetGw2PathFromRegistry() {
            try {
                using (var gw2Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(GW2_REGISTRY_KEY, RegistryRights.ReadKey)) {
                    if (gw2Key != null) {
                        string gw2Path = gw2Key.GetValue(GW2_REGISTRY_PATH_SV).ToString();

                        if (File.Exists(gw2Path)) {
                            return gw2Path;
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to read Guild Wars 2 path from registry value {registryKey} located at {registryPath}.", GW2_REGISTRY_PATH_SV, GW2_REGISTRY_KEY);
            }

            return string.Empty;
        }

        private void LaunchGw2(bool autologin = false) {
            if (File.Exists(this.Gw2ExecutablePath)) {
                var gw2Proc = new Process {
                    StartInfo = {
                        FileName = this.Gw2ExecutablePath,
                        Arguments = autologin ? "-autologin" : ""
                    }
                };

                gw2Proc.Start();
            }
        }

        protected override void Load() {
            BlishHud.Form.Shown += delegate {
                WindowUtil.SetupOverlay(BlishHud.FormHandle);
            };

            CreateTrayIcon();

            TryAttachToGw2();

            Gw2Mumble.Info.IsGameFocusedChanged += OnGameAcquiredFocus;
        }

        private void OnGameAcquiredFocus(object sender, ValueEventArgs<bool> e) {
            if (e.Value) {
                this.Gw2Process = Process.GetProcessById((int) Gw2Mumble.Info.ProcessId);
            }
        }

        #region TrayIcon Menu Items

        private ToolStripItem ts_launchGw2;
        private ToolStripItem ts_launchGw2Auto;
        private ToolStripItem ts_exit;

        #endregion

        private void CreateTrayIcon() {
            this.TrayIconMenu = new ContextMenuStrip();

            // Found this here: https://stackoverflow.com/a/25409865/595437
            // Extract the tray icon from our assembly
            this.TrayIcon = new NotifyIcon() {
                Icon             = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text             = Strings.Common.BlishHUD,
                Visible          = true,
                ContextMenuStrip = this.TrayIconMenu
            };

            // Populate TrayIconMenu items
            ts_launchGw2Auto = this.TrayIconMenu.Items.Add($"{Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2} - {Strings.GameServices.GameIntegrationService.TrayIcon_Autologin}");
            ts_launchGw2     = this.TrayIconMenu.Items.Add(Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2);

            ts_launchGw2Auto.Click += delegate { LaunchGw2(true); };
            ts_launchGw2.Click     += delegate { LaunchGw2(false); };

            this.TrayIcon.DoubleClick += delegate {
                if (!this.Gw2IsRunning)
                    LaunchGw2(true);
            };

            this.TrayIconMenu.Items.Add(new ToolStripSeparator());
            ts_exit = this.TrayIconMenu.Items.Add($"{Strings.Common.Action_Exit} {Strings.Common.BlishHUD}");

            ts_exit.Click += delegate { ActiveBlishHud.Exit(); };

            this.TrayIconMenu.Opening += delegate {
                ts_launchGw2.Enabled     = !this.Gw2IsRunning && File.Exists(this.Gw2ExecutablePath);
                ts_launchGw2Auto.Enabled = !this.Gw2IsRunning && File.Exists(this.Gw2ExecutablePath);
            };
        }

        private void TryAttachToGw2() {
            this.Gw2Process = GetGw2Process();

            if (this.Gw2IsRunning) {
                try {
                    this.Gw2Process.EnableRaisingEvents =  true;
                    this.Gw2Process.Exited              += OnGw2Exit;
                } catch (Win32Exception ex) /* [BLISHHUD-W] */ {
                    // Observed as "Access is denied"
                    Logger.Warn(ex, "A Win32Exception was encountered while trying to monitor the Gw2 process. It might be running with different permissions.");
                } catch (InvalidOperationException e) /* [BLISHHUD-1H] */ {
                    // Can get thrown if the game is closed just as we launched it
                    OnGw2Exit(null, EventArgs.Empty);
                }

                BlishHud.Form.Invoke((MethodInvoker) (() => {
                    BlishHud.Form.Visible = true;
                }));
            }
        }

        private Process GetGw2Process() {
            // Check to see if 64-bit Gw2 process is running (since it's likely the most common at this point)
            Process[] gw2Processes = Process.GetProcessesByName(ApplicationSettings.Instance.ProcessName ?? GW2_64_BIT_PROCESSNAME);

            if (gw2Processes.Length == 0) {
                // 64-bit process not found so see if they're using a 32-bit client instead
                gw2Processes = Process.GetProcessesByName(GW2_32_BIT_PROCESSNAME);
            }

            if (gw2Processes.Length > 0) {
                // TODO: We don't currently have multibox support, but future updates should at least handle
                // multiboxing in a better way
                return gw2Processes[0];
            }

            return null;
        }

        private void OnGw2Exit(object sender, EventArgs e) {
            this.Gw2Process = null;

            Logger.Info("Guild Wars 2 application has exited!");

            if (!GameService.Overlay.StayInTray.Value) {
                Application.Exit();
            }
        }

        protected override void Unload() {
            if (this.TrayIcon != null) {
                this.TrayIcon.Visible = false;
                this.TrayIcon.Dispose();
            }
        }
        
        // Keeps track of how long it's been since we last checked for the gw2 process
        private double _lastGw2Check = 0;

        protected override void Update(GameTime gameTime) {
            // Determine if we are in game or not
            this.IsInGame = Gw2Mumble.TimeSinceTick.TotalSeconds <= 0.5;

            if (this.Gw2IsRunning) {
                switch (WindowUtil.UpdateOverlay(BlishHud.FormHandle, this.Gw2WindowHandle, this.Gw2HasFocus)) {
                    case WindowUtil.OverlayUpdateResponse.WithFocus:
                        this.Gw2HasFocus = true;
                        break;

                    case WindowUtil.OverlayUpdateResponse.WithoutFocus:
                        this.Gw2HasFocus = false;
                        break;

                    case WindowUtil.OverlayUpdateResponse.Errored:
                        this.Gw2Process = null;
                        break;
                }
            } else {
                _lastGw2Check += gameTime.ElapsedGameTime.TotalSeconds;
                
                if (_lastGw2Check > GW2_EXE_CHECKRATE) {
                    TryAttachToGw2();

                    _lastGw2Check = 0;
                }
            }
        }

        public void FocusGw2() {
            if (this.Gw2Process != null) {
                try {
                    WindowUtil.SetForegroundWindowEx(this.Gw2Process.MainWindowHandle);
                } catch (NullReferenceException e) {
                    Logger.Warn(e, "Failed to give focus to GW2 handle.");
                }
            }
        }

    }

}
