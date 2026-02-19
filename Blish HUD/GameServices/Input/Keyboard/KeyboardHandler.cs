using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    public class KeyboardHandler : IInputHandler {

        private static readonly Logger Logger = Logger.GetLogger<KeyboardHandler>();

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

        public Control FocusedControl {
            get => _focusedControl;
            set {
                _focusedControl = value;

                Control.FocusedControl = value;
            }
        }

        private Control _focusedControl;

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

        private readonly ReaderWriterLockSlim _stagedKeyBindingLock = new ReaderWriterLockSlim();
        private readonly HashSet<KeyBinding>  _stagedKeyBindings    = new HashSet<KeyBinding>();

        internal KeyboardHandler() { }

        public void Update() {
            if (GameService.Input.Mouse.ActiveControl != null) {
                foreach (var ancestor in GameService.Input.Mouse.ActiveControl.GetAncestors()) {
                    if (ancestor.Visible == false) {
                        GameService.Input.Mouse.ActiveControl.UnsetFocus();
                        GameService.Input.Mouse.UnsetActiveControl();
                    }
                }
            }

            if (FocusedControl != null) {
                foreach (var ancestor in FocusedControl.GetAncestors()) {
                    if (ancestor.Visible == false) {
                        FocusedControl.UnsetFocus();
                    }
                }
            }

            while (_inputBuffer.TryDequeue(out KeyboardEventArgs keyboardEvent)) {
                if (keyboardEvent.EventType == KeyboardEventType.KeyDown) {
                    // Avoid firing on held keys
                    if (_keysDown.Contains(keyboardEvent.Key)) continue;

                    _keysDown.Add(keyboardEvent.Key);
                } else {
                    _keysDown.Remove(keyboardEvent.Key);
                }

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

        /// <summary>
        /// Returns <c>true</c> if either an in-game Textbox or Blish HUD text field is active.
        /// </summary>
        public bool TextFieldIsActive() {
            return (GameService.Gw2Mumble.IsAvailable && GameService.Gw2Mumble.UI.IsTextInputFocused)
                || _textInputDelegate != null;
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

        private bool ShouldBlockKeyEvent(Keys key) {
            // TODO: WIN key combinations should probably completely handled by the OS

            // Skip keys that we wish to explicitly ignore
            if (_hookIgnoredKeys.Contains(key)) return false;

            return true;
        }

        internal void StageKeyBinding(KeyBinding keyBinding) {
            _stagedKeyBindingLock.EnterWriteLock();
            if (!_stagedKeyBindings.Contains(keyBinding)) {
                Logger.Debug("Staging keybind {keybind}.", keyBinding.GetBindingDisplayText());
                _stagedKeyBindings.Add(keyBinding);
            }
            _stagedKeyBindingLock.ExitWriteLock();
        }

        internal void UnstageKeyBinding(KeyBinding keyBinding) {
            _stagedKeyBindingLock.EnterWriteLock();
            if (_stagedKeyBindings.Contains(keyBinding)) {
                Logger.Debug("Unstaging keybind {keybind}.", keyBinding.GetBindingDisplayText());
                _stagedKeyBindings.Remove(keyBinding);
            }
            _stagedKeyBindingLock.ExitWriteLock();
        }

        private bool ProcessInput(KeyboardEventType eventType, Keys key) {
            _inputBuffer.Enqueue(new KeyboardEventArgs(eventType, key));

            if (GameService.Overlay.InterfaceHidden) return false;

            if (GameService.Gw2Mumble.IsAvailable && GameService.Gw2Mumble.UI.IsTextInputFocused) return false;

            // Handle the escape key
            if (key == Keys.Escape && eventType == KeyboardEventType.KeyDown) {
                // Loose focus on input fields
                if (FocusedControl != null) {
                    FocusedControl.UnsetFocus();
                    return true;
                }

                // Close the active window or top level context menu (if any) if enabled in settings
                if (GameService.Overlay.CloseWindowOnEscape.Value) {
                    var activeContextMenu = GameService.Graphics.SpriteScreen
                        .GetChildrenOfType<ContextMenuStrip>().FirstOrDefault(c => c.Visible);

                    if (activeContextMenu != null) {
                        // If we found an active context menu item, close it
                        activeContextMenu.Hide();
                        return true;
                    } else {
                        // If we found an active window, close it
                        var activeWindow = WindowBase2.ActiveWindow;

                        if (activeWindow != null && activeWindow.CanClose && activeWindow.CanCloseWithEscape) {
                            activeWindow.Hide();
                            return true;
                        }
                    }
                }
            }

            // Handle text input
            if (_textInputDelegate != null) {
                string chars = TypedInputUtil.VkCodeToString((uint)key, eventType == KeyboardEventType.KeyDown);
                _textInputDelegate?.Invoke(chars);
                return ShouldBlockKeyEvent(key);
            }

            // We don't want to risk holding up the api response.  Better to
            // accidentally send a key to the game than to lag the users input.
            if (_stagedKeyBindingLock.TryEnterReadLock(0)) {
                foreach (var keyBinding in _stagedKeyBindings) {
                    if (keyBinding.PrimaryKey == key) {
                        _stagedKeyBindingLock.ExitReadLock();
                        return true;
                    }
                }

                _stagedKeyBindingLock.ExitReadLock();
            }

            return false;
        }

    }

}
