using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Globalization;
using System.Text;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;

namespace Blish_HUD.Controls {

    public class FloatInput : TextInputBase {
        private const int TextPaddingX = 10;

        private const int SpinnerWidth = 32;

        private const int SpinnerButtonHeight = 16;

        // Points up
        private static readonly AsyncTexture2D SpinnerSprite = AsyncTexture2D.FromAssetId(517181);

        // Points up
        private static readonly AsyncTexture2D SpinnerGlowSprite = AsyncTexture2D.FromAssetId(517182);

        private static readonly Texture2D TextBoxSprite = Content.GetTexture("textbox");

        private NumberInputSpinnerGlow _glow = NumberInputSpinnerGlow.None;

        private NumberInputAction _action = NumberInputAction.None;

        private TimeSpan _incrementInterval = TimeSpan.FromMilliseconds(150);

        private TimeSpan _incrementTimer;

        private Rectangle _textBoxRectangle = Rectangle.Empty;

        private Rectangle _textRectangle = Rectangle.Empty;

        private Rectangle _cursorRectangle = Rectangle.Empty;

        private Rectangle _highlightRectangle = Rectangle.Empty;

        private int _horizontalOffset;

        private float _minValue = float.MinValue;

        private float _maxValue = float.MaxValue;

        private float _incrementAmount = 0.1f;

        private string _formatString = "G"; // Default general format

        private float _originalValue;

        public FloatInput() {
            Width = 150;
            Height = SpinnerButtonHeight * 2;
            Input.Mouse.MouseWheelScrolled += OnGlobalMouseWheelScrolled;
        }

        public event EventHandler<EventArgs>? ValueChanged;

        /// <summary>
        /// Gets or sets the format string used to display the value.
        /// Examples: "0.00" for 2 decimal places, "0.0" for 1 decimal place, "G" for general format.
        /// This only affects display, not the stored value.
        /// </summary>
        public string FormatString {
            get => _formatString;
            set => SetProperty(ref _formatString, value ?? "G");
        }

        /// <summary>
        /// Gets or sets the increment/decrement amount for spinner buttons and scroll wheel.
        /// </summary>
        public float IncrementAmount {
            get => _incrementAmount;
            set => SetProperty(ref _incrementAmount, value);
        }

        public float Value {
            get => InvariantUtil.TryParseFloat(Text, out float value) ? value : 0f;
            set {
                if (value < MinValue) {
                    value = MinValue;
                } else if (value > MaxValue) {
                    value = MaxValue;
                }

                string text = value.ToString(_formatString, NumberFormatInfo.InvariantInfo);
                if (!string.Equals(Text, text, StringComparison.Ordinal)) {
                    Text = text;
                    Invalidate();
                    OnValueChanged();
                }
            }
        }

        public float MinValue {
            get => _minValue;
            set {
                _minValue = value;
                if (Value < value) {
                    Value = value;
                }
            }
        }

        public float MaxValue {
            get => _maxValue;
            set {
                _maxValue = value;
                if (Value > value) {
                    Value = value;
                }
            }
        }

        public override void DoUpdate(GameTime gameTime) {
            switch (_action) {
                case NumberInputAction.Increment:
                    _incrementTimer += gameTime.ElapsedGameTime;
                    if (_incrementTimer >= _incrementInterval) {
                        Value += _incrementAmount;
                        _incrementTimer += gameTime.ElapsedGameTime;
                        _incrementInterval = TimeSpan.FromMilliseconds(100);
                    }
                    break;
                case NumberInputAction.Decrement:
                    _incrementTimer += gameTime.ElapsedGameTime;
                    if (_incrementTimer >= _incrementInterval) {
                        Value -= _incrementAmount;
                        _incrementTimer += gameTime.ElapsedGameTime;
                        _incrementInterval = TimeSpan.FromMilliseconds(100);
                    }
                    break;
                case NumberInputAction.None:
                default:
                    _incrementTimer = TimeSpan.Zero;
                    _incrementInterval = TimeSpan.FromMilliseconds(150);
                    break;
            }

            base.DoUpdate(gameTime);
        }

