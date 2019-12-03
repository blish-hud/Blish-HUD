using System;
using System.Runtime.InteropServices;
using Blish_HUD.Controls;
using Blish_HUD.Input.WinApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    public class MouseManager : InputManager {

        private static readonly Logger Logger = Logger.GetLogger<MouseManager>();

        #region Event Handling

        public event EventHandler<MouseEventArgs> MouseMoved;
        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseWheelScrolled;

        private void OnMouseEvent(MouseEventArgs e) {
            switch (e.EventType) {
                case MouseEventType.MouseMoved:
                    MouseMoved?.Invoke(this, e);
                    break;
                case MouseEventType.LeftMouseButtonPressed:
                    LeftMouseButtonPressed?.Invoke(this, e);
                    break;
                case MouseEventType.LeftMouseButtonReleased:
                    LeftMouseButtonReleased?.Invoke(this, e);
                    break;
                case MouseEventType.RightMouseButtonPressed:
                    RightMouseButtonPressed?.Invoke(this, e);
                    break;
                case MouseEventType.RightMouseButtonReleased:
                    RightMouseButtonReleased?.Invoke(this, e);
                    break;
                case MouseEventType.MouseWheelScrolled:
                    MouseWheelScrolled?.Invoke(this, e);
                    break;
                default:
                    // NOOP - Mouse event type not supported
                    break;
            }
        }

#endregion

        /// <summary>
        /// The current position of the mouse relative to the application.
        /// </summary>
        public Point Position => State.Position;

        /// <summary>
        /// The current state of the mouse.
        /// </summary>
        public MouseState State { get; private set; }

        private MouseEventArgs _mouseEvent;

        private bool _cameraDragging;

        private bool _hudFocused;

        private bool _hookOverride;

        private Control _activeControl;

        /// <summary>
        /// The <see cref="Control"/> that last accepted a mouse event.
        /// </summary>
        public Control ActiveControl {
            get => _activeControl;
            private set {
                _hudFocused   = value != null;
                _hookOverride = value != null && value.Captures.HasFlag(CaptureType.ForceNone);

                _activeControl = value;

                Control.ActiveControl = value;
            }
        }

        internal MouseManager() : base(HookType.WH_MOUSE_LL) { /* NOOP */ }

        internal override void Update() {
            if (!GameService.GameIntegration.Gw2IsRunning || !GameService.GameIntegration.Gw2HasFocus) {
                _hudFocused = false;
                return;
            }

            if (_cameraDragging) return;

            var rawMouseState = Mouse.GetState();

            var newMouseState = new MouseState((int) (rawMouseState.X / GameService.Graphics.UIScaleMultiplier),
                                               (int) (rawMouseState.Y / GameService.Graphics.UIScaleMultiplier),
                                               _mouseEvent?.Details.WheelDelta ?? 0,
                                               rawMouseState.LeftButton,
                                               rawMouseState.MiddleButton,
                                               rawMouseState.RightButton,
                                               rawMouseState.XButton1,
                                               rawMouseState.XButton2);

            // Handle mouse moved
            if (State.Position != newMouseState.Position) {
                if (_hookOverride) {
                    this.ActiveControl = this.ActiveControl.MouseOver ? this.ActiveControl : null;
                }

                this.ActiveControl = GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseMoved, newMouseState);
                this.MouseMoved?.Invoke(this, new MouseEventArgs(MouseEventType.MouseMoved));
            }

            // Handle mouse left pressed/released
            if (State.LeftButton != newMouseState.LeftButton) {
                if (newMouseState.LeftButton == ButtonState.Pressed) {
                    this.LeftMouseButtonPressed?.Invoke(this, new MouseEventArgs(MouseEventType.LeftMouseButtonPressed));
                    GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonPressed, newMouseState);
                } else if (newMouseState.LeftButton == ButtonState.Released) {
                    this.LeftMouseButtonReleased?.Invoke(this, new MouseEventArgs(MouseEventType.LeftMouseButtonReleased));
                    GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonReleased, newMouseState);
                }
            }

            // Handle mouse events blocked by the mouse hook
            if (_mouseEvent != null) {
                switch (_mouseEvent.EventType) {
                    case MouseEventType.MouseMoved:
                    case MouseEventType.RightMouseButtonPressed:
                    case MouseEventType.RightMouseButtonReleased:
                    case MouseEventType.MouseWheelScrolled:
                        break;
                    case MouseEventType.LeftMouseButtonPressed:
                    case MouseEventType.LeftMouseButtonReleased:
                        OnMouseEvent(_mouseEvent);
                        GameService.Graphics.SpriteScreen.TriggerMouseInput(_mouseEvent.EventType, newMouseState);
                        break;
                    default:
                        Logger.Debug("Got unsupported input {mouseDataMessage}.", _mouseEvent.EventType);
                        break;
                }

                _mouseEvent = null;
            }

            // Handle mouse right pressed/released
            if (State.RightButton != newMouseState.RightButton) {
                if (newMouseState.RightButton == ButtonState.Pressed) {
                    this.RightMouseButtonPressed?.Invoke(this, new MouseEventArgs(MouseEventType.RightMouseButtonPressed));
                    GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonPressed, newMouseState);
                } else if (newMouseState.RightButton == ButtonState.Released) {
                    this.RightMouseButtonReleased?.Invoke(this, new MouseEventArgs(MouseEventType.RightMouseButtonReleased));
                    GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonReleased, newMouseState);
                }
            }

            // Handle mouse scroll
            if (newMouseState.ScrollWheelValue != 0) {
                this.MouseWheelScrolled?.Invoke(this, new MouseEventArgs(MouseEventType.MouseWheelScrolled));
                GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseWheelScrolled, newMouseState);
            }

            State = newMouseState;
        }

        protected override bool HandleNewInput(IntPtr wParam, IntPtr lParam) {
            var newEvent = new MouseEventArgs((MouseEventType)wParam, Marshal.PtrToStructure<MouseLLHookStruct>(lParam));

            if (newEvent.EventType == MouseEventType.MouseMoved) return false;

            if (_cameraDragging && newEvent.EventType == MouseEventType.RightMouseButtonReleased) {
                _cameraDragging = false;
            } else if (_hudFocused && !_hookOverride) {
                _mouseEvent = newEvent;

                #if !NOMOUSEHOOK
                return newEvent.EventType != MouseEventType.LeftMouseButtonReleased;
                #endif
            } else if (newEvent.EventType == MouseEventType.RightMouseButtonPressed) {
                _cameraDragging = true;
            }

            return false;
        }

    }
}
