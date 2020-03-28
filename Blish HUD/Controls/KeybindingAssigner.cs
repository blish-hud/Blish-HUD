using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {
    public class KeybindingAssigner : LabelBase {

        private const int UNIVERSAL_PADDING = 2;

        /// <summary>
        /// Fires when the keybinding on the assigned <see cref="KeyBinding"/> is updated
        /// through this control.
        /// </summary>
        public event EventHandler<EventArgs> BindingChanged;

        protected void OnBindingChanged(EventArgs e) {
            this.BindingChanged?.Invoke(this, e);
        }

        private int _nameWidth = 183;

        /// <summary>
        /// The width of the name area of the <see cref="KeybindingAssigner"/>.
        /// The remaining width of the control is used to show the key binding.
        /// </summary>
        public int NameWidth {
            get => _nameWidth;
            set => SetProperty(ref _nameWidth, value, true);
        }

        private KeyBinding _keyBinding;

        /// <summary>
        /// The name shown as the key binding name.
        /// </summary>
        public string KeyBindingName {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private Rectangle _nameRegion;
        private Rectangle _hotkeyRegion;

        private bool _overHotkey;

        public KeyBinding KeyBinding {
            get => _keyBinding;
            set => SetProperty(ref _keyBinding, value);
        }

        public KeybindingAssigner(KeyBinding keyBinding) {
            _keyBinding = keyBinding;

            // Configure LabelBase
            _font       = Content.DefaultFont14;
            _showShadow = true;
            _cacheLabel = false;

            this.Size = new Point(340, 16);
        }

        protected override void OnClick(MouseEventArgs e) {
            if (_overHotkey && e.IsDoubleClick) {
                SetupNewAssignmentWindow();
            }

            base.OnClick(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            _overHotkey = this.RelativeMousePosition.X >= _hotkeyRegion.Left;

            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            _overHotkey = false;

            base.OnMouseLeft(e);
        }

        public override void RecalculateLayout() {
            _nameRegion   = new Rectangle(0,                              0, _nameWidth,                               _size.Y);
            _hotkeyRegion = new Rectangle(_nameWidth + UNIVERSAL_PADDING, 0, _size.X - _nameWidth - UNIVERSAL_PADDING, _size.Y);
        }

        private void SetupNewAssignmentWindow() {
            var newHkAssign = new KeybindingAssignmentWindow(_text, _keyBinding.ModifierKeys, _keyBinding.PrimaryKey) {
                Parent = Graphics.SpriteScreen
            };

            newHkAssign.AssignmentAccepted += delegate {
                _keyBinding.ModifierKeys = newHkAssign.ModifierKeys;
                _keyBinding.PrimaryKey   = newHkAssign.PrimaryKey;

                OnBindingChanged(EventArgs.Empty);
            };

            newHkAssign.Show();
        }

        protected override CaptureType CapturesInput() { return CaptureType.Filter | CaptureType.Mouse; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw white panel for keybinding name
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   _nameRegion,
                                   Color.White * 0.15f);

            // Draw keybinding name
            DrawText(spriteBatch, _nameRegion);

            // Draw white panel for hotkey
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   _hotkeyRegion,
                                   Color.White * (_overHotkey ? 0.20f : 0.15f));

            // Draw keybind string
            spriteBatch.DrawStringOnCtrl(this,
                                         _keyBinding.GetBindingDisplayText(),
                                         Content.DefaultFont14,
                                         _hotkeyRegion.OffsetBy(1, 1),
                                         Color.Black,
                                         false,
                                         HorizontalAlignment.Center);

            spriteBatch.DrawStringOnCtrl(this,
                                         _keyBinding.GetBindingDisplayText(),
                                         Content.DefaultFont14,
                                         _hotkeyRegion,
                                         Color.White,
                                         false,
                                         HorizontalAlignment.Center);
        }

    }
}
