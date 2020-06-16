using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    /// <summary>
    /// Allows for actions to be ran as the result of a provided key combination.
    /// </summary>
    public class KeyBinding {

        /// <summary>
        /// Fires when the <see cref="KeyBinding"/> is triggered.
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        protected void OnActivated(EventArgs e) {
            Activated?.Invoke(this, e);
        }

        /// <summary>
        /// The primary key in the binding.
        /// </summary>
        [JsonProperty]
        public Keys PrimaryKey { get; set; }

        /// <summary>
        /// Any combination of <see cref="ModifierKeys"/> required to be pressed
        /// in addition to the <see cref="PrimaryKey"/> for the <see cref="KeyBinding"/> to fire.
        /// </summary>
        [JsonProperty]
        public ModifierKeys ModifierKeys { get; set; }

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
                    if (this.PrimaryKey == Keys.None && value) return;

                    if (value) {
                        GameService.Input.Keyboard.KeyStateChanged += KeyboardOnKeyStateChanged;
                    } else {
                        GameService.Input.Keyboard.KeyStateChanged -= KeyboardOnKeyStateChanged;
                    }

                    _enabled = value;

                    Reset();
                }
            }
        }

        private bool _isTriggering;

        public KeyBinding() { /* NOOP */ }

        public KeyBinding(Keys primaryKey) : this(ModifierKeys.None, primaryKey) { /* NOOP */ }

        public KeyBinding(ModifierKeys modifierKeys, Keys primaryKey) {
            this.ModifierKeys = modifierKeys;
            this.PrimaryKey   = primaryKey;
        }

        private void KeyboardOnKeyStateChanged(object sender, KeyboardEventArgs e) {
            CheckTrigger(GameService.Input.Keyboard.ActiveModifiers, GameService.Input.Keyboard.KeysDown);
        }

        private void Reset() {
            StopFiring();
        }

        private void Fire() {
            if (_isTriggering) return;

            _isTriggering = true;

            ManuallyTrigger();
        }

        private void StopFiring() {
            _isTriggering = false;
        }

        private void CheckTrigger(ModifierKeys activeModifiers, IEnumerable<Keys> pressedKeys) {
            if ((this.ModifierKeys & activeModifiers) == this.ModifierKeys && pressedKeys.Contains(this.PrimaryKey)) {
                Fire();
            } else if (_isTriggering) {
                StopFiring();
            }
        }

        /// <summary>
        /// Gets a display string representing the <see cref="KeyBinding"/> suitable
        /// for display in the UI.
        /// </summary>
        public string GetBindingDisplayText() => KeysUtil.GetFriendlyName(this.ModifierKeys, this.PrimaryKey);

        /// <summary>
        /// Manually triggers the actions bound to this <see cref="KeyBinding"/>.
        /// </summary>
        public void ManuallyTrigger() {
            OnActivated(EventArgs.Empty);
        }

    }

}
