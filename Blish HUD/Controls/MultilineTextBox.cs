using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class MultilineTextBox : TextInputBase {

        private const int TEXT_TOPPADDING  = 7;
        private const int TEXT_LEFTPADDING = 10;

        private bool _hideBackground;
        public bool HideBackground {
            get => _hideBackground;
            set => SetProperty(ref _hideBackground, value);
        }

        protected int[] _displayNewLineIndices = Array.Empty<int>();

        /// <summary>
        /// The indices of the added new line characters in the processed
        /// <see cref="TextInputBase.DisplayText"/>.
        /// </summary>
        public int[] DisplayNewLineIndices => _displayNewLineIndices;

        private bool _disableWordWrap;

        /// <summary>
        /// Determines whether the automatic word-wrap will be disabled.
        /// </summary>
        public bool DisableWordWrap {
            get => _disableWordWrap;
            set {
                if (SetProperty(ref _disableWordWrap, value)) {
                   RecalculateLayout();
                }
            }
        }

        private char[] _wrapCharacters;

        /// <summary>
        /// The characters, that are used to wrap a word, if it does not fit the current line
        /// it's on.
        /// </summary>
        public char[] WrapCharacters {
            get => _wrapCharacters ?? Array.Empty<char>();
            set {
                if (SetProperty(ref _wrapCharacters, value)) {
                    RecalculateLayout();
                }
            }
        }

        public MultilineTextBox() {
            _multiline = true;
            _maxLength = 524288;
        }

        /// <summary>
        /// Calculates the actual cursor index (in reference to
        /// <see cref="TextInputBase.Text"/>), if the <paramref name="displayIndex"/>
        /// was calculated using the <see cref="TextInputBase.DisplayText"/>.
        /// </summary>
        protected int GetCursorIndexFromDisplayIndex(int displayIndex) {
            int cursorIndex = displayIndex;
            foreach (int displayNewLineIndex in _displayNewLineIndices) {
                if (displayNewLineIndex > displayIndex) break;
                cursorIndex--;
            }

            return cursorIndex;
        }

        /// <summary>
        /// Calculates the display cursor index (in reference to
        /// <see cref="TextInputBase.DisplayText"/>), if the <paramref name="cursorIndex"/>
        /// was calculated using the <see cref="TextInputBase.Text"/>.
        /// </summary>
        protected int GetDisplayIndexFromCursorIndex(int cursorIndex) {
            int displayIndex = cursorIndex;
            foreach (int displayNewLineIndex in _displayNewLineIndices) {
                if (displayNewLineIndex > displayIndex) break;
                displayIndex++;
            }
            return displayIndex;
        }

        protected override void MoveLine(int delta) {
            int newIndex = 0; // if targetLine is < 0, we set cursor index to 0

            string[] lines = _displayText.Split(NEWLINE);

            var cursor = GetSplitIndex(_cursorIndex);

            int targetLine = cursor.Line + delta;

            if (targetLine >= lines.Length) {
                newIndex = _displayText.Length;
            } else if (targetLine >= 0) {
                float cursorLeft   = MeasureStringWidth(lines[cursor.Line].Substring(0, cursor.Character));
                float minOffset    = cursorLeft;
                int   currentIndex = 0;

                var glyphs = _font.GetGlyphs(lines[targetLine]);

                foreach (var glyph in glyphs) {
                    float localOffset = Math.Abs(glyph.Position.X - cursorLeft);

                    if (localOffset < minOffset) {
                        newIndex  = currentIndex;
                        minOffset = localOffset;
                    }

                    currentIndex++;
                }

                for (int i = 0; i < targetLine; i++) {
                    newIndex += lines[i].Length + 1;
                }
            }

            newIndex = GetCursorIndexFromDisplayIndex(newIndex);

            UserSetCursorIndex(newIndex);
            UpdateSelectionIfShiftDown();
        }

        /// <inheritdoc/>
        protected override string ProcessDisplayText(string value) {
            return ApplyWordWrap(value);
        }

        /// <summary>
        /// Applies word-wrap to the <paramref name="value"/>.
        /// </summary>
        protected string ApplyWordWrap(string value) {
            if (DisableWordWrap) {
                _displayNewLineIndices = Array.Empty<int>();
                return value;
            }

            string displayText = DrawUtil.WrapText(_font, value, this._textRegion.Width, WrapCharacters, out int[] newLineIndices);
            _displayNewLineIndices = newLineIndices;

            return displayText;
        }

        public override int GetCursorIndexFromPosition(int x, int y) {
            x -= TEXT_LEFTPADDING;
            y -= TEXT_TOPPADDING;

            string[] lines = _displayText.Split(NEWLINE);

            int predictedLine = y / _font.LineHeight;

            if (predictedLine > lines.Length - 1) {
                return GetCursorIndexFromDisplayIndex(_displayText.Length);
            }

            var glyphs = _font.GetGlyphs(lines[predictedLine]);

            int charIndex = 0;

            foreach (var glyph in glyphs) {
                if (glyph.Position.X + glyph.FontRegion.Width / 2f > x) {
                    break;
                }

                charIndex++;
            }

            for (int i = 0; i < predictedLine; i++) {
                charIndex += lines[i].Length + 1;
            }

            return GetCursorIndexFromDisplayIndex(charIndex);
        }

        private Rectangle   _textRegion       = Rectangle.Empty;
        private Rectangle[] _highlightRegions = Array.Empty<Rectangle>();
        private Rectangle   _cursorRegion     = Rectangle.Empty;

        /// <remarks>
        /// The <paramref name="index"/> refers to the cursorIndex (in reference to
        /// <see cref="TextInputBase.Text"/>), while the return value is based on
        /// the <see cref="TextInputBase.DisplayText"/>.
        /// </remarks>
        private (int Line, int Character) GetSplitIndex(int index) {
            int lineIndex = 0;
            int charIndex = 0;

            index = GetDisplayIndexFromCursorIndex(index);

            for (int i = 0; i < index; i++) {
                charIndex++;

                if (_displayText[i] == NEWLINE) {
                    lineIndex++;
                    charIndex = 0;
                }
            }

            return (lineIndex, charIndex);
        }

        private Rectangle[] CalculateHighlightRegions() {
            int selectionStart  = Math.Min(_selectionStart, _selectionEnd);
            int selectionLength = Math.Abs(_selectionStart - _selectionEnd);

            if (selectionLength <= 0 || selectionStart + selectionLength > _text.Length) return Array.Empty<Rectangle>();

            string[] lines = _displayText.Split(NEWLINE);

            var startIndex = GetSplitIndex(selectionStart);
            var endIndex   = GetSplitIndex(selectionStart + selectionLength);

            int lineSpans = endIndex.Line - startIndex.Line;

            var regions = new Rectangle[lineSpans + 1];

            if (lineSpans == 0) {
                float highlightLeftOffset = MeasureStringWidth(lines[startIndex.Line].Substring(0, startIndex.Character));
                float highlightWidth      = MeasureStringWidth(lines[startIndex.Line].Substring(startIndex.Character, selectionLength));

                regions[0] = new Rectangle(_textRegion.Left + (int)highlightLeftOffset - 1,
                                           _textRegion.Top + (startIndex.Line * _font.LineHeight),
                                           (int)highlightWidth,
                                           _font.LineHeight - 1);
            } else {
                // First line
                float firstHighlightLeftOffset = MeasureStringWidth(lines[startIndex.Line].Substring(0, startIndex.Character));
                float firstHighlightWidth      = MeasureStringWidth(lines[startIndex.Line].Substring(startIndex.Character));

                regions[0] = new Rectangle(_textRegion.Left + (int) firstHighlightLeftOffset - 1,
                                           _textRegion.Top  + (startIndex.Line * _font.LineHeight),
                                           (int) firstHighlightWidth,
                                           _font.LineHeight - 1);

                // Middle lines
                for (int i = startIndex.Line + 1; i < endIndex.Line; i++) {
                    float fullWidth = MeasureStringWidth(lines[i]);

                    regions[i - startIndex.Line] = new Rectangle(_textRegion.Left - 1,
                                                                 _textRegion.Top  + (i * _font.LineHeight),
                                                                 (int) fullWidth,
                                                                 _font.LineHeight - 1);
                }

                // Last line
                float lastHighlightWidth = MeasureStringWidth(lines[endIndex.Line].Substring(0, endIndex.Character));

                regions[lineSpans] = new Rectangle(_textRegion.Left - 1,
                                                   _textRegion.Top  + (endIndex.Line * _font.LineHeight),
                                                   (int) lastHighlightWidth,
                                                   _font.LineHeight - 1);
            }

            return regions;
        }

        private Rectangle CalculateTextRegion() {
            return new Rectangle(TEXT_LEFTPADDING,
                                 TEXT_TOPPADDING,
                                 _size.X - TEXT_LEFTPADDING * 2,
                                 _size.Y - TEXT_TOPPADDING  * 2);
        }

        private Rectangle CalculateCursorRegion() {
            var cursor = GetSplitIndex(_cursorIndex);

            string[] lines = _displayText.Split(NEWLINE);

            float cursorLeft = MeasureStringWidth(lines[cursor.Line].Substring(0, cursor.Character));

            var offset = Point.Zero;

            if (_cursorIndex > 0) {
                offset = new Point((int)cursorLeft,
                                   _font.LineHeight * cursor.Line);
            }

            return new Rectangle(_textRegion.X + offset.X - 2,
                                 _textRegion.Y + offset.Y + 2,
                                 2,
                                 _font.LineHeight - 4);
        }

        public override void RecalculateLayout() {
            _displayText = ProcessDisplayText(_text);
            _textRegion       = CalculateTextRegion();
            _highlightRegions = CalculateHighlightRegions();
            _cursorRegion     = CalculateCursorRegion();
        }

        protected override void HandleDelete() {
            base.HandleDelete();
            RecalculateLayout();
        }

        protected override void UpdateScrolling() { /* NOOP */ }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!this.HideBackground) {
                // Background tint
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, bounds.Width - 2, bounds.Height - 2), Color.Black * 0.5f);

                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, bounds.Width - 2, 2), Color.Black * 0.3f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, bounds.Width - 2, 1), Color.Black * 0.2f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 2, bounds.Height - 2), Color.Black * 0.3f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 1, bounds.Height - 2), Color.Black * 0.2f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, bounds.Height - 2, bounds.Width - 2, 2), Color.Black * 0.3f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, bounds.Height - 2, bounds.Width - 2, 1), Color.Black * 0.2f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Width - 2, 1, 2, bounds.Height - 2), Color.Black * 0.3f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Width - 2, 1, 1, bounds.Height - 2), Color.Black * 0.2f);
            }

            PaintText(spriteBatch, _textRegion);
            
            if (_highlightRegions.Length > 0) {
                foreach (var highlightRegion in _highlightRegions) {
                    PaintHighlight(spriteBatch, highlightRegion);
                }
            } else {
                PaintCursor(spriteBatch, _cursorRegion);
            }
        }
    }
}
