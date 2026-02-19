using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Input.Mouse;

namespace Blish_HUD.Input {

    /// <summary>
    /// Allows for actions to be ran as the result of a provided key combination.
    /// </summary>
    public class KeyBinding {

        private static readonly HashSet<Keys> _acceptedPrimaryModifierKeys = new HashSet<Keys>() { Keys.LeftAlt, Keys.RightAlt, Keys.LeftControl, Keys.RightControl, Keys.LeftShift, Keys.RightShift };

        /// <summary>
        /// Fires when the keys of the <see cref="KeyBinding"/> are changed.
        /// </summary>
        public event EventHandler<EventArgs> BindingChanged; 

        /// <summary>
        /// Fires when the <see cref="KeyBinding"/> is triggered.
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        protected void OnActivated(EventArgs e) {
            Activated?.Invoke(this, e);
        }

        private Keys _primaryKey;
        /// <summary>
        /// The primary key in the binding.
        /// Mutually exclusive with <see cref="PrimaryMouseButton"/>.
        /// </summary>
        [JsonProperty]
        public Keys PrimaryKey { 
            get => _primaryKey;
            set {
                if (_primaryKey == value) {
                    return;
                }

                if (value != Keys.None) {
                    _primaryMouseButton = MouseButtons.None;
                }

                _primaryKey = value;
                BindingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private MouseButtons _primaryMouseButton;
        /// <summary>
        /// The primary mouse button in the binding.
        /// Mutually exclusive with <see cref="PrimaryKey"/>.
        /// </summary>
        [JsonProperty]
        public MouseButtons PrimaryMouseButton {
            get => _primaryMouseButton;
            set {
                if (_primaryMouseButton == value) {
                    return;
                }

                if (value != MouseButtons.None) {
                    _primaryKey = Keys.None;
                }

                _primaryMouseButton = value;
                this.BindingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ModifierKeys _modifierKeys;
        /// <summary>
        /// Any combination of <see cref="ModifierKeys"/> required to be pressed
        /// in addition to the <see cref="PrimaryKey"/> for the <see cref="KeyBinding"/> to fire.
        /// </summary>
        [JsonProperty]
        public ModifierKeys ModifierKeys {
            get => _modifierKeys;
            set {
                if (_modifierKeys == value) {
                    return;
                }

                _modifierKeys = value;
                BindingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _enabled;

        /// <summary>
        /// If <c>true</c>, the <see cref="KeyBinding"/> will be enabled and can be triggered by
        /// the specified key combinations.
        /// </summary>
        [JsonProperty]
        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled != value) {
                    if (value) {
                        KeyboardOnKeyStateChanged(null, null);
                        GameService.Input.Keyboard.KeyStateChanged += KeyboardOnKeyStateChanged;

                        GameService.Input.Mouse.MiddleMouseButtonPressed  += MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton1Pressed           += MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton2Pressed           += MouseButtonStateChanged;
                        GameService.Input.Mouse.MiddleMouseButtonReleased += MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton1Released          += MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton2Released          += MouseButtonStateChanged;
                    } else {
                        GameService.Input.Keyboard.KeyStateChanged -= KeyboardOnKeyStateChanged;

                        GameService.Input.Mouse.MiddleMouseButtonPressed  -= MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton1Pressed           -= MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton2Pressed           -= MouseButtonStateChanged;
                        GameService.Input.Mouse.MiddleMouseButtonReleased -= MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton1Released          -= MouseButtonStateChanged;
                        GameService.Input.Mouse.XButton2Released          -= MouseButtonStateChanged;

                        GameService.Input.Keyboard.UnstageKeyBinding(this);
                    }

                    _enabled = value;

                    Reset();
                }
            }
        }

        private void MouseButtonStateChanged(object sender, MouseEventArgs e) {
            KeyboardOnKeyStateChanged(null, null);
        }

        /// <summary>
        /// If <c>true</c>, the <see cref="PrimaryKey"/> is not sent to the game when it is
        /// the final key pressed in the keybinding sequence.
        /// </summary>
        [JsonIgnore]
        public bool BlockSequenceFromGw2 { get; set; } = false;
        
        /// <summary>
        /// Indicates if the <see cref="KeyBinding"/> is actively triggered.
        /// If triggered with <see cref="ManuallyTrigger"/>(), this
        /// will report <c>true</c> for only a single frame.
        /// </summary>
        [JsonIgnore]
        public bool IsTriggering { get; private set; }

        /// <summary>
        /// If <c>true</c>, then the <see cref="KeyBinding"/> will not trigger when the user is in an in-game or Blish HUD text field.
        /// </summary>
        public bool IgnoreWhenInTextField { get; set; } = true;

        public KeyBinding() { /* NOOP */ }

        public KeyBinding(Keys primaryKey) : this(ModifierKeys.None, primaryKey) { /* NOOP */ }

        public KeyBinding(MouseButtons primaryMouseButon) : this(ModifierKeys.None, primaryMouseButon) { /* NOOP */ }

        public KeyBinding(ModifierKeys modifierKeys, Keys primaryKey) {
            _modifierKeys = modifierKeys;
            _primaryKey   = primaryKey;
        }

        public KeyBinding(ModifierKeys modifierKeys, MouseButtons primaryMouseButton) {
            _modifierKeys = modifierKeys;
            _primaryMouseButton = primaryMouseButton;
        }

        private void KeyboardOnKeyStateChanged(object sender, KeyboardEventArgs e) {
            if ((this.PrimaryKey == Keys.None && this.PrimaryMouseButton == MouseButtons.None)
             || (this.IgnoreWhenInTextField   && GameService.Input.Keyboard.TextFieldIsActive())) return;

            CheckTrigger(GameService.Input.Keyboard.ActiveModifiers, GameService.Input.Keyboard.KeysDown);
        }

        private void Reset() {
            StopFiring();
        }

        private void Fire() {
            if (this.IsTriggering) return;

            this.IsTriggering = true;

            ManuallyTrigger();
        }

        private void StopFiring() {
            this.IsTriggering = false;
        }

        private bool MouseButtonActive() {
            return _primaryMouseButton switch {
                MouseButtons.MiddleButton => GameService.Input.Mouse.State.MiddleButton == ButtonState.Pressed,
                MouseButtons.XButton1     => GameService.Input.Mouse.State.XButton1     == ButtonState.Pressed,
                MouseButtons.XButton2     => GameService.Input.Mouse.State.XButton2     == ButtonState.Pressed,
                _                         => false
            };
        }

        private void CheckTrigger(ModifierKeys activeModifiers, IEnumerable<Keys> pressedKeys) {
            if (activeModifiers == this.ModifierKeys || _acceptedPrimaryModifierKeys.Contains(_primaryKey)) {
                if (this.BlockSequenceFromGw2) {
                    GameService.Input.Keyboard.StageKeyBinding(this);
                }

                if (pressedKeys.Contains(this.PrimaryKey) || MouseButtonActive()) {
                    Fire();
                    return;
                }
            } else {
                GameService.Input.Keyboard.UnstageKeyBinding(this);
            }

            StopFiring();
        }

        /// <summary>
        /// Gets a display string representing the <see cref="KeyBinding"/> suitable
        /// for display in the UI.
        /// </summary>
        public string GetBindingDisplayText() => KeysUtil.GetFriendlyName(this.ModifierKeys, this.PrimaryKey, this.PrimaryMouseButton);

        /// <summary>
        /// Manually triggers the actions bound to this <see cref="KeyBinding"/>.
        /// </summary>
        public void ManuallyTrigger() {
            OnActivated(EventArgs.Empty);
        }

    }

}
