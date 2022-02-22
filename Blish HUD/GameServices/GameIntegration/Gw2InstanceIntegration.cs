using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;
using Blish_HUD.GameServices;
using Blish_HUD.Settings;
using Gapotchenko.FX.Diagnostics;
using Microsoft.Win32;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameIntegration {
    public class Gw2InstanceIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<Gw2InstanceIntegration>();

        private const string GW2_REGISTRY_KEY     = @"SOFTWARE\ArenaNet\Guild Wars 2";
        private const string GW2_REGISTRY_PATH_SV = "Path";

        private const string GW2_PATCHWINDOW_CLASS = "ArenaNet";

        private const string APPDATA_ENVKEY = "appdata";

        #region Events

        public event EventHandler<EventArgs> Gw2Started;
        public event EventHandler<EventArgs> Gw2Closed;

        public event EventHandler<EventArgs> Gw2AcquiredFocus;
        public event EventHandler<EventArgs> Gw2LostFocus;

        public event EventHandler<ValueEventArgs<bool>> IsInGameChanged;

        private void OnGw2Started()                            => this.Gw2Started?.Invoke(this, EventArgs.Empty);
        private void OnGw2Closed()                             => this.Gw2Closed?.Invoke(this, EventArgs.Empty);
        private void OnGw2AcquiredFocus()                      => this.Gw2AcquiredFocus?.Invoke(this, EventArgs.Empty);
        private void OnGw2LostFocus()                          => this.Gw2LostFocus?.Invoke(this, EventArgs.Empty);
        private void OnIsInGameChanged(ValueEventArgs<bool> e) => this.IsInGameChanged?.Invoke(this, e);

        #endregion

        private Process _gw2Process;
        public Process Gw2Process {
            get => _gw2Process;
            set {
                if (PropertyUtil.SetProperty(ref _gw2Process, value)) {
                    try {
                        HandleProcessUpdate(value);
                    } catch (Win32Exception) {
                        Debug.Contingency.NotifyWin32AccessDenied();
                        _gw2Process = null;
                    }
                }
            }
        }

        private bool _gw2IsRunning;
        /// <summary>
        /// Indicates that Guild Wars 2 is running and that Blish HUD is actively attached to it.
        /// </summary>
        public bool Gw2IsRunning {
            get => _gw2IsRunning;
            set {
                if (PropertyUtil.SetProperty(ref _gw2IsRunning, value)) {
                    if (value) {
                        OnGw2Started();
                    } else {
                        OnGw2Closed();
                    }
                }
            }
        }

        private bool _gw2HasFocus;
        /// <summary>
        /// Indicates if Guild Wars 2 is the focused foreground window.
        /// </summary>
        public bool Gw2HasFocus {
            get => _gw2HasFocus;
            private set {
                if (PropertyUtil.SetProperty(ref _gw2HasFocus, value)) {
                    if (value) {
                        OnGw2AcquiredFocus();
                    } else {
                        OnGw2LostFocus();
                    }
                }
            }
        }

        /// <summary>
        /// The active Guild Wars 2 process' window handle or <c>IntPtr.Zero</c> if none.
        /// </summary>
        public IntPtr Gw2WindowHandle => _gw2Process?.MainWindowHandle ?? IntPtr.Zero;

        /// <summary>
        /// The path to the Guild Wars 2 executable.
        /// </summary>
        public string Gw2ExecutablePath => _gw2ExecutablePath.Value;

        private bool _isInGame;
        /// <summary>
        /// Indicates if we are actively in game.  If <c>false</c> it indicate that
        /// we're on a loading screen, in a cinamatic, or on the character selection screen.
        /// </summary>
        public bool IsInGame {
            get => _isInGame;
            private set {
                if (PropertyUtil.SetProperty(ref _isInGame, value)) {
                    OnIsInGameChanged(new ValueEventArgs<bool>(value));
                }
            }
        }

        private string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        /// <summary>
        /// Indicates the associated AppData path used by the active Guild Wars 2 instance.
        /// Primarily used when the user is multiboxing and the appdata directory could be changed.
        /// </summary>
        public string AppDataPath {
            get => _appDataPath;
            private set => PropertyUtil.SetProperty(ref _appDataPath, value);
        }

        private string _commandLine;
        /// <summary>
        /// The full command line of the current Guild Wars 2 process.
        /// </summary>
        public string CommandLine {
            get => _commandLine;
            private set => PropertyUtil.SetProperty(ref _commandLine, value);
        }

        // Settings
        private SettingEntry<string> _gw2ExecutablePath;

        private readonly string[] _processNames = { "Gw2-64", "Gw2", "KZW" };

        private bool _exitLocked = false;

        public Gw2InstanceIntegration(GameIntegrationService service) : base(service) { /* NOOP */ }

        public override void Load() {
            DefineSettings(_service.ServiceSettings.AddSubCollection(nameof(Gw2InstanceIntegration)));

            GameService.Gw2Mumble.Info.IsGameFocusedChanged += OnGameFocusChanged;

            TryAttachToGw2();
        }

        private void DefineSettings(SettingCollection settings) {
            const string UNDEFINED_EXECPATH = "NotDetected";

            _gw2ExecutablePath = settings.DefineSetting("Gw2ExecutablePath", UNDEFINED_EXECPATH, () => "Gw2-64.exe Path", () => "The path to the game's executable. This is auto-detected, so don't change this unless you know what you're doing.");

            // We do this to avoid trying to detect in the registry
            // unless we have never detected the true path
            if (_gw2ExecutablePath.Value == UNDEFINED_EXECPATH) {
                _gw2ExecutablePath.Value = GetGw2PathFromRegistry();
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

        private void HandleProcessUpdate(Process newProcess) {
            if (newProcess == null || _gw2Process.HasExited || _gw2Process.MainWindowHandle == IntPtr.Zero) {
                BlishHud.Instance.Form.Invoke((MethodInvoker)(() => { BlishHud.Instance.Form.Visible = false; }));

                _gw2Process = null;
                this.Gw2IsRunning = false;

                if (GameService.Overlay.ShowInTaskbar.Value) {
                    WindowUtil.SetShowInTaskbar(BlishHud.Instance.FormHandle, false);
                }
            } else {
                if (_gw2Process.MainModule != null) {
                    _gw2ExecutablePath.Value = _gw2Process.MainModule.FileName;
                }

                try {
                    this.CommandLine = newProcess.GetCommandLine();
                } catch (Win32Exception e) {
                    this.CommandLine = string.Empty;
                    Logger.Warn(e, "A Win32Exception was encountered while trying to retrieve the process command line.");
                }

                try {
                    var envs = newProcess.ReadEnvironmentVariables();

                    if (envs.ContainsKey(APPDATA_ENVKEY)) {
                        this.AppDataPath = envs[APPDATA_ENVKEY];
                    }
                } catch (EndOfStreamException) {
                    // See: https://github.com/gapotchenko/Gapotchenko.FX/issues/2
                    Logger.Warn("Failed to auto-detect Guild Wars 2 environment variables.  Restart Guild Wars 2 to try again.");
                } catch (NullReferenceException e) {
                    Logger.Warn(e, "Failed to grab Guild Wars 2 env variable.  It is likely exiting.");
                }

                // GW2 is running if the "_gw2Process" isn't null and the class name of process' 
                // window is the game window name (so we know we are passed the login screen)
                string windowClass = WindowUtil.GetClassNameOfWindow(_gw2Process.MainWindowHandle);

                this.Gw2IsRunning = windowClass == ApplicationSettings.Instance.WindowName
                                 || windowClass != GW2_PATCHWINDOW_CLASS;

                if (GameService.Overlay.ShowInTaskbar.Value) {
                    WindowUtil.SetShowInTaskbar(BlishHud.Instance.FormHandle, true);
                }
            }
        }

        private void OnGameFocusChanged(object sender, ValueEventArgs<bool> e) {
            if (e.Value) {
                TryAttachToGw2();
            }
        }

        private void TryAttachToGw2() {
            // Get process from Mumble if it is defined
            // otherwise just get the first instance running
            if (ApplicationSettings.Instance.MumbleMapName != null) {
                // User-set mumble link name - so don't fallback.
                this.Gw2Process = GetMumbleSpecifiedGw2Process();
            } else {
                // No user-set mumble link name - so fallback,
                // starting with default mumble data if found
                this.Gw2Process = GetMumbleSpecifiedGw2Process()
                               ?? GetDefaultGw2ProcessById()
                               ?? GetDefaultGw2ProcessByName();
            }

            if (this.Gw2IsRunning) {
                try {
                    this.Gw2Process.EnableRaisingEvents =  true;
                    this.Gw2Process.Exited              += OnGw2Exit;
                } catch (Win32Exception ex) /* [BLISHHUD-W] */ {
                    // Observed as "Access is denied"
                    Logger.Warn(ex, "A Win32Exception was encountered while trying to monitor the Gw2 process. It might be running with different permissions.");
                } catch (InvalidOperationException) /* [BLISHHUD-1H] */ {
                    // Can get thrown if the game is closed just as we launched it
                    OnGw2Exit(null, EventArgs.Empty);
                }

                BlishHud.Instance.Form.Invoke((MethodInvoker)(() => { BlishHud.Instance.Form.Visible = true; }));
            }
        }

        private Process GetGw2ProcessByPID(int pid, string src) {
            try {
                return Process.GetProcessById(pid);
            } catch (ArgumentException) {
                Logger.Debug("{src} {pid} which did not correlate to an active process.", src, pid);
            } catch (InvalidOperationException) {
                Logger.Debug("{src} {pid} failed to return a process.", src, pid);
            }

            return null;
        }

        private Process GetMumbleSpecifiedGw2Process() {
            GameService.Gw2Mumble.RefreshClient();

            if (GameService.Gw2Mumble.IsAvailable) {
                return GetGw2ProcessByPID((int)GameService.Gw2Mumble.Info.ProcessId, "Mumble reported PID");
            }

            return null;
        }

        private Process GetDefaultGw2ProcessById() {
            if (ApplicationSettings.Instance.ProcessId != 0) {
                return GetGw2ProcessByPID(ApplicationSettings.Instance.ProcessId, "PID specified by --pid");
            }

            return null;
        }

        private Process GetDefaultGw2ProcessByName() {
            Process[] gw2Processes = Array.Empty<Process>();

            if (ApplicationSettings.Instance.ProcessName != null) {
                gw2Processes = Process.GetProcessesByName(ApplicationSettings.Instance.ProcessName);
            } else {
                for (int i = 0; i < _processNames.Length && gw2Processes.Length < 1; i++) {
                    gw2Processes = Process.GetProcessesByName(_processNames[i]);
                }
            }

            return gw2Processes.Length > 0
                       ? gw2Processes[0]
                       : null;
        }

        private string GetGw2PathFromRegistry() {
            try {
                using var gw2Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(GW2_REGISTRY_KEY, RegistryRights.ReadKey);
                if (gw2Key != null) {
                    string gw2Path = gw2Key.GetValue(GW2_REGISTRY_PATH_SV).ToString();

                    if (File.Exists(gw2Path)) {
                        return gw2Path;
                    }
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to read Guild Wars 2 path from registry value {registryKey} located at {registryPath}.", GW2_REGISTRY_PATH_SV, GW2_REGISTRY_KEY);
            }

            return string.Empty;
        }

        public override void Update(GameTime gameTime) {
            // Determine if we are in game or not
            this.IsInGame = GameService.Gw2Mumble.TimeSinceTick.TotalSeconds <= 0.5 && this.Gw2IsRunning;

            if (this.Gw2IsRunning) {
                var updateResult = WindowUtil.UpdateOverlay(BlishHud.Instance.FormHandle, this.Gw2WindowHandle, this.Gw2HasFocus);

                switch (updateResult.Response) {
                    case WindowUtil.OverlayUpdateResponse.WithFocus:
                        this.Gw2HasFocus = true;
                        break;

                    case WindowUtil.OverlayUpdateResponse.WithoutFocus:
                        this.Gw2HasFocus = false;
                        break;

                    case WindowUtil.OverlayUpdateResponse.Errored:
                        switch (updateResult.ErrorCode) {
                            case 1400:
                                this.Gw2Process?.Refresh();
                                
                                if (this.Gw2Process == null || this.Gw2Process.MainWindowHandle == IntPtr.Zero) {
                                    // Guild Wars 2 most likely closed
                                    goto case -1;
                                }

                                break;
                            case -1:
                            default:
                                this.Gw2Process = null;
                                if (GameService.Overlay.ShowInTaskbar.Value) {
                                    WindowUtil.SetShowInTaskbar(BlishHud.Instance.FormHandle, false);
                                }
                                break;
                        }
                        break;
                }

                if (BlishHud.Instance.Form.Visible != !updateResult.Minimized) {
                    BlishHud.Instance.Form.Visible = !updateResult.Minimized;
                }
            } else {
                TryAttachToGw2();
            }
        }

        private void OnGw2Exit(object sender, EventArgs e) {
            if (_exitLocked) {
                return;
            }

            _exitLocked = true;

            this.Gw2Process = null;

            Logger.Info("Guild Wars 2 application has exited!");

            // We close the game if we are not configured to stay in the tray OR if we launched GW2 with
            // command line (we don't want to relaunch the game with a restart).  Otherwise, we restart.
            if (!GameService.Overlay.StayInTray.Value || ApplicationSettings.Instance.StartGw2 > 0) {
                GameService.Overlay.Exit();
            } else {
                GameService.Overlay.Restart();
            }
        }

    }
}