        public override void RecalculateLayout() {
            // Layout zones (left to right):
            // |<-- Text Area (can overflow) -->|<-- Padding -->|<-- Spinner -->|
            // |          _textBoxRectangle                     |  SpinnerWidth  |
            //
            // _horizontalOffset: shifts the text view when cursor moves
            //   - Positive offset = scrolled right (showing more left content)
            //   - Reset to 0 when control loses focus

            _textBoxRectangle = TextBoxRectangle();

            _textRectangle = TextRectangle();

            _highlightRectangle = HighlightRectangle();

            _cursorRectangle = CursorRectangle();

            Rectangle TextBoxRectangle() {
                // The visible textbox background area (clipping region for text)
                // - X: 0 (left edge of control)
                // - Y: 0 (top edge of control)
                // - Width: control width minus spinner
                // - Height: full control height
                return new Rectangle(0, 0, Width - SpinnerWidth, Height);
            }

            Rectangle TextRectangle() {
                // Logical text positioning area for DrawStringOnCtrl with HorizontalAlignment.Right
                // Text is right-aligned within this rectangle (right edge of text = right edge of rectangle)
                //
                // _horizontalOffset adjusts text position:
                // - offset = 0: default position, text right edge at (textBoxWidth - padding)
                // - offset < 0: text shifted right, revealing left overflow
                // - offset > 0: not used (we scroll by making offset negative to reveal left overflow)
                //
                // Padding rules:
                // - TextPaddingX on right when offset = 0 (normal state)
                // - No padding when scrolled (offset != 0), text can touch edges

                // The right edge of the text (where right-aligned text ends)
                // Subtracting offset: negative offset increases right edge (shifts text right)
                int textRightEdge = _textBoxRectangle.Width - TextPaddingX - _horizontalOffset;

                // Vertical centering
                int verticalPadding = (Height / 2) - (_font.LineHeight / 2);

                return new Rectangle(0, verticalPadding, textRightEdge, _font.LineHeight);
            }

            Rectangle CursorRectangle() {
                // Blinking cursor position within the text
                // - X: calculated from right-aligned text start + width of text before cursor
                // - Y: slightly inset from text rectangle top
                // - Width: 2 pixels (thin cursor line)
                // - Height: font line height minus inset
                // - Must account for _horizontalOffset

                int currentCursorIndex = _cursorIndex;
                if (currentCursorIndex > _text.Length) {
                    currentCursorIndex = _text.Length;
                }

                float textWidth = _font.MeasureString(_text).Width;
                float textStart = _textRectangle.Right - textWidth;
                float cursorX = textStart + _font.MeasureString(_text.Substring(0, currentCursorIndex)).Width;

                return new Rectangle(
                    (int)cursorX,
                    _textRectangle.Y + 2,
                    2,
                    _font.LineHeight - 4
                );
            }

            Rectangle HighlightRectangle() {
                // Selection highlight area
                // - X: position of selection start character (accounting for right-alignment)
                // - Y: same as text rectangle
                // - Width: distance from selection start to selection end
                // - Height: font line height
                // - Must account for _horizontalOffset
                // - Returns Empty if no selection or invalid selection bounds
                // - Clipped to _textBoxRectangle bounds

                int currentSelectionStart = _selectionStart;
                int currentSelectionEnd = _selectionEnd;
                int selectionOffset = Math.Min(currentSelectionStart, currentSelectionEnd);
                if (selectionOffset > _text.Length) {
                    return Rectangle.Empty;
                }

                int selectionLength = Math.Abs(currentSelectionStart - currentSelectionEnd);
                if (selectionLength <= 0 || selectionOffset + selectionLength > _text.Length) {
                    return Rectangle.Empty;
                }

                float textWidth = _font.MeasureString(_text).Width;
                float textStart = _textRectangle.Right - textWidth;
                float highlightStart = textStart + _font.MeasureString(_text.Substring(0, selectionOffset)).Width;
                float highlightWidth = _font.MeasureString(_text.Substring(selectionOffset, selectionLength)).Width;

                var highlight = new Rectangle(
                    (int)highlightStart,
                    _textRectangle.Y,
                    (int)highlightWidth,
                    _font.LineHeight
                );

                // Clip to textbox bounds
                return Rectangle.Intersect(highlight, _textBoxRectangle);
            }
        }

