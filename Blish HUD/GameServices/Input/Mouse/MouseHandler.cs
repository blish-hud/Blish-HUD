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
        ///     Indicates if the camera is being dragged.
        /// </summary>
        public bool CameraDragging { get; private set; }

        /// <summary>
        ///     The <see cref="Control" /> that the mouse last moved over.
        /// </summary>
        public Control ActiveControl {
            get => _activeControl;
            private set {
                _hudFocused    = value != null;
                _activeControl = value;

                Control.ActiveControl = value;
            }
        }

        private Control _activeControl;

        private bool _hudFocused;

        private MouseEventArgs _mouseEvent;

        internal MouseHandler() { }

        public bool HandleInput(MouseEventArgs mouseEventArgs) {
            if (mouseEventArgs.EventType == MouseEventType.MouseMoved) {
                this.PositionRaw = new Point(mouseEventArgs.PointX, mouseEventArgs.PointY);
                return false;
            }

            if (_hudFocused) {
                _mouseEvent = mouseEventArgs;

                return mouseEventArgs.EventType != MouseEventType.LeftMouseButtonReleased  // Never block the users input if they are releasing the left mouse button
                    && mouseEventArgs.EventType != MouseEventType.RightMouseButtonReleased // Never block the users input if they are releasing the right mouse button
                    && !this.ActiveControl.Captures.HasFlag(CaptureType.DoNotBlock);       // If the current control has capture forced off, then do not block
            }

            this.CameraDragging = mouseEventArgs.EventType switch {
                MouseEventType.RightMouseButtonPressed => true,
                MouseEventType.RightMouseButtonReleased => false,
                _ => this.CameraDragging
            };

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

            if (CameraDragging) {
                return;
            }

            var prevMouseState = this.State;

            var rawMouseState = Mouse.GetState();

            this.State = new MouseState((int) (rawMouseState.X / GameService.Graphics.UIScaleMultiplier),
                                        (int) (rawMouseState.Y / GameService.Graphics.UIScaleMultiplier),
                                        _mouseEvent?.WheelDelta ?? 0, 
                                        rawMouseState.LeftButton,
                                        rawMouseState.MiddleButton,
                                        rawMouseState.RightButton,
                                        rawMouseState.XButton1,
                                        rawMouseState.XButton2);

            // Handle mouse moved
            if (prevMouseState.Position != this.State.Position) {
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