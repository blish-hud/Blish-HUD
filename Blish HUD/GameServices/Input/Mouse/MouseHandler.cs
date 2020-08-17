using System;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    public class MouseHandler : IInputHandler {

        private static readonly Logger Logger = Logger.GetLogger<MouseHandler>();

        /// <summary>
        ///     The current position of the mouse relative to the application.
        /// </summary>
        public Point Position => this.State.Position;

        public Point PositionRaw { get; private set; }

        /// <summary>
        ///     The current state of the mouse.
        /// </summary>
        public MouseState State { get; private set; }

        /// <summary>
        ///     The <see cref="Control" /> that the mouse last moved over.
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

        private Control _activeControl;

        private bool _cameraDragging;

        /// <summary>
        ///     Indicates if the <see cref="ActiveControl" /> has <see cref="Control.Captures" />
        ///     set to <see cref="CaptureType.ForceNone" />.
        /// </summary>
        private bool _hookOverride;

        private bool _hudFocused;

        private MouseEventArgs _mouseEvent;

        internal MouseHandler() { }

        public bool HandleInput(MouseEventArgs mouseEventArgs) {
            if (mouseEventArgs.EventType == MouseEventType.MouseMoved) {
                this.PositionRaw = new Point(mouseEventArgs.PointX, mouseEventArgs.PointY);
                return false;
            }

            if (_cameraDragging && mouseEventArgs.EventType == MouseEventType.RightMouseButtonReleased) {
                _cameraDragging = false;
            } else if (_hudFocused && !_hookOverride) {
                _mouseEvent = mouseEventArgs;
                return mouseEventArgs.EventType != MouseEventType.LeftMouseButtonReleased;
            } else if (mouseEventArgs.EventType == MouseEventType.RightMouseButtonPressed) {
                _cameraDragging = true;
            }

            return false;
        }

        private bool HandleHookedMouseEvent(MouseEventArgs e) {
            switch (e.EventType) {
                case MouseEventType.LeftMouseButtonPressed:
                    this.LeftMouseButtonPressed?.Invoke(this, e);
                    break;
                case MouseEventType.LeftMouseButtonReleased:
                    this.LeftMouseButtonReleased?.Invoke(this, e);
                    break;
                case MouseEventType.RightMouseButtonPressed:
                    this.RightMouseButtonPressed?.Invoke(this, e);
                    break;
                case MouseEventType.RightMouseButtonReleased:
                    this.RightMouseButtonReleased?.Invoke(this, e);
                    break;
                case MouseEventType.MouseWheelScrolled:
                    this.MouseWheelScrolled?.Invoke(this, e);
                    break;
                default:
                    Logger.Debug("Got unsupported input {mouseDataMessage}.", e.EventType);
                    return false;
            }

            return true;
        }

        public void Update() {
            if (!GameService.GameIntegration.Gw2IsRunning || !GameService.GameIntegration.Gw2HasFocus) {
                _hudFocused = false;
                return;
            }

            if (_cameraDragging) {
                return;
            }

            var prevMouseState = this.State;

            var rawMouseState = Mouse.GetState();

            this.State = new MouseState(
                                        (int) (PositionRaw.X / GameService.Graphics.UIScaleMultiplier),
                                        (int) (PositionRaw.Y / GameService.Graphics.UIScaleMultiplier),
                                        _mouseEvent?.WheelDelta ?? 0,
                                        rawMouseState.LeftButton,
                                        rawMouseState.MiddleButton,
                                        rawMouseState.RightButton,
                                        rawMouseState.XButton1,
                                        rawMouseState.XButton2
                                       );

            // Handle mouse moved
            if (prevMouseState.Position != this.State.Position) {
                if (_hookOverride) {
                    this.ActiveControl = this.ActiveControl.MouseOver ? this.ActiveControl : null;
                }

                this.ActiveControl = GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseMoved, this.State);
                this.MouseMoved?.Invoke(this, new MouseEventArgs(MouseEventType.MouseMoved));
            }

            // Handle mouse events blocked by the mouse hook
            if (_mouseEvent != null) {
                if (HandleHookedMouseEvent(_mouseEvent)) {
                    GameService.Graphics.SpriteScreen.TriggerMouseInput(_mouseEvent.EventType, this.State);
                }

                _mouseEvent = null;
            }

            // Handle mouse left pressed/released
            if (prevMouseState.LeftButton != this.State.LeftButton) {
                switch (this.State.LeftButton) {
                    case ButtonState.Pressed:
                        this.LeftMouseButtonPressed?.Invoke(this, new MouseEventArgs(MouseEventType.LeftMouseButtonPressed));
                        break;
                    case ButtonState.Released:
                        this.LeftMouseButtonReleased?.Invoke(this, new MouseEventArgs(MouseEventType.LeftMouseButtonReleased));
                        break;
                }
            }

            // Handle mouse right pressed/released
            if (prevMouseState.RightButton != this.State.RightButton) {
                switch (this.State.RightButton) {
                    case ButtonState.Pressed:
                        this.RightMouseButtonPressed?.Invoke(this, new MouseEventArgs(MouseEventType.RightMouseButtonPressed));
                        break;
                    case ButtonState.Released:
                        this.RightMouseButtonReleased?.Invoke(this, new MouseEventArgs(MouseEventType.RightMouseButtonReleased));
                        break;
                }
            }

            // Handle mouse scroll
            if (this.State.ScrollWheelValue != 0) {
                this.MouseWheelScrolled?.Invoke(this, new MouseEventArgs(MouseEventType.MouseWheelScrolled));
            }
        }

        public void OnEnable() {
            /* NOOP */
        }

        public void OnDisable() {
            /* NOOP */
        }

        #region Events

        public event EventHandler<MouseEventArgs> MouseMoved;
        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseWheelScrolled;

        #endregion

    }

}