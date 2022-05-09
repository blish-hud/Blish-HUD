using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {
    public class KeybindingAssignmentWindow : Container, IWindow {

        #region Load Static

        private static readonly Texture2D _textureWindowTexture = Content.GetTexture("hotkey-window");

        #endregion

        #region Events

        /// <summary>
        /// Fires when the "Accept" button is pressed within the assignment window.
        /// Indicates that the assignment was accepted and the resulting primary and
        /// modifier keys should update their target.
        /// </summary>
        public event EventHandler<EventArgs> AssignmentAccepted;

        /// <summary>
        /// Fires when the "Cancel" button or escape is pressed within the assignment window.
        /// Indicates that the assignment was canceled and the resulting primary and modifier
        /// keys should be ignored.
        /// </summary>
        public event EventHandler<EventArgs> AssignmentCanceled;

        private void OnAssignmentAccepted(EventArgs e) {
            this.AssignmentAccepted?.Invoke(this, e);

            FinishAssignment();
        }

        private void OnAssignmentCanceled(EventArgs e) {
            this.AssignmentCanceled?.Invoke(this, e);

            FinishAssignment();
        }

        #endregion

        private readonly Rectangle _normalizedHotkeyRegion = new Rectangle(60, 80, 225, 30);
        private readonly Rectangle _normalizedWindowRegion = new Rectangle(0,  0,  371, 200);

        private ModifierKeys _modifierKeys;
        private Keys         _primaryKey;

        /// <summary>
        /// The current modifier key(s) assignment.
        /// </summary>
        public ModifierKeys ModifierKeys {
            get => _modifierKeys;
            private set => SetProperty(ref _modifierKeys, value, true);
        }

        /// <summary>
        /// The current primary key assignment.
        /// </summary>
        public Keys PrimaryKey {
            get => _primaryKey;
            private set => SetProperty(ref _primaryKey, value, true);
        }

        private readonly string _assignmentName;

        private string _assignmentDisplayString;

        public KeybindingAssignmentWindow(string assignmentName, ModifierKeys modifierKeys = ModifierKeys.None, Keys primaryKey = Keys.None) {
            _assignmentName = assignmentName;
            _modifierKeys   = modifierKeys;
            _primaryKey     = primaryKey;

            this.BackgroundColor = Color.Black * 0.3f;
            this.Size            = new Point(_normalizedWindowRegion.Width, _normalizedWindowRegion.Height);
            this.ZIndex          = int.MaxValue - 2;
            this.Visible         = false;

            BuildChildElements();

            Input.Keyboard.KeyStateChanged += KeyboardOnKeyStateChanged;
        }

        protected override void OnShown(EventArgs e) {
            Invalidate();

            GameService.Input.Keyboard.SetTextInputListner(BlockGameInput);

            base.OnShown(e);
        }

        private void BlockGameInput(string input) { /* NOOP */ }

        private void KeyboardOnKeyStateChanged(object sender, KeyboardEventArgs e) {
            if (e.Key == Keys.Escape) {
                if (e.EventType == KeyboardEventType.KeyUp) {
                    OnAssignmentCanceled(EventArgs.Empty);
                }

                return;
            }

            if (e.EventType == KeyboardEventType.KeyDown) {
                (this.ModifierKeys, this.PrimaryKey) = KeysUtil.SplitToBindingPair(Input.Keyboard.KeysDown);
            }
        }

        private void FinishAssignment() {
            GameService.Input.Keyboard.UnsetTextInputListner(BlockGameInput);

            this.Dispose();
        }

        private StandardButton _acceptBttn;
        private StandardButton _unbindBttn;
        private StandardButton _cancelBttn;

        private void BuildChildElements() {
            var assignInputsLbl = new Label() {
                Text           = string.Format(Strings.GameServices.InputService.Hotkey_AssignInputsTo, _assignmentName),
                Location       = new Point(40, 35),
                ShowShadow     = true,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = this
            };

            _unbindBttn = new StandardButton() {
                Text     = Strings.GameServices.InputService.Hotkey_Unbind,
                Location = new Point(275, 85),
                Width    = 70,
                Height   = 25,
                Parent   = this
            };

            _cancelBttn = new StandardButton() {
                Text     = Strings.Common.Action_Cancel,
                Location = new Point(275, 140),
                Width    = 70,
                Height   = 25,
                Parent   = this
            };

            _acceptBttn = new StandardButton() {
                Text   = Strings.Common.Action_Accept,
                Width  = 105,
                Height = 25,
                Parent = this
            };
            _acceptBttn.Location = new Point(_cancelBttn.Left - 8 - _acceptBttn.Width, _cancelBttn.Top);

            _unbindBttn.Click += delegate {
                this.ModifierKeys = ModifierKeys.None;
                this.PrimaryKey   = Keys.None;
            };

            _cancelBttn.Click += delegate {
                OnAssignmentCanceled(EventArgs.Empty);
            };

            _acceptBttn.Click += delegate {
                OnAssignmentAccepted(EventArgs.Empty);
            };
        }

        private Rectangle _hotkeyRegion;
        private Rectangle _windowRegion;

        public override void RecalculateLayout() {
            base.RecalculateLayout();

            if (this.Parent != null) {
                _size = this.Parent.Size;

                var distanceInwards = new Point(_size.X / 2 - _normalizedWindowRegion.Width  / 2,
                                                _size.Y / 2 - _normalizedWindowRegion.Height / 2);

                _hotkeyRegion = _normalizedHotkeyRegion.OffsetBy(distanceInwards);
                _windowRegion = _normalizedWindowRegion.OffsetBy(distanceInwards);

                this.ContentRegion = _windowRegion;
            }

            _assignmentDisplayString = KeysUtil.GetFriendlyName(_modifierKeys, _primaryKey);

            if (_unbindBttn != null) {
                _unbindBttn.Enabled = this.PrimaryKey != Keys.None;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureWindowTexture, _windowRegion);

            spriteBatch.DrawStringOnCtrl(this, _assignmentDisplayString, Content.DefaultFont16, _hotkeyRegion.OffsetBy(1, 1), Color.Black);
            spriteBatch.DrawStringOnCtrl(this, _assignmentDisplayString, Content.DefaultFont16, _hotkeyRegion,                Color.White);
        }

        protected override void DisposeControl() {
            base.DisposeControl();

            GameService.Input.Keyboard.UnsetTextInputListner(BlockGameInput);

            Input.Keyboard.KeyStateChanged -= KeyboardOnKeyStateChanged;
        }

        // We implement IWindow to avoid other windows from reacting to our ESC input

        public bool   TopMost              => true;
        public double LastInteraction      => double.MaxValue;
        public bool   CanClose             => false;
        public bool   CanCloseWithEscape   => false;
        public void   BringWindowToFront() { /* NOOP */ }

    }
}
