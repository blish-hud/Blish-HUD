using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class HotkeyAssignmentWindow : Container {

        private const int WINDOW_WIDTH = 371;
        private const int WINDOW_HEIGHT = 200;

        private Rectangle _hotkeyRegion = new Rectangle(60, 80, 225, 30);

        private Hotkey _hotkeyDefinition;

        public HotkeyAssignmentWindow(Hotkey hotkey) {
            _hotkeyDefinition = hotkey;

            this.Size = new Point(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.ZIndex = int.MaxValue - 2;

            Input.FocusedControl = this;

            BuildChildElements();
        }

        public override void TriggerKeyboardInput(KeyboardMessage e) {
            if (e.EventType == KeyboardEventType.KeyUp) return;

            _hotkeyDefinition.Keys.Clear();
            _hotkeyDefinition.Keys.AddRange(Input.KeysDown);

            Invalidate();
        }

        private void BuildChildElements() {
            var assignInputsLbl = new Label() {
                Text           = string.Format(Strings.GameServices.InputService.Hotkey_AssignInputsTo, _hotkeyDefinition.Name),
                Location       = new Point(40, 35),
                ShowShadow     = true,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = this
            };

            var unbindBttn = new StandardButton() {
                Text     = Strings.GameServices.InputService.Hotkey_Unbind,
                Location = new Point(275, 85),
                Width    = 70,
                Height   = 25,
                Parent   = this
            };

            var cancelBttn = new StandardButton() {
                Text     = Strings.Common.Action_Cancel,
                Location = new Point(275, 140),
                Width    = 70,
                Height   = 25,
                Parent   = this
            };

            var overwriteBttn = new StandardButton() {
                Text     = Strings.GameServices.InputService.Hotkey_Overwrite,
                Width    = 105,
                Height   = 25,
                Parent   = this
            };
            overwriteBttn.Location = new Point(cancelBttn.Left - 8 - overwriteBttn.Width, cancelBttn.Top);

            unbindBttn.LeftMouseButtonReleased += delegate {
                _hotkeyDefinition.Keys.Clear();
                Invalidate();
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(Content.GetTexture("hotkey-window"), bounds, Color.White);

            // Easy way to get a string representation of the hotkeys
            string hotkeyRep = string.Join(" + ", _hotkeyDefinition.Keys);

            Blish_HUD.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size16, ContentService.FontStyle.Regular), hotkeyRep, _hotkeyRegion.OffsetBy(1, 1), Color.Black, HorizontalAlignment.Left, VerticalAlignment.Middle);
            Blish_HUD.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size16, ContentService.FontStyle.Regular), hotkeyRep, _hotkeyRegion, Color.White, HorizontalAlignment.Left, VerticalAlignment.Middle);
        }

    }
}
