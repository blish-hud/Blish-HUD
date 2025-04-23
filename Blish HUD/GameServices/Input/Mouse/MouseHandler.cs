using System;
using System.Windows.Forms;
using Blish_HUD.Controls;
using Blish_HUD.Input.WinApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Control = Blish_HUD.Controls.Control;

namespace Blish_HUD.Input {

    public class MouseHandler : IInputHandler {

        private static readonly Logger Logger = Logger.GetLogger<MouseHandler>();

        /// <summary>
        /// The current position of the mouse relative to the application.
        /// </summary>
        public Point Position => this.State.Position;

        public Point PositionRaw { get; private set; }

        /// <summary>
        /// The current state of the mouse.
        /// </summary>
        public MouseState State { get; private set; }

        /// <summary>
        /// Indicates if the camera is being dragged.
        /// </summary>
        public bool CameraDragging { get; private set; }

        private Control _activeControl;
        /// <summary>
        /// The <see cref="Controls.Control" /> that the mouse last moved over.
        /// </summary>
        public Control ActiveControl {
            get => _activeControl;
            private set {
                _hudFocused    = value != null;
                _activeControl = value;

                Control.ActiveControl = value;
            }
        }

        private bool _cursorIsVisible = true;

        private bool _recentlyEnabled = false;

        /// <summary>
        /// Indicates if the hardware mouse is currently visible.  When <c>false</c>,
        /// this typically indicates that the user is rotating their camera or in action
        /// camera mode.
        /// </summary>
        public bool CursorIsVisible {
            get => _cursorIsVisible;
            set {
                if (_cursorIsVisible == value) return;

                if (!value) {
                    this.ActiveControl = null;
                }

                _cursorIsVisible = value;
            }
        }

        private bool           _hudFocused;
        private MouseEventArgs _mouseEvent;

        internal MouseHandler() { }

        public bool HandleInput(MouseEventArgs mouseEventArgs) {
            if (mouseEventArgs.EventType == MouseEventType.MouseMoved) {
                this.PositionRaw = new Point(mouseEventArgs.PointX, mouseEventArgs.PointY);
                return false;
            }

            var activeForm = Form.ActiveForm;

            if (activeForm != null && activeForm.ClientRectangle.Contains(new System.Drawing.Point(mouseEventArgs.PointX, mouseEventArgs.PointY))) {
                // If another form is active (like Debug, Pathing editor, etc.) don't intercept
                return false;
            }

            if (!_hudFocused) {
                this.CameraDragging = mouseEventArgs.EventType switch {
                    MouseEventType.RightMouseButtonPressed => true,
                    MouseEventType.RightMouseButtonReleased => false,
                    _ => this.CameraDragging
                };
            }

            if (this.CameraDragging || !this.CursorIsVisible) return false;
            
            _mouseEvent = mouseEventArgs;

            return mouseEventArgs.EventType != MouseEventType.LeftMouseButtonReleased             // Never block the users input if they are releasing the left mouse button
                && mouseEventArgs.EventType != MouseEventType.RightMouseButtonReleased            // Never block the users input if they are releasing the right mouse button
                && (_hudFocused && !this.ActiveControl.Captures.HasFlag(CaptureType.DoNotBlock)); // If no control, or if the current control has capture forced off, then do not block
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
            // We check if not hidden because we should act normally if cursor is just surpressed (e.g. from touch or pen input)
            this.CursorIsVisible = CursorExtern.GetCursorInfo().Flags != CursorFlags.CursorHiding;

            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || !GameService.GameIntegration.Gw2Instance.Gw2HasFocus || GameService.Overlay.InterfaceHidden) {
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
                if (this.CursorIsVisible) {
                    this.ActiveControl = GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseMoved, this.State);
                }

                this.MouseMoved?.Invoke(this, new MouseEventArgs(MouseEventType.MouseMoved));
            }

