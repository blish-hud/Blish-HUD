using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Control = Blish_HUD.Controls.Control;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Blish_HUD {
    public class InputService:GameService {

        internal class MouseEvent {
            public WinAPI.MouseHook.MouseMessages EventMessage { get; protected set; }
            public WinAPI.MouseHook.MSLLHOOKSTRUCT EventDetails { get; protected set; }

            public MouseEvent(WinAPI.MouseHook.MouseMessages message, WinAPI.MouseHook.MSLLHOOKSTRUCT hookdetails) {
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

        internal static WinAPI.MouseHook    mouseHook;
        internal static WinAPI.KeyboardHook keyboardHook;

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

                _activeControl = value;

                Control.ActiveControl = _activeControl;
            }
        }


        public bool HookOverride { get; private set; }

        public bool BlockInput => this.FocusedControl != null;
        public Controls.Control FocusedControl { get; set; }

        public ConcurrentQueue<KeyboardMessage> KeyboardMessages = new ConcurrentQueue<KeyboardMessage>();

        internal MouseEvent ClickState { get; set; } = null;

        private Thread _thrdMouseHook;
        private Thread _thrdKeyboardHook;

        private static void HookMouse() {
            mouseHook = new WinAPI.MouseHook();
            mouseHook.HookMouse();

            System.Windows.Forms.Application.Run();

            mouseHook.UnhookMouse();
        }

        private static void HookKeyboard() {
            keyboardHook = new WinAPI.KeyboardHook();
            keyboardHook.HookKeyboard();

            System.Windows.Forms.Application.Run();

            keyboardHook.UnhookKeyboard();
        }

        protected override void Initialize() {
#if !NOMOUSEHOOK
            _thrdMouseHook = new Thread(HookMouse);
            _thrdMouseHook.IsBackground = true;
            _thrdMouseHook.Start();
#endif
            //_thrdKeyboardHook = new Thread(HookKeyboard);
            //_thrdKeyboardHook.IsBackground = true;
            //_thrdKeyboardHook.Start();
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() {
            mouseHook?.UnhookMouse();
            keyboardHook?.UnhookKeyboard();
        }

        protected override void Update(GameTime gameTime) {
            HandleMouse();
            HandleKeyboard();
        }

        private void HandleMouse() {
            if (!GameIntegration.Gw2IsRunning || (this.FocusedControl == null && !GameIntegration.Gw2HasFocus)) {
                this.HudFocused = false;
                return;
            }

            var rawMouseState = Mouse.GetState();
            var newMouseState = new MouseState(
                  (int)(rawMouseState.X / Graphics.UIScaleMultiplier),
                  (int)(rawMouseState.Y / Graphics.UIScaleMultiplier),
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
                if (this.ClickState.EventMessage == WinAPI.MouseHook.MouseMessages.WM_LeftButtonDown) {
                    this.LeftMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonPressed, newMouseState);
                } else if (this.ClickState.EventMessage == WinAPI.MouseHook.MouseMessages.WM_LeftButtonUp) {
                    this.LeftMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonReleased, newMouseState);
                } else if (this.ClickState.EventMessage == WinAPI.MouseHook.MouseMessages.WM_RightButtonDown) {
                    this.RightMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonPressed, newMouseState);
                } else if (this.ClickState.EventMessage == WinAPI.MouseHook.MouseMessages.WM_RightButtonUp) {
                    this.RightMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonReleased, newMouseState);
                } else if (this.ClickState.EventMessage == WinAPI.MouseHook.MouseMessages.WM_MouseWheel) {
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
