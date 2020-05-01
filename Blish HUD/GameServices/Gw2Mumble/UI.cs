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
        public event EventHandler<ValueEventArgs<Size>> CompassSizeChanged;

        /// <summary>
        /// Fires when the user toggles compass rotation.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>> IsCompassRotationEnabledChanged;

        /// <summary>
        /// Fires when the user moves the compass between the top-right and the bottom-right.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>> IsCompassTopRightChanged;

        /// <summary>
        /// Fires when the player selects or deselects a Guild Wars 2 text field (chat bar, search, etc.).
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>> IsTextInputFocusedChanged;

        /// <summary>
        /// Fires when the world map is opened or closed.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>> IsMapOpenChanged;

        /// <summary>
        /// Fires when the user changes their in-game interface size.
        /// </summary>
        public event EventHandler<ValueEventArgs<UiSize>> UISizeChanged;

        private void OnCompassSizeChanged(ValueEventArgs<Size>              e) => CompassSizeChanged?.Invoke(this, e);
        private void OnIsCompassRotationEnabledChanged(ValueEventArgs<bool> e) => this.IsCompassRotationEnabledChanged?.Invoke(this, e);
        private void OnIsCompassTopRightChanged(ValueEventArgs<bool>        e) => this.IsCompassTopRightChanged?.Invoke(this, e);
        private void OnIsTextInputFocusedChanged(ValueEventArgs<bool>       e) => IsTextInputFocusedChanged?.Invoke(this, e);
        private void OnIsMapOpenChanged(ValueEventArgs<bool>                e) => IsMapOpenChanged?.Invoke(this, e);
        private void OnUISizeChanged(ValueEventArgs<UiSize>                 e) => UISizeChanged?.Invoke(this, e);

        private Size   _prevCompassSize              = new Size(1, 1);
        private bool   _prevIsCompassRotationEnabled = false;
        private bool   _prevIsCompassTopRight        = true;
        private bool   _prevIsTextInputFocused       = false;
        private bool   _prevIsMapOpen                = false;
        private UiSize _prevUiSize                   = UiSize.Normal;

        private void HandleEvents() {
            MumbleEventImpl.CheckAndHandleEvent(ref _prevCompassSize,              this.CompassSize,              OnCompassSizeChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevIsCompassRotationEnabled, this.IsCompassRotationEnabled, OnIsCompassRotationEnabledChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevIsCompassTopRight,        this.IsCompassTopRight,        OnIsCompassTopRightChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevIsTextInputFocused,       this.IsTextInputFocused,       OnIsTextInputFocusedChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevIsMapOpen,                this.IsMapOpen,                OnIsMapOpenChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevUiSize,                   this.UISize,                   OnUISizeChanged);
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
