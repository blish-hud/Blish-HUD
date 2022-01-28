using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {
    public class TextBox : TextInputBase {

        private const int STANDARD_CONTROLWIDTH  = 250;
        private const int STANDARD_CONTROLHEIGHT = 27;

        private const int TEXT_HORIZONTALPADDING = 10;

        #region Load Static

        private static readonly Texture2D _textureTextbox = Content.GetTexture("textbox");

        #endregion

        public static readonly DesignStandard Standard = new DesignStandard(/*          Size */ new Point(250, 27),
                                                                            /*   PanelOffset */ new Point(5,   2),
                                                                            /* ControlOffset */ ControlStandard.ControlOffset);

        /// <summary>
        /// Fires when <see cref="Keys.Enter"/> is pressed while this <see cref="TextBox"/> is focused (<see cref="TextInputBase.Focused" /> is <c>true</c>).
        /// </summary>
        public event EventHandler<EventArgs> EnterPressed;

        private bool _hideBackground;
        public bool HideBackground {
            get => _hideBackground;
            set => SetProperty(ref _hideBackground, value);
        }

        protected virtual void OnEnterPressed(EventArgs e) {
            this.Focused = false;

            EnterPressed?.Invoke(this, e);
        }

        private int _prevCursorIndex  = 0;
        private int _horizontalOffset = 0;

        public TextBox() {
            _multiline = false;
            _maxLength = 2048;

            this.Size = new Point(STANDARD_CONTROLWIDTH, STANDARD_CONTROLHEIGHT);
        }

        protected override void HandleEnter() {
            OnEnterPressed(EventArgs.Empty);
        }

        protected override void MoveLine(int delta) {
            if (delta < 0) {
                HandleHome(false);
            } else {
                HandleEnd(false);
            }
        }

        public override int GetCursorIndexFromPosition(int x, int y) {
            x -= TEXT_HORIZONTALPADDING;

            int charIndex = 0;

            var glyphs = _font.GetGlyphs(_text);

            foreach (var glyph in glyphs) {
                if (glyph.Position.X + glyph.FontRegion.Width / 2f > _horizontalOffset + x) {
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

            if (selectionLength <= 0 || selectionStart + selectionLength > _text.Length) return Rectangle.Empty;

            float highlightLeftOffset = MeasureStringWidth(_text.Substring(0, selectionStart));
            float highlightWidth      = MeasureStringWidth(_text.Substring(selectionStart, selectionLength));

            switch (this.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    highlightLeftOffset += (this.Width - highlightWidth) / 2f - TEXT_HORIZONTALPADDING;
                    break;
                case HorizontalAlignment.Right:
                    highlightLeftOffset += this.Width - highlightWidth - TEXT_HORIZONTALPADDING * 2;
                    break;
                default: break;
            }

            return new Rectangle(_textRegion.Left + (int)highlightLeftOffset - 1,
                                 _textRegion.Y,
                                 (int)highlightWidth,
                                 _font.LineHeight - 1);
        }

        private Rectangle CalculateCursorRegion() {
            float textOffset = MeasureStringWidth(_text.Substring(0, _cursorIndex));

            switch (this.HorizontalAlignment) {
                case HorizontalAlignment.Center:
                    textOffset += (this.Width - MeasureStringWidth(_text)) / 2f - TEXT_HORIZONTALPADDING;
                    break;
                case HorizontalAlignment.Right:
                    textOffset += this.Width - MeasureStringWidth(_text) - TEXT_HORIZONTALPADDING * 2;
                    break;
                default: break;
            }

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
            Invalidate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!this.HideBackground) {
                spriteBatch.DrawOnCtrl(
                                       this,
                                       _textureTextbox,
                                       new Rectangle(Point.Zero, _size - new Point(5, 0)),
                                       new Rectangle(0,          0, Math.Min(_textureTextbox.Width - 5, _size.X - 5), _textureTextbox.Height)
                                      );

                spriteBatch.DrawOnCtrl(
                                       this, _textureTextbox,
                                       new Rectangle(_size.X               - 5, 0, 5, _size.Y),
                                       new Rectangle(_textureTextbox.Width - 5, 0, 5, _textureTextbox.Height)
                                      );
            }

            PaintText(spriteBatch, _textRegion);

            if (_highlightRegion.IsEmpty) {
                PaintCursor(spriteBatch, _cursorRegion);
            } else {
                PaintHighlight(spriteBatch, _highlightRegion);
            }
        }

    }
}