            // Handle mouse events blocked by the mouse hook
            if (_mouseEvent != null) {
                if(_recentlyEnabled) {
                    _recentlyEnabled = false;
                    SimulateNonCapturePressedEvent(_mouseEvent);
                }

                HandleMouseEvent(_mouseEvent);
                _mouseEvent = null;
            }
        }

        /// <summary>
        /// Meant to simulate missing parts of mouse events (LeftMouseButtonPressed and RightMouseButtonPressed) in case the mouse hook just got enabled. This was not an "issue" prior to a fix for issue #768 as e.g. the base control just always operated with the release event.
        /// Now that the event handling there requires a primed state setup by the respective pressed event this logic exists to simulate said event. This saves us from having to pass down a special case flag to the handlers and aims to keep behaviour consistent since
        /// there will always be a press and release event and not sometimes just half of it. 
        /// </summary>
        /// <param name="mouseEvent">Current mouse event that we need to simulate the missing part for given its one of the 2 cases we even want to handle</param>
        private void SimulateNonCapturePressedEvent(MouseEventArgs mouseEvent) {
            // What state keeping do we need to even mess with? (Considering public fields of this class accessed by other methods in the handler chain)
            // no need to worry about PositionRaw as it will be the same as the simulated event
            // no need to worry about CameraDragging as it will be the same as the simulated event
            // ActiveControl is not changed by the mouse event so we also dont have to worry about that one either (targets of the events may choose to alter this as necessary but that is normal behaviour then)

            // need to modify the State but it's basically just a copy with changed mouse button pressed states

            // we don't necessarily have to check the previous mouse state here as an additional security as this can only be happening on the enable / disable chain right now
            // -> the previous state may also be a dangling leftover so it should even be safer to ignore it

            var tmpState = this.State;
            if (mouseEvent.EventType == MouseEventType.LeftMouseButtonReleased) {
                this.State = new MouseState(tmpState.X,
                                            tmpState.Y,
                                            tmpState.ScrollWheelValue,
                                            Microsoft.Xna.Framework.Input.ButtonState.Pressed,
                                            tmpState.MiddleButton,
                                            tmpState.RightButton,
                                            tmpState.XButton1,
                                            tmpState.XButton2);
                // currently unsure if the MouseData and Flags contained any additional information about the button state maybe requires some bitmagic .. (both seem to always be 0 right now?)
                HandleMouseEvent(new MouseEventArgs(MouseEventType.LeftMouseButtonPressed, mouseEvent.PointX, mouseEvent.PointY, mouseEvent.MouseData, mouseEvent.Flags, mouseEvent.Time, mouseEvent.Extra));
            } else if (mouseEvent.EventType == MouseEventType.RightMouseButtonReleased) {
                this.State = new MouseState(tmpState.X,
                                            tmpState.Y,
                                            tmpState.ScrollWheelValue,
                                            tmpState.LeftButton,
                                            tmpState.MiddleButton,
                                            Microsoft.Xna.Framework.Input.ButtonState.Pressed,
                                            tmpState.XButton1,
                                            tmpState.XButton2);
                // currently unsure if the MouseData and Flags contained any additional information about the button state maybe requires some bitmagic .. (both seem to always be 0 right now?)
                HandleMouseEvent(new MouseEventArgs(MouseEventType.RightMouseButtonPressed, mouseEvent.PointX, mouseEvent.PointY, mouseEvent.MouseData, mouseEvent.Flags, mouseEvent.Time, mouseEvent.Extra));
            }
            this.State = tmpState;
        }

        private void HandleMouseEvent(MouseEventArgs mouseEvent) {
            if (HandleHookedMouseEvent(mouseEvent) && this.CursorIsVisible) {
                GameService.Graphics.SpriteScreen.TriggerMouseInput(mouseEvent.EventType, this.State);
            }
        }

        public void OnEnable() {
            _recentlyEnabled = true;
        }

        public void OnDisable() {
            // shouldn't be needed but can't hurt to tidy up a little
            _recentlyEnabled = false;
        }

        public void UnsetActiveControl() {
            this.ActiveControl = null;
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