/*
    This code is heavily adapted from the Myra TextBox (https://github.com/rds1983/Myra/blob/a9dbf7a1ceedc19f9e416c754eaf38e89a89a746/src/Myra/Graphics2D/UI/TextBox.cs)

    MIT License

    Copyright (c) 2017-2020 The Myra Team

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

 */
 
using System;
using System.Linq;
using System.Windows.Forms;
using Blish_HUD.Controls.Resources;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Represents a textbox control.
    /// </summary>
    public class TextBox : Control {

        private static readonly Logger Logger = Logger.GetLogger<TextBox>();

        private const int STANDARD_CONTROLWIDTH  = 250;
        private const int STANDARD_CONTROLHEIGHT = 27;

        private const char NEWLINE = '\n';

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
        public event EventHandler<ValueEventArgs<int>> CursorIndexChanged;
        
        protected void OnCursorIndexChanged(ValueEventArgs<int> e) {
            UpdateScrolling();

            CursorIndexChanged?.Invoke(this, e);
        }

        protected void OnTextChanged(ValueChangedEventArgs<string> e) {
            TextChanged?.Invoke(this, e);
        }

        private string _text = string.Empty;
        public string Text {
            get => _text;
            set => SetText(value, false);
        }

        private string UserText {
            get => _text;
            set => SetText(value, true);
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
            set => SetProperty(ref _font, value, true);
        }

        private bool _focused = false;
        public bool Focused {
            get => _focused;
            set => SetProperty(ref _focused, value);
        }

        private int _selectionStart;
        public int SelectionStart {
            get => _selectionStart;
            set => SetProperty(ref _selectionStart, value);
        }

        private int _selectionEnd;
        public int SelectionEnd {
            get => _selectionEnd;
            set => SetProperty(ref _selectionEnd, value);
        }

        private int _cursorIndex;
        public int CursorIndex {
            get => _cursorIndex;
            set {
                if (SetProperty(ref _cursorIndex, value)) {
                    OnCursorIndexChanged(new ValueEventArgs<int>(value));
                }
            }
        }

        private bool _multiline;
        public bool Multiline {
            get => _multiline;
            set => SetProperty(ref _multiline, value, true);
        }

        private bool _wrapText;
        public bool WrapText {
            get => _wrapText;
            set => SetProperty(ref _wrapText, value, true);
        }

        public int Length => _text.Length;

        private bool IsShiftDown         => GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift) || GameService.Input.Keyboard.KeysDown.Contains(Keys.RightShift);
        private bool IsCtrlDown          => GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftControl) || GameService.Input.Keyboard.KeysDown.Contains(Keys.RightControl);
        private int  CursorWidth         => SystemInformation.CaretWidth;
        private int  CursorBlinkInterval => SystemInformation.CaretBlinkTime;

        private TimeSpan _lastInvalidate;
        private bool     _textWasChanged = false;
        private bool     _caretVisible   = false;
        private bool     _insertMode     = false;

        private bool _suppressRedoStackReset;

        private readonly UndoRedoStack _undoStack = new UndoRedoStack();
        private readonly UndoRedoStack _redoStack = new UndoRedoStack();

        public TextBox() {
            _lastInvalidate = DateTime.MinValue.TimeOfDay;

            this.Size = new Point(STANDARD_CONTROLWIDTH, STANDARD_CONTROLHEIGHT);

            Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseLeftMouseButtonReleased;
            Input.Keyboard.KeyPressed           += OnGlobalKeyboardKeyPressed;
            Input.Keyboard.TextInputAsync       += OnGlobalKeyboardTextInputAsync;
        }

        private void OnGlobalKeyboardTextInputAsync(object sender, ValueEventArgs<string> e) {
            if (!_focused && _enabled) return;

            foreach (char c in e.Value) {
                if (char.IsControl(c)) continue;

                InputChar(c);
            }
        }

        private void DeleteChars(int index, int length) {
            if (length <= 0) return;

            UserText = UserText.Substring(0, index) + UserText.Substring(index + length);
        }


        private bool InsertChars(int index, string value) {
            if (string.IsNullOrEmpty(value)) return false;

            if (string.IsNullOrEmpty(_text)) {
                this.UserText = value;
            } else {
                this.UserText = this.UserText.Substring(0, index) + value + this.UserText.Substring(index);
            }

            return true;
        }

        private bool InsertChar(int index, char value) {
            if (string.IsNullOrEmpty(_text)) {
                this.UserText = value.ToString();
            } else {
                this.UserText = UserText.Substring(0, index) + value + this.UserText.Substring(index);
            }

            return true;
        }

        public void Insert(int index, string value) {
            if (string.IsNullOrEmpty(value)) return;

            if (InsertChars(index, value)) {
                _undoStack.MakeInsert(index, value.Length);
                this.CursorIndex += value.Length;
            }
        }

        public void Replace(int index, int length, string value) {
            if (length <= 0) {
                Insert(index, value);
                return;
            }

            if (string.IsNullOrEmpty(value)) {
                Delete(index, length);
                return;
            }

            _undoStack.MakeReplace(value, index, length, value.Length);
            this.UserText = this.UserText.Substring(0, index) + value + this.UserText.Substring(index + length);
        }

        public void ReplaceAll(string value) {
            Replace(0, 
                    string.IsNullOrEmpty(value) 
                        ? 0 
                        : value.Length,
                    value);
        }

        private bool Delete(int index, int length) {
            if (index < 0 || index >= this.Length || length < 0) return false;

            _undoStack.MakeDelete(_text, index, length);
            DeleteChars(index, length);

            return true;
        }

        private void DeleteSelection() {
            if (_selectionStart == _selectionEnd) return;

            if (_selectionStart < _selectionEnd) {
                Delete(_selectionStart, _selectionEnd - _selectionStart);
                this.SelectionEnd = this.CursorIndex = this.SelectionStart;
            } else {
                Delete(_selectionEnd, _selectionStart - _selectionEnd);
                this.SelectionStart = this.CursorIndex = this.SelectionEnd;
            }
        }

        private bool Paste(string value) {
            DeleteSelection();

            if (InsertChars(_cursorIndex, value)) {
                _undoStack.MakeInsert(_cursorIndex, value.Length);
                this.CursorIndex += value.Length;
                return true;
            }

            return false;
        }

        private void InputChar(char value) {
            if (!_multiline && value == NEWLINE) return;

            if (_insertMode && _selectionStart == _selectionEnd && _cursorIndex < this.Length) {
                _undoStack.MakeReplace(_text, _cursorIndex, 1, 1);
                DeleteChars(_cursorIndex, 1);

                if (InsertChar(_cursorIndex, value)) {
                    UserSetCursorIndex(_cursorIndex + 1);
                }
            } else {
                DeleteSelection();

                if (InsertChar(_cursorIndex, value)) {
                    _undoStack.MakeInsert(_cursorIndex, 1);
                    UserSetCursorIndex(_cursorIndex + 1);
                }
            }

            ResetSelection();
        }

        private void UndoRedo(UndoRedoStack undoStack, UndoRedoStack redoStack) {
            if (undoStack.Stack.Count == 0) return;

            var record = undoStack.Stack.Pop();

            try {
                _suppressRedoStackReset = true;

                switch (record.OperationType) {
                    case OperationType.Insert:
                        redoStack.MakeDelete(_text, record.Index, record.Length);
                        DeleteChars(record.Index, record.Length);
                        UserSetCursorIndex(record.Index);
                        break;
                    case OperationType.Delete:
                        if (InsertChars(record.Index, record.Data)) {
                            redoStack.MakeInsert(record.Index, record.Data.Length);
                            UserSetCursorIndex(record.Index + record.Data.Length);
                        }

                        break;
                    case OperationType.Replace:
                        redoStack.MakeReplace(_text, record.Index, record.Length, record.Data.Length);
                        DeleteChars(record.Index, record.Length);
                        InsertChars(record.Index, record.Data);
                        break;
                }
            } finally {
                _suppressRedoStackReset = false;
            }

            ResetSelection();
        }

        private void UserSetCursorIndex(int newIndex) {
            if (newIndex > this.Length) {
                newIndex = this.Length;
            }

            if (newIndex < 0) {
                newIndex = 0;
            }

            this.CursorIndex = newIndex;
        }

        private void ResetSelection() {
            this.SelectionStart = this.SelectionEnd = this.CursorIndex;
        }

        private void UpdateSelection() {
            this.SelectionEnd = _cursorIndex;
        }

        private void UpdateSelectionIfShiftDown() {
            if (this.IsShiftDown) {
                UpdateSelection();
            } else {
                ResetSelection();
            }
        }

        private void MoveLine(int delta) {

        }

        private void SelectAll() {
            this.SelectionStart = 0;
            this.SelectionEnd   = this.Length;
        }

        private string ProcessText(string value) {
            value = value?.Replace("\r", string.Empty);

            return value;
        }

        private bool SetText(string value, bool byUser) {
            string prevText = _text;

            value = ProcessText(value);

            if (!SetProperty(ref _text, value)) return false;

            // TODO: Update formatted text?

            if (!byUser) {
                this.CursorIndex = this.SelectionStart = this.SelectionEnd = 0;
            }

            if (!_suppressRedoStackReset) {
                _redoStack.Reset();
            }

            // TODO: Invalidate measure

            _textWasChanged = true;

            OnTextChanged(new ValueChangedEventArgs<string>(prevText, value));

            return true;
        }

        private void OnGlobalKeyboardKeyPressed(object sender, KeyboardEventArgs e) {
            if (!_focused && _enabled) return;

            bool shiftDown = this.IsShiftDown;
            bool ctrlDown  = this.IsCtrlDown;

            switch (e.Key) {
                case Keys.C:
                    if (ctrlDown) HandleCopy();
                    break;
                case Keys.X:
                    if (ctrlDown) HandleCut();
                    break;
                case Keys.V:
                    if (ctrlDown) HandlePaste();
                    break;
                case Keys.Insert:
                    _insertMode = !_insertMode;
                    break;
                case Keys.Z:
                    if (ctrlDown) HandleUndo();
                    break;
                case Keys.Y:
                    if (ctrlDown) HandleRedo();
                    break;
                case Keys.A:
                    if (ctrlDown) SelectAll();
                    break;
                case Keys.Left:
                    if (_cursorIndex > 0) {
                        UserSetCursorIndex(_cursorIndex - 1);
                        UpdateSelectionIfShiftDown();
                    }
                    break;
                case Keys.Right:
                    if (_cursorIndex < this.Length) {
                        UserSetCursorIndex(_cursorIndex + 1);
                        UpdateSelectionIfShiftDown();
                    }
                    break;
                case Keys.Up:
                    MoveLine(-1);
                    break;
                case Keys.Down:
                    MoveLine(1);
                    break;
                case Keys.Back:
                    HandleBackspace();
                    break;
                case Keys.Delete:
                    HandleDelete();
                    break;
                case Keys.Home:
                    HandleHome(ctrlDown);
                    break;
                case Keys.End:
                    HandleEnd(ctrlDown);
                    break;
                case Keys.Enter:
                    InputChar(NEWLINE);
                    break;
            }
        }

        private void HandleCopy() {
            if (_selectionEnd != _selectionStart) {
                int selectStart = Math.Min(_selectionStart, _selectionEnd);
                int selectEnd   = Math.Max(_selectionStart, _selectionEnd);

                string clipboardText = _text.Substring(selectStart, selectEnd - selectStart);

                try {
                    Clipboard.SetText(clipboardText);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to set clipboard text to {clipboardText}!", clipboardText);
                }
            }
        }

        private void HandleCut() {
            HandleCopy();
            DeleteSelection();
        }

        private void HandlePaste() {
            string clipboardText = string.Empty;

            try {
                clipboardText = Clipboard.GetText();
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to read clipboard text from system clipboard!");
            }

            if (!string.IsNullOrEmpty(clipboardText)) {
                Paste(clipboardText);
            }
        }

        private void HandleUndo() {
            UndoRedo(_undoStack, _redoStack);
        }

        private void HandleRedo() {
            UndoRedo(_redoStack, _undoStack);
        }

        private void HandleBackspace() {
            if (_selectionStart == _selectionEnd) {
                if (Delete(_cursorIndex - 1, 1)) {
                    UserSetCursorIndex(_cursorIndex - 1);
                    ResetSelection();
                }
            } else {
                DeleteSelection();
            }
        }

        private void HandleDelete() {
            if (_selectionStart == _selectionEnd) {
                Delete(_cursorIndex, 1);
            } else {
                DeleteSelection();
            }
        }

        private void HandleHome(bool ctrlDown) {
            int newIndex = 0;

            if (!ctrlDown && !string.IsNullOrEmpty(_text)) {
                newIndex = _cursorIndex;

                while (newIndex > 0 && (newIndex - 1 >= this.Length || -Text[newIndex - 1] != NEWLINE)) {
                    --newIndex;
                }
            }

            UserSetCursorIndex(newIndex);
            UpdateSelectionIfShiftDown();
        }

        private void HandleEnd(bool ctrlDown) {
            int newIndex = this.Length;

            if (!ctrlDown) {
                while (newIndex < this.Length && Text[newIndex] != NEWLINE) {
                    ++newIndex;
                }
            }

            UserSetCursorIndex(newIndex);
            UpdateSelectionIfShiftDown();
        }

        private void UpdateScrolling() {

        }

        private void OnGlobalMouseLeftMouseButtonReleased(object sender, Input.MouseEventArgs e) {
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

            if (_textWasChanged) {
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

            int selectionStart  = Math.Min(_selectionStart, _selectionEnd);
            int selectionLength = Math.Abs(_selectionStart - _selectionEnd);

            if (selectionLength > 0) {
                float highlightLeftOffset = _font.MeasureString(_text.Substring(0, selectionStart)).Width + textBounds.Left;
                float highlightWidth      = _font.MeasureString(_text.Substring(selectionStart, selectionLength)).Width;

                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        new Rectangle((int)highlightLeftOffset - 1, 3, (int)highlightWidth, _size.Y - 9),
                                        new Color(92, 80, 103, 150));
            } else if (_focused /*&& _caretVisible*/) {
                int   cursorPos   = _cursorIndex;
                float textOffset  = this.Font.MeasureString(_text.Substring(0, cursorPos)).Width;
                var   caretOffset = new Rectangle(textBounds.X + (int)textOffset - 2, textBounds.Y, textBounds.Width, textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", _font, caretOffset, Color.Magenta);
            }
        }

    }
}