        public override int GetCursorIndexFromPosition(int x, int y) {
            float textStart = Width - _font.MeasureString(Text).Width - TextPaddingX - SpinnerWidth;

            int charIndex = 0;

            BitmapFont.StringGlyphEnumerable glyphs = _font.GetGlyphs(_text);
            foreach (BitmapFontGlyph glyph in glyphs) {
                if (textStart + glyph.Position.X + (glyph.FontRegion.Width / 2f) > _horizontalOffset + x) {
                    break;
                }

                charIndex++;
            }

            return charIndex;
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            bool mouseOverSpinner = e.MousePosition.X > AbsoluteBounds.Right - SpinnerWidth;
            bool mouseOverUpButton = mouseOverSpinner && e.MousePosition.Y < AbsoluteBounds.Top + SpinnerButtonHeight;
            _glow = (mouseOverSpinner, mouseOverUpButton) switch {
                (true, true) => NumberInputSpinnerGlow.Up,
                (true, false) => NumberInputSpinnerGlow.Down,
                _ => NumberInputSpinnerGlow.None
            };

            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            _glow = NumberInputSpinnerGlow.None;
            base.OnMouseLeft(e);
        }

        protected override void MoveLine(int delta) {
            // Not sure what to do here
        }

        protected override void UpdateScrolling() {
            // Don't scroll when not focused - offset should remain at 0
            if (!_focused || _text.Length == 0) {
                _horizontalOffset = 0;
                _prevCursorIndex = _cursorIndex;
                Invalidate();
                return;
            }

            // For right-aligned text:
            // - Text right edge is at: _textBoxRectangle.Width - TextPaddingX - _horizontalOffset
            // - Text left edge is at: rightEdge - textWidth
            // - Cursor position: leftEdge + widthBeforeCursor
            // - We need cursor to stay within visible bounds (0 to _textBoxRectangle.Width)
            //
            // _horizontalOffset adjusts text position:
            // - Positive offset: text shifts left, reveals right content (when cursor moves right)
            // - Negative offset: text shifts right, reveals left content (when cursor moves left into overflow)

            int currentCursorIndex = Math.Min(_cursorIndex, _text.Length);
            float textWidth = _font.MeasureString(_text).Width;
            float widthBeforeCursor = _font.MeasureString(_text.Substring(0, currentCursorIndex)).Width;

            // Calculate text position with current offset
            float textRightEdge = _textBoxRectangle.Width - TextPaddingX - _horizontalOffset;
            float textLeftEdge = textRightEdge - textWidth;
            float cursorX = textLeftEdge + widthBeforeCursor;

            // Adjust offset to keep cursor in visible bounds
            if (cursorX < 0) {
                // Cursor is off the left edge, shift text right (make offset more negative)
                _horizontalOffset += (int)cursorX;
            } else if (cursorX > _textBoxRectangle.Width - TextPaddingX) {
                // Cursor is off the right edge, shift text left (make offset more positive)
                _horizontalOffset += (int)(cursorX - (_textBoxRectangle.Width - TextPaddingX));
            }

            // Clamp offset to valid range:
            // - Min offset (most negative): when text is shifted fully right, leftmost char visible at left edge
            //   textLeftEdge = 0 means: textRightEdge - textWidth = 0, so textRightEdge = textWidth
            //   baseRightEdge - offset = textWidth, so offset = baseRightEdge - textWidth
            //   If textWidth > baseRightEdge, minOffset is negative
            // - Max offset (most positive): 0 (text at default right-aligned position)
            float availableWidth = _textBoxRectangle.Width - TextPaddingX;
            float minOffset = availableWidth - textWidth; // Can be negative if text overflows
            float maxOffset = 0;

            // Only allow scrolling if text actually overflows
            if (textWidth > availableWidth) {
                _horizontalOffset = (int)Math.Max(minOffset, Math.Min(_horizontalOffset, maxOffset));
            } else {
                _horizontalOffset = 0;
            }

            _prevCursorIndex = _cursorIndex;
            Invalidate();
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            if (e.MousePosition.X > AbsoluteBounds.Right - SpinnerWidth) {
                UnsetFocus();
                Content.PlaySoundEffectByName(@"button-click");
                _action = e.MousePosition.Y < AbsoluteBounds.Top + SpinnerButtonHeight
                    ? NumberInputAction.Increment
                    : NumberInputAction.Decrement;
            } else {
                base.OnLeftMouseButtonPressed(e);
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            switch (_action) {
                case NumberInputAction.Increment:
                    Value += _incrementAmount;
                    UnsetFocus();
                    break;
                case NumberInputAction.Decrement:
                    Value -= _incrementAmount;
                    UnsetFocus();
                    break;
                case NumberInputAction.None:
                default:
                    base.OnLeftMouseButtonReleased(e);
                    break;
            }

            _action = NumberInputAction.None;
        }


        protected override void HandleEnter() {
            UnsetFocus();
        }

        protected override void OnInputFocusChanged(ValueEventArgs<bool> e) {
            base.OnInputFocusChanged(e);
            if (e.Value) {
                _originalValue = Value;
                SelectAll();
                Invalidate();
            } else {
                ApplyTextAsValue();
                _horizontalOffset = 0;
                Invalidate();
            }
        }

        private void OnValueChanged() {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGlobalMouseWheelScrolled(object sender, MouseEventArgs e) {
            if (MouseOver) {
                if (Input.Mouse.State.ScrollWheelValue > 0) {
                    Value += _incrementAmount;
                } else {
                    Value -= _incrementAmount;
                }
            }
        }

        private void ApplyTextAsValue() {
            if (InvariantUtil.TryParseFloat(_text, out float value)) {
                Value = value;
                Invalidate();
                OnValueChanged();
            } else {
                Value = _originalValue;
            }
        }

        /// <remarks>
        /// Direct copy of <see cref="TextInputBase.PaintText(SpriteBatch, Rectangle, HorizontalAlignment)"/>
        /// that also exposes the clippingRectangle parameter of
        /// <see cref="BitmapFontExtensions.DrawString(SpriteBatch, BitmapFont, string, Vector2, Color, Rectangle?)"/>.
        /// </remarks>
        protected virtual void PaintText(SpriteBatch spriteBatch, Rectangle textRegion, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, Rectangle? clippingRectangle = null) {
            if (!_focused && _text.Length == 0) {
                spriteBatch.DrawStringOnCtrl(this, _placeholderText, _font, textRegion, Color.LightGray, false, false, 0, horizontalAlignment, VerticalAlignment.Top, clippingRectangle);
            }

            spriteBatch.DrawStringOnCtrl(this, _text, _font, textRegion, _foreColor, false, false, 0, horizontalAlignment, VerticalAlignment.Top, clippingRectangle);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            #region Text

            if (Focused) {
                spriteBatch.DrawOnCtrl(
                    this,
                    TextBoxSprite,
                    _textBoxRectangle
                );

                if (_highlightRectangle.IsEmpty) {
                    PaintCursor(spriteBatch, _cursorRectangle);
                } else {
                    PaintHighlight(spriteBatch, _highlightRectangle);
                }
            }

            PaintText(spriteBatch, _textRectangle, HorizontalAlignment.Right, _textBoxRectangle);

            #endregion Text

            #region Spinner

            Rectangle buttonsRectangle = new Rectangle(bounds.Right - SpinnerWidth, 0, SpinnerWidth, SpinnerButtonHeight * 2);
            switch ((hoverButton: _glow, pressedButton: _action)) {
                case (NumberInputSpinnerGlow.Up, NumberInputAction.None):
                    spriteBatch.DrawOnCtrl(
                        this,
                        SpinnerGlowSprite,
                        buttonsRectangle
                    );

                    spriteBatch.DrawOnCtrl(
                        this,
                        SpinnerSprite,
                        buttonsRectangle,
                        null,
                        Color.White,
                        0,
                        Vector2.Zero,
                        SpriteEffects.FlipVertically
                    );
                    break;
                case (NumberInputSpinnerGlow.Down, NumberInputAction.None):
                    spriteBatch.DrawOnCtrl(
                        this,
                        SpinnerSprite,
                        buttonsRectangle
                    );
                    spriteBatch.DrawOnCtrl(
                        this,
                        SpinnerGlowSprite,
                        buttonsRectangle,
                        null,
                        Color.White,
                        0,
                        Vector2.Zero,
                        SpriteEffects.FlipVertically
                    );
                    break;
                default:
                    spriteBatch.DrawOnCtrl(
                        this,
                        SpinnerSprite,
                        buttonsRectangle
                    );

                    spriteBatch.DrawOnCtrl(
                        this,
                        SpinnerSprite,
                        buttonsRectangle,
                        null,
                        Color.White,
                        0,
                        Vector2.Zero,
                        SpriteEffects.FlipVertically
                    );
                    break;
            }

            #endregion Spinner
        }

        protected override void DisposeControl() {
            ValueChanged = null;
            Input.Mouse.MouseWheelScrolled -= OnGlobalMouseWheelScrolled;
            base.DisposeControl();
        }

        private enum NumberInputAction {
            None,

            Increment,

            Decrement
        }

        private enum NumberInputSpinnerGlow {
            None,

            Up,

            Down
        }
    }
}
