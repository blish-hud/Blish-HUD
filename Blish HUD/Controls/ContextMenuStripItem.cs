using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public class ContextMenuStripItem : ScrollingButton {

        private const int BULLET_SIZE        = 18;
        private const int HORIZONTAL_PADDING = 6;

        private const int TEXT_LEFTPADDING = HORIZONTAL_PADDING + BULLET_SIZE + HORIZONTAL_PADDING;

        private static Texture2D _bulletSprite;

        private string _text;
        public string Text {
            get => _text;
            set {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged();
            }
        }

        public ContextMenuStripItem() {
            _bulletSprite = _bulletSprite ?? Content.GetTexture("155038");
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(_bulletSprite,
                             new Rectangle(HORIZONTAL_PADDING,
                                           bounds.Height / 2 - BULLET_SIZE / 2,
                                           BULLET_SIZE,
                                           BULLET_SIZE),
                             this.MouseOver ? Color.FromNonPremultiplied(255, 228, 181, 255) : Color.White);

            // Draw shadow
            Utils.DrawUtil.DrawAlignedText(spriteBatch,
                                           Content.DefaultFont14,
                                           this.Text,
                                           new Rectangle(TEXT_LEFTPADDING              + 1,
                                                         0                             + 1,
                                                         this.Width - TEXT_LEFTPADDING - HORIZONTAL_PADDING,
                                                         this.Height),
                                           Color.Black);

            Utils.DrawUtil.DrawAlignedText(spriteBatch,
                                           Content.DefaultFont14,
                                           this.Text,
                                           new Rectangle(TEXT_LEFTPADDING,
                                                         0,
                                                         this.Width - TEXT_LEFTPADDING - HORIZONTAL_PADDING,
                                                         this.Height),
                                           this.Enabled ? Color.White : Color.DarkGray);
        }

    }

}