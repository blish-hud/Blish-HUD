using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Blish_HUD.Controls {
    public class TextBox : TextInputBase {

        private const int STANDARD_CONTROLWIDTH  = 250;
        private const int STANDARD_CONTROLHEIGHT = 27;

        private const int TEXT_TOPPADDING  = 2;
        private const int TEXT_LEFTPADDING = 10;

        #region Load Static

        private static readonly Texture2D _textureTextbox;

        static TextBox() {
            _textureTextbox = Content.GetTexture("textbox");
        }

        #endregion

        public static readonly DesignStandard Standard = new DesignStandard(/*          Size */ new Point(250, 27),
                                                                            /*   PanelOffset */ new Point(5,   2),
                                                                            /* ControlOffset */ ControlStandard.ControlOffset);

        private int _prevCursorIndex  = 0;
        private int _horizontalOffset = 0;

        public TextBox() {
            _multiline = false;

            this.Size = new Point(STANDARD_CONTROLWIDTH, STANDARD_CONTROLHEIGHT);
        }

        protected override void UpdateScrolling() {
            Size2 leftPos = _font.MeasureString(_text.Substring(0, _cursorIndex));

            if (_cursorIndex > _prevCursorIndex) {
                _horizontalOffset = (int)Math.Max(_horizontalOffset, leftPos.Width - _size.X);
            } else {
                _horizontalOffset = (int)Math.Min(_horizontalOffset, leftPos.Width);
            }

            _prevCursorIndex = _cursorIndex;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this,
                                   _textureTextbox,
                                   new Rectangle(Point.Zero, _size - new Point(5, 0)),
                                   new Rectangle(0, 0, Math.Min(_textureTextbox.Width - 5, _size.X - 5), _textureTextbox.Height));

            spriteBatch.DrawOnCtrl(this, _textureTextbox,
                                   new Rectangle(_size.X - 5, 0, 5, _size.Y),
                                   new Rectangle(_textureTextbox.Width - 5, 0,
                                                 5, _textureTextbox.Height));

            var textBounds = new Rectangle(TEXT_LEFTPADDING - _horizontalOffset,
                                           TEXT_TOPPADDING,
                                           _size.X - TEXT_LEFTPADDING * 2,
                                           _size.Y - TEXT_TOPPADDING  * 2);

            // Draw the Textbox placeholder text
            if (!_focused && _text.Length == 0) {
                var phFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic);
                spriteBatch.DrawStringOnCtrl(this, _placeholderText, phFont, textBounds, Color.LightGray);
            }

            // Draw the Textbox text
            spriteBatch.DrawStringOnCtrl(this, this.Text, _font, textBounds, _foreColor);

            int selectionStart  = Math.Min(_selectionStart, _selectionEnd);
            int selectionLength = Math.Abs(_selectionStart - _selectionEnd);

            if (selectionLength > 0) {
                float highlightLeftOffset = _font.MeasureString(_text.Substring(0, selectionStart)).Width + textBounds.Left;
                float highlightWidth      = _font.MeasureString(_text.Substring(selectionStart, selectionLength)).Width;

                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        new Rectangle((int)highlightLeftOffset - 1, 3, (int)highlightWidth, _size.Y - 9),
                                        new Color(92, 80, 103, 150));
            } else if (_focused && _caretVisible) {
                float textOffset  = this.Font.MeasureString(_text.Substring(0, _cursorIndex)).Width;
                var   caretOffset = new Rectangle(textBounds.X + (int)textOffset - 2, textBounds.Y, textBounds.Width, textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", _font, caretOffset, _foreColor);
            }
        }

    }
}
