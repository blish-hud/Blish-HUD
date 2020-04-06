using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class MultilineTextBox : TextInputBase {

        private const int TEXT_TOPPADDING  = 7;
        private const int TEXT_LEFTPADDING = 10;

        #region Load Static

        private static readonly Texture2D _textureTextbox;

        static MultilineTextBox() {
            _textureTextbox = Content.GetTexture("textbox");
        }

        #endregion

        public MultilineTextBox() {
            _multiline = true;
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            int newIndex = GetCursorIndexFromFromPosition(this.RelativeMousePosition - new Point(TEXT_LEFTPADDING, TEXT_TOPPADDING));

            if (_cursorIndex == newIndex && e.IsDoubleClick) {
                SelectAll();
            } else {
                UserSetCursorIndex(newIndex);
                ResetSelection();
            }
        }

        private int GetCursorIndexFromFromPosition(Point clickPos) {
            string[] lines = _text.Split(NEWLINE);

            int predictedLine = clickPos.Y / _font.LineHeight;

            if (predictedLine > lines.Length - 1) {
                return this.Length;
            }

            var glyphs = _font.GetGlyphs(lines[predictedLine]);

            int charIndex = 0;

            foreach (var glyph in glyphs) {
                if (glyph.Position.X + glyph.FontRegion.Width > clickPos.X) {
                    break;
                }

                charIndex++;
            }

            for (int i = 0; i < predictedLine; i++) {
                charIndex += lines[i].Length + 1;
            }

            return charIndex;
        }

        private Rectangle   _textRegion       = Rectangle.Empty;
        private Rectangle[] _highlightRegions = Array.Empty<Rectangle>();
        private Rectangle   _cursorRegion     = Rectangle.Empty;

        private (int Line, int Character) GetSplitIndex(int index) {
            int lineIndex = 0;
            int charIndex = 0;

            for (int i = 0; i < index; i++) {
                charIndex++;

                if (_text[lineIndex] == NEWLINE) {
                    lineIndex++;
                    charIndex = 0;
                };
            }

            return (lineIndex, charIndex);
        }

        private Rectangle[] CalculateHighlightRegions() {
            return Array.Empty<Rectangle>();
        }

        private Rectangle CalculateTextRegion() {
            return new Rectangle(TEXT_LEFTPADDING,
                                 TEXT_TOPPADDING,
                                 _size.X - TEXT_LEFTPADDING * 2,
                                 _size.Y - TEXT_TOPPADDING  * 2);
        }

        private Rectangle CalculateCursorRegion() {
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

            return new Rectangle(_textRegion.X + offset.X - 2,
                                 _textRegion.Y + offset.Y + 2,
                                 2,
                                 _font.LineHeight - 4);
        }

        public override void RecalculateLayout() {
            _textRegion       = CalculateTextRegion();
            _highlightRegions = CalculateHighlightRegions();
            _cursorRegion     = CalculateCursorRegion();
        }

        protected override void UpdateScrolling() { /* NOOP */ }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
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
