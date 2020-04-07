using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class TextBox : TextInputBase {

        private const int STANDARD_CONTROLWIDTH  = 250;
        private const int STANDARD_CONTROLHEIGHT = 27;

        private const int TEXT_HORIZONTALPADDING = 10;

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

        protected override void MoveLine(int delta) { /* NOOP */ }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            int newIndex = GetCursorIndexFromFromPosition(this.RelativeMousePosition.X - TEXT_HORIZONTALPADDING);

            if (_cursorIndex == newIndex && e.IsDoubleClick) {
                SelectAll();
            } else {
                UserSetCursorIndex(newIndex);
                ResetSelection();
            }
        }

        private int GetCursorIndexFromFromPosition(int x) {
            int charIndex = 0;

            var glyphs = _font.GetGlyphs(_text);

            foreach (var glyph in glyphs) {
                if (glyph.Position.X + glyph.FontRegion.Width > _horizontalOffset + x) {
                    break;
                }

                charIndex++;
            }

            return charIndex;
        }

        private Rectangle _textRegion      = Rectangle.Empty;
        private Rectangle _highlightRegion = Rectangle.Empty;
        private Rectangle _cursorRegion    = Rectangle.Empty;

        private Rectangle CalculateTextRegion() {
            int verticalPadding = _size.Y / 2 - (_font.LineHeight / 2);

            return new Rectangle(TEXT_HORIZONTALPADDING - _horizontalOffset,
                                 verticalPadding,
                                 _size.X - TEXT_HORIZONTALPADDING * 2,
                                 _size.Y - verticalPadding * 2);
        }

        private Rectangle CalculateHighlightRegion() {
            int selectionStart  = Math.Min(_selectionStart, _selectionEnd);
            int selectionLength = Math.Abs(_selectionStart - _selectionEnd);

            if (selectionLength <= 0 || selectionStart + selectionLength > this.Length) return Rectangle.Empty;

            float highlightLeftOffset = MeasureStringWidth(_text.Substring(0, selectionStart));
            float highlightWidth      = MeasureStringWidth(_text.Substring(selectionStart, selectionLength));

            return new Rectangle(_textRegion.Left + (int)highlightLeftOffset - 1,
                                 _textRegion.Y,
                                 (int)highlightWidth,
                                 _font.LineHeight - 1);
        }

        private Rectangle CalculateCursorRegion() {
            float textOffset = MeasureStringWidth(_text.Substring(0, _cursorIndex));

            return new Rectangle(_textRegion.X + (int)textOffset - 2,
                                 _textRegion.Y + 2,
                                 2,
                                 _font.LineHeight - 4);
        }

        public override void RecalculateLayout() {
            _textRegion      = CalculateTextRegion();
            _highlightRegion = CalculateHighlightRegion();
            _cursorRegion    = CalculateCursorRegion();
        }

        protected override void UpdateScrolling() {
            float lineWidth = MeasureStringWidth(_text.Substring(0, _cursorIndex));

            if (_cursorIndex > _prevCursorIndex) {
                _horizontalOffset = (int)Math.Max(_horizontalOffset, lineWidth - _size.X);
            } else {
                _horizontalOffset = (int)Math.Min(_horizontalOffset, lineWidth);
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
                                   new Rectangle(_textureTextbox.Width - 5, 0, 5, _textureTextbox.Height));

            PaintText(spriteBatch, _textRegion);

            if (_highlightRegion.IsEmpty) {
                PaintCursor(spriteBatch, _cursorRegion);
            } else {
                PaintHighlight(spriteBatch, _highlightRegion);
            }
        }

    }
}
