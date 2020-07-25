using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
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

        private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
        private const string GW2_GAMEWINDOW_NAME  = "ArenaNet_Dx_Window_Class";

        private const string GAMEINTEGRATION_SETTINGS = "GameIntegrationConfiguration";

        private readonly string[] _processNames = { "Gw2-64", "Gw2", "KZW" };

        public NotifyIcon TrayIcon { get; private set; }
        public ContextMenuStrip TrayIconMenu { get; private set; }
        public IGameChat Chat { get; private set; }
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

        public IntPtr Gw2WindowHandle => _gw2Process?.MainWindowHandle ?? IntPtr.Zero;

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
            Chat = new GameChat();

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
            var args = new List<string>();

            // Auto login
            if (autologin) {
                args.Add("-autologin");
            }

            // Mumble target name
            if (ApplicationSettings.Instance.MumbleMapName != null) {
                args.Add($"-mumble \"{ApplicationSettings.Instance.MumbleMapName}\"");
            }

            if (File.Exists(this.Gw2ExecutablePath)) {
                var gw2Proc = new Process {
                    StartInfo = {
                        FileName  = this.Gw2ExecutablePath,
                        Arguments = string.Join(" ", args)
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

            Gw2Mumble.Info.IsGameFocusedChanged += OnGameFocusChanged;
        }

        private void OnGameFocusChanged(object sender, ValueEventArgs<bool> e) {
            if (e.Value) {
                TryAttachToGw2();
            }
        }

        #region TrayIcon Menu Items

        private ToolStripItem ts_launchGw2;
        private ToolStripItem ts_launchGw2Auto;
        private ToolStripItem ts_exit;

        #endregion

        private void CreateTrayIcon() {
            string trayIconText = Strings.Common.BlishHUD;

            if (ApplicationSettings.Instance.MumbleMapName != null) {
                trayIconText += $" ({ApplicationSettings.Instance.MumbleMapName})";
            }

            this.TrayIconMenu = new ContextMenuStrip();

            // Found this here: https://stackoverflow.com/a/25409865/595437
            // Extract the tray icon from our assembly
            this.TrayIcon = new NotifyIcon() {
                Icon             = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text             = trayIconText,
                Visible          = true,
                ContextMenuStrip = this.TrayIconMenu
            };

            // Populate TrayIconMenu items
            ts_launchGw2Auto = this.TrayIconMenu.Items.Add($"{Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2} - {Strings.GameServices.GameIntegrationService.TrayIcon_Autologin}");
            ts_launchGw2     = this.TrayIconMenu.Items.Add(Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2);

            ts_launchGw2Auto.Click += delegate { LaunchGw2(true); };
            ts_launchGw2.Click     += delegate { LaunchGw2(false); };

            this.TrayIcon.DoubleClick += delegate {
                if (!this.Gw2IsRunning) {
                    LaunchGw2(true);
                }
            };

            this.TrayIconMenu.Items.Add(new ToolStripSeparator());
            ts_exit = this.TrayIconMenu.Items.Add($"{Strings.Common.Action_Exit} {Strings.Common.BlishHUD}");

            ts_exit.Click += delegate { Overlay.Exit(); };

            this.TrayIconMenu.Opening += delegate {
                ts_launchGw2.Enabled     = !this.Gw2IsRunning && File.Exists(this.Gw2ExecutablePath);
                ts_launchGw2Auto.Enabled = !this.Gw2IsRunning && File.Exists(this.Gw2ExecutablePath);
            };
        }

        private void TryAttachToGw2() {
            // Get process from Mumble if it is defined
            // otherwise just get the first instance running
            this.Gw2Process = GetMumbleSpecifiedGw2Process()
                           ?? GetDefaultGw2Process();

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

                BlishHud.Form.Invoke((MethodInvoker) (() => {
                    BlishHud.Form.Visible = true;
                }));
            }
        }

        private Process GetMumbleSpecifiedGw2Process() {
            if (Gw2Mumble.IsAvailable) {
                try {
                    return Process.GetProcessById((int) Gw2Mumble.Info.ProcessId);
                } catch (ArgumentException) {
                    Logger.Debug("Mumble reported PID {pid} which did not correlate to an active process.", Gw2Mumble.Info.ProcessId);
                } catch (InvalidOperationException) {
                    Logger.Debug("Mumble reported PID {pid} failed to return a process.", Gw2Mumble.Info.ProcessId);
                }
            }

            return null;
        }

        private Process GetDefaultGw2Process() {
            var gw2Processes = new Process[0];

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

        private void OnGw2Exit(object sender, EventArgs e) {
            this.Gw2Process = null;

            Logger.Info("Guild Wars 2 application has exited!");

            if (!Overlay.StayInTray.Value) {
                Overlay.Exit();
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
                var updateResult = WindowUtil.UpdateOverlay(BlishHud.FormHandle, this.Gw2WindowHandle, this.Gw2HasFocus);

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
                                this.Gw2Process.Refresh();

                                if (this.Gw2Process.MainWindowHandle == IntPtr.Zero) {
                                    // Guild Wars 2 most likely closed
                                    goto case -1;
                                }

                                break;
                            case -1:
                            default:
                                this.Gw2Process = null;
                                break;
                        }
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

        #region Chat Interactions
        /// <summary>
        /// Methods related to interaction with the in-game chat.
        /// </summary>
        public interface IGameChat {
            void Send(string message);
            void Paste(string text);
            Task<string> GetInputText();
            void Clear();
        }
        private class GameChat : IGameChat {
            /// <summary>
            /// Sends a message to the chat.
            /// </summary>
            public async void Send(string message) {
                if (IsBusy() && !IsTextValid(message)) return;
                byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
                await ClipboardUtil.WindowsClipboardService.SetTextAsync(message)
                                   .ContinueWith(clipboardResult => {
                                       if (clipboardResult.IsFaulted)
                                           Logger.Warn(clipboardResult.Exception, "Failed to set clipboard text to {message}!", message);
                                       else
                                           Task.Run(() => {
                                               Focus();
                                               Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                               Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                               Thread.Sleep(50);
                                               Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                                               Keyboard.Stroke(VirtualKeyShort.RETURN);
                                           }).ContinueWith(result => {
                                               if (result.IsFaulted) {
                                                   Logger.Warn(result.Exception, "Failed to send message {message}", message);
                                               } else if (prevClipboardContent != null)
                                                   ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                           }); });
            }
            /// <summary>
            /// Adds a string to the input field.
            /// </summary>
            public async void Paste(string text) {
                if (IsBusy()) return;
                string currentInput = await GetInputText();
                if (!IsTextValid(currentInput + text)) return;
                byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
                await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)
                                   .ContinueWith(clipboardResult => {
                                       if (clipboardResult.IsFaulted)
                                           Logger.Warn(clipboardResult.Exception, "Failed to set clipboard text to {text}!", text);
                                       else
                                           Task.Run(() => {
                                               Focus();
                                               Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                               Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                               Thread.Sleep(50);
                                               Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                                           }).ContinueWith(result => {
                                               if (result.IsFaulted) {
                                                   Logger.Warn(result.Exception, "Failed to paste {text}", text);
                                               } else if (prevClipboardContent != null)
                                                   ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                           }); });
            }
            /// <summary>
            /// Returns the current string in the input field.
            /// </summary>
            public async Task<string> GetInputText() {
                if (IsBusy()) return "";
                byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
                await Task.Run(() => {
                    Focus();
                    Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                    Keyboard.Stroke(VirtualKeyShort.KEY_A, true);
                    Keyboard.Stroke(VirtualKeyShort.KEY_C, true);
                    Thread.Sleep(50);
                    Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                    Unfocus();
                });
                string inputText = await ClipboardUtil.WindowsClipboardService.GetTextAsync()
                                                      .ContinueWith(result => {
                                                          if (prevClipboardContent != null)
                                                              ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                                          return !result.IsFaulted ? result.Result : "";
                                                      });
                return inputText;
            }
            /// <summary>
            /// Clears the input field.
            /// </summary>
            public void Clear() {
                if (IsBusy()) return;
                Task.Run(() => {
                    Focus();
                    Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                    Keyboard.Stroke(VirtualKeyShort.KEY_A, true);
                    Thread.Sleep(50);
                    Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                    Keyboard.Stroke(VirtualKeyShort.BACK);
                    Unfocus();
                });
            }
            private void Focus() {
                Unfocus();
                Keyboard.Stroke(VirtualKeyShort.RETURN);
            }
            private void Unfocus() {
                Mouse.Click(MouseButton.LEFT, Graphics.GraphicsDevice.Viewport.Width / 2, 0);
            }
            private bool IsTextValid(string text) {
                return (text != null && text.Length < 200);
                // More checks? (Symbols: https://wiki.guildwars2.com/wiki/User:MithranArkanere/Charset)
            }
            private bool IsBusy() {
                return !GameIntegration.Gw2IsRunning || !GameIntegration.Gw2HasFocus || !GameIntegration.IsInGame;
            }
        }
        #endregion

    }
}