using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.GameIntegration;
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

        public event EventHandler<ValueEventArgs<bool>> IsInGameChanged;

        private const string GW2_REGISTRY_KEY     = @"SOFTWARE\ArenaNet\Guild Wars 2";
        private const string GW2_REGISTRY_PATH_SV = "Path";

        private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
        private const string GW2_GAMEWINDOW_NAME  = "ArenaNet_Dx_Window_Class";

        private const string GAMEINTEGRATION_SETTINGS = "GameIntegrationConfiguration";

        public AudioIntegration      Audio      { get; private set; }
        public TacOIntegration       TacO       { get; private set; }
        public WinFormsIntegration   WinForms   { get; private set; }
        public ClientTypeIntegration ClientType { get; private set; }

        private readonly string[] _processNames = { "Gw2-64", "Gw2", "KZW" };

        public IGameChat Chat { get; private set; }

        private bool _isInGame;
        public bool IsInGame { 
            get => _isInGame; 
            private set { 
                if (_isInGame == value) return;

                _isInGame = value;

                IsInGameChanged?.Invoke(this, new ValueEventArgs<bool>(_isInGame));
            }
        }

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
                    BlishHud.Instance.Form.Invoke((MethodInvoker) (() => { BlishHud.Instance.Form.Visible = false; }));

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

        private ISettingCollection _gameIntegrationSettings;

        private IUiSettingEntry<string> _gw2ExecutablePath;

        public string Gw2ExecutablePath => _gw2ExecutablePath.Value;
        protected override void Initialize() {
            Chat = new GameChat();

            _gameIntegrationSettings = Settings.RegisterRootSettingCollection(GAMEINTEGRATION_SETTINGS);

            DefineSettings(_gameIntegrationSettings);
        }

        private void DefineSettings(ISettingCollection settings) {
            const string UNDEFINED_EXECPATH = "NotDetected";

            _gw2ExecutablePath = settings.DefineUiSetting("Gw2ExecutablePath", UNDEFINED_EXECPATH, () => "Gw2-64.exe Path", () => "The path to the game's executable. This is auto-detected, so don't change this unless you know what you're doing.");

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

        protected override void Load() {
            this.ClientType = new ClientTypeIntegration(this);
            this.Audio      = new AudioIntegration(this);
            this.TacO       = new TacOIntegration(this);
            this.WinForms   = new WinFormsIntegration(this);

            BlishHud.Instance.Form.Shown += delegate {
                WindowUtil.SetupOverlay(BlishHud.Instance.FormHandle);
            };

            TryAttachToGw2();

            Gw2Mumble.Info.IsGameFocusedChanged += OnGameFocusChanged;
        }

        private void OnGameFocusChanged(object sender, ValueEventArgs<bool> e) {
            if (e.Value) {
                TryAttachToGw2();
            }
        }

        private void TryAttachToGw2() {
            // Get process from Mumble if it is defined
            // otherwise just get the first instance running
            this.Gw2Process = GetMumbleSpecifiedGw2Process()
                           ?? GetDefaultGw2ProcessById()
                           ?? GetDefaultGw2ProcessByName();

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

                BlishHud.Instance.Form.Invoke((MethodInvoker) (() => { BlishHud.Instance.Form.Visible = true; }));
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
            if (Gw2Mumble.IsAvailable) {
                return GetGw2ProcessByPID((int) Gw2Mumble.Info.ProcessId, "Mumble reported PID");
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
            } else {
                Overlay.Restart();
            }
        }

        protected override void Unload() {
            this.ClientType.Unload();
            this.Audio.Unload();
            this.TacO.Unload();
            this.WinForms.Unload();
        }

        protected override void Update(GameTime gameTime) {
            // Determine if we are in game or not
            this.IsInGame = Gw2Mumble.TimeSinceTick.TotalSeconds <= 0.5 && this.Gw2IsRunning;

            if (this.Gw2IsRunning) {
                this.ClientType.Update(gameTime);
                this.Audio.Update(gameTime);
                this.TacO.Update(gameTime);
                this.WinForms.Update(gameTime);

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
                TryAttachToGw2();
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
            /// <summary>
            /// Sends a message to the chat.
            /// </summary>
            void Send(string message);
            /// <summary>
            /// Adds a string to the input field.
            /// </summary>
            void Paste(string text);
            /// <summary>
            /// Returns the current string in the input field.
            /// </summary>
            Task<string> GetInputText();
            /// <summary>
            /// Clears the input field.
            /// </summary>
            void Clear();
        }
        ///<inheritdoc/>
        private class GameChat : IGameChat {
            ///<inheritdoc/>
            public async void Send(string message) {
                if (IsBusy() || !IsTextValid(message)) return;
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
            ///<inheritdoc/>
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
            ///<inheritdoc/>
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
            ///<inheritdoc/>
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