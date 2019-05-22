using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Control = Blish_HUD.Controls.Control;
using Keys = Microsoft.Xna.Framework.Input.Keys;

public class MouseEventArgs : EventArgs {
    public MouseState MouseState { get; }

    /// <summary>
    /// The relative mouse position when the event was fired.
    /// </summary>
    public Point MousePosition { get; }

    public MouseEventArgs(MouseState ms) {
        this.MouseState = ms;
    }
}

public enum MouseEventType {
    LeftMouseButtonPressed,
    LeftMouseButtonReleased,
    MouseMoved,
    RightMouseButtonPressed,
    RightMouseButtonReleased,
    MouseWheelScrolled
}

public enum KeyboardEventType {
    KeyDown = 0x0100,
    KeyUp = 0x0101
}

public struct KeyboardMessage {

    public int    uMsg;
    public KeyboardEventType EventType;
    public Keys Key;

    public KeyboardMessage(int _uMsg, IntPtr _wParam, int _lParam) {
        uMsg   = _uMsg;
        EventType = (KeyboardEventType)_wParam;
        Key = (Keys)_lParam;
    }

}

namespace Blish_HUD {
    public class InputService:GameService {

        public class MouseEvent {
            public WinApi.MouseHook.MouseMessages EventMessage { get; protected set; }
            public WinApi.MouseHook.MSLLHOOKSTRUCT EventDetails { get; protected set; }

            public MouseEvent(WinApi.MouseHook.MouseMessages message, WinApi.MouseHook.MSLLHOOKSTRUCT hookdetails) {
                this.EventMessage = message;
                this.EventDetails = hookdetails;
            }
        }

        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseMoved;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseWheelScrolled;
        
        public MouseState MouseState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }

        // TODO: Rename this to prevent confusion with the (Thread) "thrdMouseHook"
        internal static WinApi.MouseHook mouseHook;
        internal static WinApi.KeyboardHook keyboardHook;

        public bool MouseHidden {
            get => mouseHook.NonClick;
        }

        public bool HudFocused { get; private set; }

        private Controls.Control _activeControl = null;
        public Controls.Control ActiveControl {
            get => _activeControl;
            set {
                this.HudFocused = value != null;
                this.HookOverride = (value != null && value.Captures.HasFlag(CaptureType.ForceNone));

                //string controlName = value?.GetType().Name;
                //if (!string.IsNullOrWhiteSpace(controlName))
                //    Console.WriteLine(controlName);

                _activeControl = value;
            }
        }


        public bool HookOverride { get; private set; }

        public bool BlockInput => this.FocusedControl != null;
        public Controls.Control FocusedControl { get; set; }

        public ConcurrentQueue<KeyboardMessage> KeyboardMessages = new ConcurrentQueue<KeyboardMessage>();

        public MouseEvent ClickState { get; set; } = null;

        private Thread thrdMouseHook;
        private Thread thrdKeyboardHook;

        private static void HookMouse() {
            mouseHook = new WinApi.MouseHook();
            mouseHook.HookMouse();

            System.Windows.Forms.Application.Run();

            mouseHook.UnHookMouse();
        }

        private static void HookKeyboard() {
            keyboardHook = new WinApi.KeyboardHook();
            keyboardHook.HookKeyboard();

            System.Windows.Forms.Application.Run();

            keyboardHook.UnHookKeyboard();
        }

