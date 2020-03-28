using System;
using System.Drawing;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Represents a textbox control.
    /// </summary>
    public class TextBox : Control {

        private const int STANDARD_CONTROLWIDTH  = 250;
        private const int STANDARD_CONTROLHEIGHT = 27;

        #region Load Static

        private static readonly Texture2D _textureTextbox;

        static TextBox() {
            _textureTextbox = Content.GetTexture("textbox");
        }

        #endregion

        public event EventHandler<EventArgs> TextChanged;
        public event EventHandler<EventArgs> EnterPressed;
        public event EventHandler<Keys> KeyPressed;
        public event EventHandler<Keys> KeyDown;
        public event EventHandler<Keys> KeyUp;

        private string _text = string.Empty;
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private string _placeholderText;
        public string PlaceholderText {
            get => _placeholderText;
            set => SetProperty(ref _placeholderText, value);
        }

        private Color _foreColor = Color.FromNonPremultiplied(239, 240, 239, 255);
        public Color ForeColor {
            get => _foreColor;
            set => SetProperty(ref _foreColor, value);
        }

        private BitmapFont _font = Content.DefaultFont14;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private bool _focused = false;
        public bool Focused {
            get => _focused;
            set => SetProperty(ref _focused, value);
        }

        private int _cursorIndex;
        public int CursorIndex {
            get => _cursorIndex;
            set => SetProperty(ref _cursorIndex, value);
        }

        private int _selectionLength;
        public int SelectionLength {
            get => _selectionLength;
            set => SetProperty(ref _selectionLength, value);
        }

        private bool _multiline;
        public bool Multiline {
            get => _multiline;
            set => SetProperty(ref _multiline, value, true);
        }

        private TimeSpan _lastInvalidate;
        private bool     _textWasChanged = false;
        private bool     _caretVisible   = false;

        public TextBox() {
            _lastInvalidate = DateTime.MinValue.TimeOfDay;

            this.Size = new Point(STANDARD_CONTROLWIDTH, STANDARD_CONTROLHEIGHT);

            Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseLeftMouseButtonReleased;
            Input.Keyboard.KeyPressed += OnGlobalKeyboardKeyPressed;

            BlishHud.Instance.Window.TextInput += WindowOnTextInput;
        }

        private void OnGlobalKeyboardKeyPressed(object sender, KeyboardEventArgs e) {
            switch (e.Key) {
                case Keys.Left:
                    _cursorIndex = Math.Max(0, _cursorIndex - 1);
                    break;
                case Keys.Right:
                    _cursorIndex = Math.Min(_text.Length, _cursorIndex + 1);
                    break;
                case Keys.Back:
                    break;
            }
        }

        private void WindowOnTextInput(object sender, TextInputEventArgs e) {
            InsertChar(e.Character);
        }

        private void InsertChar(char c) {
            if (!_multiline && c == '\n') {
                return;
            }

            _text = _text.Substring(0, _cursorIndex) + c + _text.Substring(_cursorIndex + _selectionLength);

            _cursorIndex++;
        }

        private void OnGlobalMouseLeftMouseButtonReleased(object sender, MouseEventArgs e) {
            this.Focused = _mouseOver;
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput() { return CaptureType.Mouse; }

        public override void DoUpdate(GameTime gameTime) {
            // Determines if the blinking caret is currently visible
            _caretVisible = _focused && Math.Round(gameTime.TotalGameTime.TotalSeconds) % 2 == 1 || gameTime.TotalGameTime.Subtract(_lastInvalidate).TotalSeconds < 0.75;

            if (this.LayoutState == LayoutState.Invalidated && _textWasChanged) {
                _lastInvalidate = gameTime.TotalGameTime;
                _textWasChanged = false;
            }
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

            var textBounds = new Rectangle(Point.Zero, _size);
            textBounds.Inflate(-10, -2);

            // Draw the Textbox placeholder text
            if (!_focused && _text.Length == 0) {
                var phFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic);
                spriteBatch.DrawStringOnCtrl(this, _placeholderText, phFont, textBounds, Color.LightGray);
            }

            // Draw the Textbox text
            spriteBatch.DrawStringOnCtrl(this, this.Text, _font, textBounds, _foreColor);

            if (_selectionLength > 0) {
                float highlightLeftOffset  = _font.MeasureString(_text.Substring(0, _cursorIndex)).Width + textBounds.Left;
                float highlightRightOffset = _font.MeasureString(_text.Substring(0, _cursorIndex + _selectionLength)).Width;

                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        new Rectangle((int)highlightLeftOffset - 1, 3, (int)highlightRightOffset, _size.Y - 9),
                                        new Color(92, 80, 103, 150));
            } else if (_focused && _caretVisible) {
                int   cursorPos   = _cursorIndex;
                float textOffset  = this.Font.MeasureString(_text.Substring(0, cursorPos)).Width;
                var   caretOffset = new Rectangle(textBounds.X + (int)textOffset - 2, textBounds.Y, textBounds.Width, textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", _font, caretOffset, this.ForeColor);
            }
        }

    }
}
