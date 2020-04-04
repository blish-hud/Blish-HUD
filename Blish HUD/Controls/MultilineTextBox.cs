using System;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class MultilineTextBox : TextInputBase {

        private const int PADDING = 2;

        private const int TEXT_TOPPADDING  = 2;
        private const int TEXT_LEFTPADDING = 10;

        #region Load Static

        private static readonly Texture2D _textureTextbox;

        static MultilineTextBox() {
            _textureTextbox = Content.GetTexture("textbox");
        }

        #endregion

        private int _prevCursorIndex  = 0;
        private int _horizontalOffset = 0;

        public MultilineTextBox() {
            _multiline = true;
        }

        protected override void UpdateScrolling() {
            
        }

        public override void DoUpdate(GameTime gameTime) {
            if (_cursorMoved) {

            }

            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this,
                                   _textureTextbox,
                                   new Rectangle(Point.Zero, _size - new Point(5, 0)),
                                   new Rectangle(0,          0, Math.Min(_textureTextbox.Width - 5, _size.X - 5), _textureTextbox.Height));

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
                spriteBatch.DrawStringOnCtrl(this, _placeholderText, _font, textBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
            }

            // Draw the Textbox text
            spriteBatch.DrawStringOnCtrl(this, this.Text, _font, textBounds, _foreColor, false, false, 0, HorizontalAlignment.Left, VerticalAlignment.Top);

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
                int lineIndex = 0;
                int lineStart = 0;

                for (int n = 0; n < _cursorIndex; n++) {
                    if (_text[n] == NEWLINE) {
                        lineIndex++;
                        lineStart = n;
                    };
                }

                var glyphs = _font.GetGlyphs(_text.Substring(lineStart, _cursorIndex - lineStart));

                var offset = Point.Zero;

                if (_cursorIndex > 0) {
                    var last = glyphs.Last();

                    offset = new Point((int)last.Position.X + (last.FontRegion?.Width ?? 0),
                                       _font.LineHeight * lineIndex);
                }

                var   caretOffset = new Rectangle(textBounds.X + offset.X - 2,
                                                  textBounds.Y + offset.Y,
                                                  textBounds.Width,
                                                  textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", _font, caretOffset, _foreColor, false, false, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }
    }
}