        protected override void Initialize() {
#if !NOMOUSEHOOK
            thrdMouseHook = new Thread(HookMouse);
            thrdMouseHook.IsBackground = true;
            thrdMouseHook.Start();
#endif
            //thrdKeyboardHook = new Thread(HookKeyboard);
            //thrdKeyboardHook.IsBackground = true;
            //thrdKeyboardHook.Start();
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() {
            mouseHook?.UnHookMouse();
            //keyboardHook.UnHookKeyboard();
        }

        protected override void Update(GameTime gameTime) {
            HandleMouse();
            HandleKeyboard();
        }

        private void HandleMouse() {
            if (!GameIntegration.Gw2IsRunning || (this.FocusedControl == null & !Utils.Window.OnTop)) {
                this.HudFocused = false;
                return;
            }

            var rawMouseState = Mouse.GetState();
            var newMouseState = new MouseState(
                  (int)(rawMouseState.X / GameService.Graphics.GetScaleRatio(GameService.Graphics.UIScale)),
                  (int)(rawMouseState.Y / GameService.Graphics.GetScaleRatio(GameService.Graphics.UIScale)),
                  rawMouseState.ScrollWheelValue,
                  rawMouseState.LeftButton,
                  rawMouseState.MiddleButton,
                  rawMouseState.RightButton,
                  rawMouseState.XButton1,
                  rawMouseState.XButton2
              );

            // Handle mouse moved
            if (this.MouseState.Position != newMouseState.Position) {
                if (this.HookOverride)
                    this.ActiveControl = this.ActiveControl.MouseOver ? this.ActiveControl : null;

                this.ActiveControl = GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseMoved, newMouseState);
                this.MouseMoved?.Invoke(null, new MouseEventArgs(newMouseState));
            }

            // Handle mouse left pressed/released
            if (this.MouseState.LeftButton != newMouseState.LeftButton) {
                if (newMouseState.LeftButton == ButtonState.Pressed) {
                    this.LeftMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonPressed, newMouseState);
                } else if (newMouseState.LeftButton == ButtonState.Released) {
                    this.LeftMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonReleased, newMouseState);
                }
            }

            // Handle mouse left pressed/released (through mouse hook)
            if (this.ClickState != null) {
                if (this.ClickState.EventMessage == WinApi.MouseHook.MouseMessages.WM_LeftButtonDown) {
                    this.LeftMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonPressed, newMouseState);
                } else if (this.ClickState.EventMessage == WinApi.MouseHook.MouseMessages.WM_LeftButtonUp) {
                    this.LeftMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonReleased, newMouseState);
                } else if (this.ClickState.EventMessage == WinApi.MouseHook.MouseMessages.WM_RightButtonDown) {
                    this.RightMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonPressed, newMouseState);
                } else if (this.ClickState.EventMessage == WinApi.MouseHook.MouseMessages.WM_RightButtonUp) {
                    this.RightMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonReleased, newMouseState);
                } else if (this.ClickState.EventMessage == WinApi.MouseHook.MouseMessages.WM_MouseWheel) {
                    this.MouseWheelScrolled?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseWheelScrolled, newMouseState);
                }

                this.ClickState = null;
            }

            // Handle mouse right pressed/released
            if (this.MouseState.RightButton != newMouseState.RightButton) {
                if (newMouseState.RightButton == ButtonState.Pressed) {
                    this.RightMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonPressed, newMouseState);
                } else if (newMouseState.RightButton == ButtonState.Released) {
                    this.RightMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonReleased, newMouseState);
                }
            }

            // Handle mouse scroll
            if (this.MouseState.ScrollWheelValue != newMouseState.ScrollWheelValue) {
                this.MouseWheelScrolled?.Invoke(null, new MouseEventArgs(newMouseState));
                Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseWheelScrolled, newMouseState);
            }

            // TODO: Check to see if mouse is over any 3D entities
            if (!this.HudFocused) {

            }

            this.MouseState = newMouseState;
        }

        private IEnumerable<Keys> prevKeysDown;

        // TODO: Expose this in a better way
        public List<Keys> KeysDown = new List<Keys>();

        public bool IsKeyDown(params Keys[] keys) { return KeysDown.Intersect(keys).Any(); }
        public bool IsKeyUp(params Keys[] keys) { return !IsKeyDown(keys); }

        public bool ShiftIsDown() { return IsKeyDown(Keys.LeftShift, Keys.RightShift); }
        public bool AltIsDown() { return IsKeyDown(Keys.LeftAlt, Keys.RightAlt); }
        public bool ControlIsDown() { return IsKeyDown(Keys.LeftControl, Keys.RightControl); }


        private void HandleKeyboard() {
            while (KeyboardMessages.TryDequeue(out var keyboardMessage)) {


                if (keyboardMessage.EventType == KeyboardEventType.KeyDown) {
                    if (KeysDown.Contains(keyboardMessage.Key)) continue;

                    KeysDown.Add(keyboardMessage.Key);
                } else {
                    KeysDown.Remove(keyboardMessage.Key);
                }
                
                this.FocusedControl?.TriggerKeyboardInput(keyboardMessage);
            }
        }

    }
}
