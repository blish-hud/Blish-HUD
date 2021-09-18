using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.GameIntegration;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class GameIntegrationService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<GameIntegrationService>();

        private const string GAMEINTEGRATION_SETTINGS = "GameIntegrationConfiguration";

        public Gw2ProcIntegration    Gw2Proc    { get; private set; }
        public ClientTypeIntegration ClientType { get; private set; }
        public AudioIntegration      Audio      { get; private set; }
        public TacOIntegration       TacO       { get; private set; }
        public WinFormsIntegration   WinForms   { get; private set; }

        #region Obsolete Gw2Proc

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2Closed instead.", true)]
        public event EventHandler<EventArgs> Gw2Closed;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2Started instead.", true)]
        public event EventHandler<EventArgs> Gw2Started;


        [Obsolete("Use GameIntegration.Gw2Proc.Gw2AcquiredFocus instead.", true)]
        public event EventHandler<EventArgs> Gw2AcquiredFocus;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2LostFocus instead.", true)]
        public event EventHandler<EventArgs> Gw2LostFocus;

        [Obsolete("Use GameIntegration.Gw2Proc.IsInGameChanged instead.", true)]
        public event EventHandler<ValueEventArgs<bool>> IsInGameChanged;
        
        public IGameChat Chat { get; private set; }

        [Obsolete("Use GameIntegration.Gw2Proc.IsInGame instead.", true)]
        public bool IsInGame => this.Gw2Proc.IsInGame;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2HasFocus instead.", true)]
        public bool Gw2HasFocus => this.Gw2Proc.Gw2HasFocus;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2IsRunning instead.", true)]
        public bool Gw2IsRunning => this.Gw2Proc.Gw2IsRunning;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2WindowHandle instead.", true)]
        public IntPtr Gw2WindowHandle => Gw2Proc.Gw2WindowHandle;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2ExecutablePath instead.", true)]
        public string Gw2ExecutablePath => this.Gw2Proc.Gw2ExecutablePath;

        [Obsolete("Use GameIntegration.Gw2Proc.Gw2Process instead.", true)]
        public Process Gw2Process => Gw2Proc.Gw2Process;

        [Obsolete("Use GameIntegration.Gw2Proc.FocusGw2() instead.", true)]
        public void FocusGw2() => this.Gw2Proc.FocusGw2();

        #endregion

        internal SettingCollection ServiceSettings { get; private set; }

        internal GameIntegrationService() {
            SetServiceModules(this.Gw2Proc    = new Gw2ProcIntegration(this),
                              this.ClientType = new ClientTypeIntegration(this),
                              this.Audio      = new AudioIntegration(this),
                              this.TacO       = new TacOIntegration(this),
                              this.WinForms   = new WinFormsIntegration(this));
        }

        protected override void Initialize() {
            this.ServiceSettings = Settings.RegisterRootSettingCollection(GAMEINTEGRATION_SETTINGS);

            Chat = new GameChat();
        }

        protected override void Load() {
            BlishHud.Instance.Form.Shown += delegate {
                WindowUtil.SetupOverlay(BlishHud.Instance.FormHandle);
            };
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

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
        /// 
        [Obsolete("No longer supported here in Core.", true)]
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
                return !GameIntegration.Gw2Proc.Gw2IsRunning || !GameIntegration.Gw2Proc.Gw2HasFocus || !GameIntegration.Gw2Proc.IsInGame;
            }
        }
        #endregion

    }
}