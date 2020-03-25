using System;
using Gw2Sharp.Models;
using Gw2Sharp.Mumble;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Mumble {
    public class UI {

        private readonly Gw2MumbleService _service;

        #region Events

        /// <summary>
        /// Fires when the user changes the compass size.
        /// </summary>
        public event EventHandler<ValueEventArgs<Size>>   CompassSizeChanged;

        /// <summary>
        /// Fires when the user toggles compass rotation.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>>   CompassRotationEnabledChanged;

        /// <summary>
        /// Fires when the user moves the compass between the top-right and the bottom-right.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>>   CompassTopRightChanged;

        /// <summary>
        /// Fires when the player selects a Guild Wars 2 text field (chat bar, search, etc.).
        /// </summary>
        public event EventHandler<EventArgs>              TextInputAcquiredFocus;

        /// <summary>
        /// Fires when the player unselects a Guild Wars 2 text field (chat bar, search, etc.).
        /// </summary>
        public event EventHandler<EventArgs>              TextInputLostFocus;

        /// <summary>
        /// Fires when the world map is opened.
        /// </summary>
        public event EventHandler<EventArgs>              MapOpened;

        /// <summary>
        /// Fires when the world map is closed.
        /// </summary>
        public event EventHandler<EventArgs>              MapClosed;

        /// <summary>
        /// Fires when the user changes their in-game interface size.
        /// </summary>
        public event EventHandler<ValueEventArgs<UiSize>> UISizeChanged;

        private void OnCompassSizeChanged(ValueEventArgs<Size>            e) => CompassSizeChanged?.Invoke(this, e);
        private void OnCompassRotationEnabledChanged(ValueEventArgs<bool> e) => CompassRotationEnabledChanged?.Invoke(this, e);
        private void OnCompassTopRightChanged(ValueEventArgs<bool>        e) => CompassTopRightChanged?.Invoke(this, e);
        private void OnTextInputAcquiredFocus(EventArgs                   e) => TextInputAcquiredFocus?.Invoke(this, e);
        private void OnTextInputLostFocus(EventArgs                       e) => TextInputLostFocus?.Invoke(this, e);
        private void OnMapOpened(EventArgs                                e) => MapOpened?.Invoke(this, e);
        private void OnMapClosed(EventArgs                                e) => MapClosed?.Invoke(this, e);
        private void OnUISizeChanged(ValueEventArgs<UiSize>               e) => UISizeChanged?.Invoke(this, e);

        private Size   _prevCompassSize              = new Size(1, 1);
        private bool   _prevIsCompassRotationEnabled = false;
        private bool   _prevIsCompassTopRight        = true;
        private bool   _prevIsTextInputFocused       = false;
        private bool   _isMapOpen                    = false;
        private UiSize _prevUiSize                   = UiSize.Normal;

        private void HandleEvents() {
            if (_prevCompassSize != this.CompassSize) {
                _prevCompassSize = this.CompassSize;
                OnCompassSizeChanged(new ValueEventArgs<Size>(_prevCompassSize));
            }

            if (_prevIsCompassRotationEnabled != this.IsCompassRotationEnabled) {
                _prevIsCompassRotationEnabled = this.IsCompassRotationEnabled;
                OnCompassRotationEnabledChanged(new ValueEventArgs<bool>(_prevIsCompassRotationEnabled));
            }

            if (_prevIsCompassTopRight != this.IsCompassTopRight) {
                _prevIsCompassTopRight = this.IsCompassTopRight;
                OnCompassTopRightChanged(new ValueEventArgs<bool>(_prevIsCompassTopRight));
            }

            if (_prevIsTextInputFocused != this.IsTextInputFocused) {
                _prevIsTextInputFocused = this.IsTextInputFocused;

                if (_prevIsTextInputFocused) {
                    OnTextInputAcquiredFocus(EventArgs.Empty);
                } else {
                    OnTextInputLostFocus(EventArgs.Empty);
                }
            }

            if (_isMapOpen != this.IsMapOpen) {
                _isMapOpen = this.IsMapOpen;

                if (_isMapOpen) {
                    OnMapOpened(EventArgs.Empty);
                } else {
                    OnMapClosed(EventArgs.Empty);
                }
            }

            if (_prevUiSize != this.UISize) {
                _prevUiSize = this.UISize;
                OnUISizeChanged(new ValueEventArgs<UiSize>(_prevUiSize));
            }
        }

        #endregion

        /// <inheritdoc cref="IGw2MumbleClient.Compass"/>
        public Size CompassSize => _service.RawClient.Compass;

        /// <inheritdoc cref="IGw2MumbleClient.CompassRotation"/>
        public double CompassRotation => _service.RawClient.CompassRotation;

        /// <inheritdoc cref="IGw2MumbleClient.IsCompassRotationEnabled"/>
        public bool IsCompassRotationEnabled => _service.RawClient.IsCompassRotationEnabled;

        /// <inheritdoc cref="IGw2MumbleClient.IsCompassTopRight"/>
        public bool IsCompassTopRight => _service.RawClient.IsCompassTopRight;

        /// <inheritdoc cref="IGw2MumbleClient.IsMapOpen"/>
        public bool IsMapOpen => _service.RawClient.IsMapOpen;

        /// <inheritdoc cref="IGw2MumbleClient.MapCenter"/>
        public Coordinates2 MapCenter => _service.RawClient.MapCenter;

        /// <inheritdoc cref="IGw2MumbleClient.MapScale"/>
        public double MapScale => _service.RawClient.MapScale;

        /// <inheritdoc cref="IGw2MumbleClient.PlayerLocationMap"/>
        public Coordinates2 MapPosition => _service.RawClient.PlayerLocationMap;

        /// <inheritdoc cref="IGw2MumbleClient.DoesAnyInputHaveFocus"/>
        public bool IsTextInputFocused => _service.RawClient.DoesAnyInputHaveFocus;

        /// <inheritdoc cref="IGw2MumbleClient.UiSize"/>
        public UiSize UISize => _service.RawClient.UiSize;

        internal UI(Gw2MumbleService service) {
            _service = service;
        }

        internal void Update(GameTime gameTime) {
            HandleEvents();
        }

    }
}
