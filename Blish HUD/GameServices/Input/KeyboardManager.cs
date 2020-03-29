using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using Blish_HUD.Input.WinApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    public class KeyboardManager : InputManager {

        #region Event Handling

        /// <summary>
        /// Occurs when a key is pressed down.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyPressed;

        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyReleased;

        /// <summary>
        /// Occurs when the state of any key changes.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyStateChanged;

        public event EventHandler<ValueEventArgs<string>> TextInput;

        private void OnTextInputAsync(ValueEventArgs<string> e) {
            this.TextInput?.Invoke(this, e);
        }

        private void OnKeyStateChanged(KeyboardEventArgs e) {
            if (e.EventType == KeyboardEventType.KeyDown) {
                this.KeyPressed?.Invoke(this, e);
            } else {
                this.KeyReleased?.Invoke(this, e);
            }

            this.KeyStateChanged?.Invoke(this, e);
        }

        #endregion

        // TODO: Block using a stack of contexts which can independently process incoming keyboard events
        private bool _hookGeneralBlock;

        /// <summary>
        /// The current state of the keyboard.
        /// </summary>
        public KeyboardState State { get; private set; }

        /// <summary>
        /// A <see cref="ModifierKeys"/> flag indicating the acting
        /// modifier keys (Ctrl, Alt, or Shift) being pressed.
        /// </summary>
        public ModifierKeys ActiveModifiers { get; private set; }

        private readonly ConcurrentQueue<KeyboardEventArgs> _inputBuffer;

        private readonly List<Keys> _keysDown;

        /// <summary>
        /// A list of keys currently being pressed down.
        /// </summary>
        public IReadOnlyList<Keys> KeysDown => _keysDown.AsReadOnly();

        private delegate void TextInputAsync(ValueEventArgs<string> e);

        private TextInputAsync _textInputCaller;

        internal KeyboardManager() : base(HookType.WH_KEYBOARD_LL) {
            _keysDown    = new List<Keys>();
            _inputBuffer = new ConcurrentQueue<KeyboardEventArgs>();

            _textInputCaller = OnTextInputAsync;
        }

        private void UpdateStates() {
            Keys[] downArray = _keysDown.ToArray();

            this.State           = new KeyboardState(downArray);
            this.ActiveModifiers = KeysUtil.ModifiersFromKeys(downArray);
        }

        internal override void Update() {
            while (_inputBuffer.TryDequeue(out var keyboardEvent)) {
                if (keyboardEvent.EventType == KeyboardEventType.KeyDown) {
                    // Avoid firing on held keys
                    if (_keysDown.Contains(keyboardEvent.Key)) {
                        continue;
                    }

                    _keysDown.Add(keyboardEvent.Key);
                } else {
                    _keysDown.Remove(keyboardEvent.Key);
                }

                UpdateStates();

                OnKeyStateChanged(keyboardEvent);
            }
        }

        protected override void OnDisable() {
            // Ensure that key states don't get stuck if the
            // application focus is lost while keys were down.

            Keys[] passingKeys = _keysDown.ToArray();
            _keysDown.Clear();

            UpdateStates();

            foreach (var key in passingKeys) {
                OnKeyStateChanged(new KeyboardEventArgs(KeyboardEventType.KeyUp, key));
            }
        }

        private void EndTextInputAsyncInvoke(IAsyncResult asyncResult) {
            _textInputCaller.EndInvoke(asyncResult);
        }

        private void ProcessInput(KeyboardEventType eventType, Keys key) {
            string chars = TypedInputUtil.VKCodeToString((uint)key, eventType == KeyboardEventType.KeyDown);

            _textInputCaller.BeginInvoke(new ValueEventArgs<string>(chars), EndTextInputAsyncInvoke, null);

            _inputBuffer.Enqueue(new KeyboardEventArgs(eventType, key));
        }

        protected override bool HandleNewInput(IntPtr wParam, IntPtr lParam) {
            if (_hookGeneralBlock)
                return true;

            var eventType = (KeyboardEventType)((uint)wParam % 2 + 256); // filter out SysKeyDown & SysKeyUp
            var key       = (Keys)Marshal.ReadInt32(lParam);

            ProcessInput(eventType, key);

            // TODO: Implement blocking based on the key that is pressed (for example: Key binding blocking the last pressed key)

            return false;
        }

    }
}
