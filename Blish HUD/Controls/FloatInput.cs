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
        public event EventHandler<EventArgs>? EnterPressed;

        private const int TextPaddingX = 10;

        private const int SpinnerWidth = 32;

        private const int SpinnerButtonHeight = 16;

        // Points up
        private static readonly Texture2D SpinnerSprite = Content.GetTexture("spinner");

        // Points up
        private static readonly Texture2D SpinnerGlowSprite = Content.GetTexture("spinner-glow");

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

        public FloatInput() {
            Width = 150;
            Height = SpinnerButtonHeight * 2;
            TextChanged += OnTextChanged;
            InputFocusChanged += OnInputFocusChanged;
            Input.Mouse.MouseWheelScrolled += OnGlobalMouseWheelScrolled;
        }

        public event EventHandler<EventArgs>? ValueChanged;

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
                const float epsilon = 1e-6f; // Use a small epsilon for floating-point comparison
                
                // Snap to boundary if within epsilon, otherwise clamp if outside bounds
                if (value <= MinValue + epsilon) {
                    value = MinValue;
                } else if (value >= MaxValue - epsilon) {
                    value = MaxValue;
                }

                string text = value.ToString(NumberFormatInfo.InvariantInfo);
                if (Text != text) {
                    Text = text;
                    OnValueChanged();
                }
            }
        }

        public float MinValue {
            get => _minValue;
            set {
                const float epsilon = 1e-6f; // Use a small epsilon for floating-point comparison
                _minValue = value;
                if (Value <= value + epsilon) {
                    Value = value;
                }
            }
        }

        public float MaxValue {
            get => _maxValue;
            set {
                const float epsilon = 1e-6f; // Use a small epsilon for floating-point comparison
                _maxValue = value;
                if (Value >= value - epsilon) {
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
            _textBoxRectangle = TextBoxRectangle();

            _textRectangle = TextRectangle();

            _highlightRectangle = HighlightRectangle();

            _cursorRectangle = CursorRectangle();

            base.RecalculateLayout();

            Rectangle TextBoxRectangle() {
                return new Rectangle(0, 0, Width - SpinnerWidth, Height);
            }

            Rectangle TextRectangle() {
                int verticalPadding = (Height / 2) - (_font.LineHeight / 2);
                return new Rectangle(
                    _horizontalOffset - TextPaddingX,
                    verticalPadding,
                    Width - SpinnerWidth,
                    Height - (verticalPadding * 2)
                );
            }

            Rectangle CursorRectangle() {
                int currentCursorIndex = _cursorIndex;
                if (currentCursorIndex > _text.Length) {
                    currentCursorIndex = _text.Length;
                }

                float textStart = Width - _font.MeasureString(Text).Width - SpinnerWidth + _horizontalOffset - TextPaddingX;
                float cursorStart = textStart + _font.MeasureString(_text.Substring(0, currentCursorIndex)).Width;
                return new Rectangle(
                    (int)cursorStart,
                    _textRectangle.Y + 2,
                    2,
                    _font.LineHeight - 4
                );
            }

            Rectangle HighlightRectangle() {
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

                float textStart = Width - _font.MeasureString(_text).Width - SpinnerWidth + _horizontalOffset - TextPaddingX;
                float highlightStart = textStart + _font.MeasureString(_text.Substring(0, selectionOffset)).Width;
                float highlightWidth = _font.MeasureString(_text.Substring(selectionOffset, selectionLength)).Width;

                return new Rectangle(
                    (int)highlightStart - 1,
                    _textRectangle.Y,
                    (int)highlightWidth,
                    _font.LineHeight - 1).Clip(_textBoxRectangle);
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
            float lineWidth = MeasureStringWidth(_text.Substring(_cursorIndex));

            _horizontalOffset = _cursorIndex < _prevCursorIndex
                ? (int)Math.Max(_horizontalOffset, lineWidth + (TextPaddingX * 2) - _textBoxRectangle.Width)
                : (int)Math.Min(_horizontalOffset, lineWidth);

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
                    break;
                default:
                    break;
            }

            _action = NumberInputAction.None;
        }

        protected override void OnClick(MouseEventArgs e) {
            SelectAll();
            base.OnClick(e);
        }

        protected override void HandleEnter() {
            OnEnterPressed();
        }

        protected virtual void OnEnterPressed() {
            UnsetFocus();
            EnterPressed?.Invoke(this, EventArgs.Empty);
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

        private void OnInputFocusChanged(object sender, ValueEventArgs<bool> e) {
            if (!e.Value) {
                Text = Value.ToString(NumberFormatInfo.InvariantInfo);
                _horizontalOffset = 0;
                Invalidate();
            }
        }

        private void OnTextChanged(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(_text)) {
                return;
            }

            StringBuilder numericBuilder = new StringBuilder(_text.Length);
            ReadOnlySpan<char> input = _text.AsSpan();
            bool hasDecimal = false;

            foreach (char c in input) {
                if (numericBuilder.Length == 0) {
                    if (c == '+' || c == '-') {
                        _ = numericBuilder.Append(c);
                        continue;
                    }
                }

                if (c >= '0' && c <= '9') {
                    _ = numericBuilder.Append(c);
                } else if (c == '.' && !hasDecimal) {
                    _ = numericBuilder.Append(c);
                    hasDecimal = true;
                }
            }

            if (InvariantUtil.TryParseFloat(numericBuilder.ToString(), out float value)) {
                Value = value;
                OnValueChanged();
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
