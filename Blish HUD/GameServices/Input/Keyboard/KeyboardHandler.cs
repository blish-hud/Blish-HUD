using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    public class KeyboardHandler : IInputHandler {

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

        private void OnKeyStateChanged(KeyboardEventArgs e) {
            if (e.EventType == KeyboardEventType.KeyDown)
                this.KeyPressed?.Invoke(this, e);
            else
                this.KeyReleased?.Invoke(this, e);

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

        private readonly ConcurrentQueue<KeyboardEventArgs> _inputBuffer = new ConcurrentQueue<KeyboardEventArgs>();

        private readonly List<Keys> _keysDown = new List<Keys>();

        // Keys which, when pressed, should never be captured exclusively by the keyboard hook
        // TODO: implement a more elegant way to not capture global hotkeys
        private readonly HashSet<Keys> _hookIgnoredKeys = new HashSet<Keys>() {
            Keys.NumLock,
            Keys.CapsLock,
            Keys.LeftWindows,
            Keys.RightWindows,
            Keys.LeftControl,
            Keys.RightControl,
            Keys.LeftAlt,
            Keys.RightAlt,
            Keys.LeftShift,
            Keys.RightShift,
            Keys.Tab
        };

        /// <summary>
        /// A list of keys currently being pressed down.
        /// </summary>
        public IReadOnlyList<Keys> KeysDown => _keysDown.AsReadOnly();
        
        private Action<string> _textInputDelegate;

        internal KeyboardHandler() { }

        public void Update() {
            while (_inputBuffer.TryDequeue(out KeyboardEventArgs keyboardEvent)) {
                if (keyboardEvent.EventType == KeyboardEventType.KeyDown) {
                    // Avoid firing on held keys
                    if (_keysDown.Contains(keyboardEvent.Key)) continue;

                    _keysDown.Add(keyboardEvent.Key);
                } else
                    _keysDown.Remove(keyboardEvent.Key);

                UpdateStates();

                OnKeyStateChanged(keyboardEvent);
            }
        }

        public void OnEnable() {
            /* NOOP */
        }

        public void OnDisable() {
            // Ensure that key states don't get stuck if the
            // application focus is lost while keys were down.

            Keys[] passingKeys = _keysDown.ToArray();
            _keysDown.Clear();

            UpdateStates();

            foreach (Keys key in passingKeys) OnKeyStateChanged(new KeyboardEventArgs(KeyboardEventType.KeyUp, key));
        }

        public bool HandleInput(KeyboardEventArgs e) {
            if (_hookGeneralBlock) return true;

            return ProcessInput(e.EventType, e.Key);
        }

        public void SetTextInputListner(Action<string> input) { _textInputDelegate = input; }

        public void UnsetTextInputListner(Action<string> input) {
            if (input == _textInputDelegate) _textInputDelegate = null;
        }

        private void UpdateStates() {
            Keys[] downArray = _keysDown.ToArray();

            this.State           = new KeyboardState(downArray);
            this.ActiveModifiers = KeysUtil.ModifiersFromKeys(downArray);
        }

        private void EndTextInputAsyncInvoke(IAsyncResult asyncResult) { _textInputDelegate?.EndInvoke(asyncResult); }

        private bool ShouldBlockKeyEvent(Keys key) {
            // TODO: WIN key combinations should probably completely handled by the OS

            // Skip keys that we wish to explicitly ignore
            if (_hookIgnoredKeys.Contains(key)) return false;

            return true;
        }

        private bool ProcessInput(KeyboardEventType eventType, Keys key) {
            _inputBuffer.Enqueue(new KeyboardEventArgs(eventType, key));

            // Handle the escape key, which should close the active window or top level context menu (if any)
            if (key == Keys.Escape) {
                var activeContextMenu = GameService.Graphics.SpriteScreen.Children
                   .OfType<ContextMenuStrip>().FirstOrDefault(c => c.Visible);

                if (activeContextMenu != null) { 
                    // If we found an active context menu item, close it
                    activeContextMenu.Hide();
                    return true;
                } else {
                    // If we found an active context menu item, close it
                    var activeWindow = WindowBase2.ActiveWindow;

                    if (activeWindow != null && activeWindow.CanClose) {
                        activeWindow.Hide();
                        return true;
                    }
                }
            }

            // Handle text input
            if (_textInputDelegate != null) {
                string chars = TypedInputUtil.VkCodeToString((uint)key, eventType == KeyboardEventType.KeyDown);
                _textInputDelegate?.BeginInvoke(chars, EndTextInputAsyncInvoke, null);
                return ShouldBlockKeyEvent(key);
            }

            // TODO: Implement blocking based on the key that is pressed (for example: Key binding blocking the last pressed key)

            return false;
        }

    }

}
