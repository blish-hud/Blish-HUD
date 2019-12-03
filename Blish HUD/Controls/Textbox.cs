using System;
using System.Drawing;
using System.Windows.Forms;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        #region Load Static

        private static MouseInterceptor           _sharedInterceptor;
        private static System.Windows.Forms.Label _sharedUnfocusLabel;

        private static readonly Texture2D _textureTextbox;

        static TextBox() {
              _sharedInterceptor                         =  new MouseInterceptor();
              _sharedInterceptor.MouseLeft               += delegate { _sharedInterceptor.Hide(); };
              _sharedInterceptor.LeftMouseButtonReleased += delegate { _sharedInterceptor.Hide(); };

              // This is needed to ensure that the textbox is *actually* unfocused
              _sharedUnfocusLabel = new System.Windows.Forms.Label {
                  Location = new System.Drawing.Point(-200, 0),
                  Parent   = BlishHud.Form
              };

            _textureTextbox = Content.GetTexture("textbox");
        }

        #endregion

        public static readonly DesignStandard Standard = new DesignStandard(/*          Size */ new Point(250, 27),
                                                                            /*   PanelOffset */ new Point(5, 2),
                                                                            /* ControlOffset */ Control.ControlStandard.ControlOffset);

        public event EventHandler<EventArgs> TextChanged;
        public event EventHandler<EventArgs> EnterPressed;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> KeyPressed;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> KeyDown;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> KeyUp;

        protected System.Windows.Forms.TextBox _mttb;

        public string Text {
            get => _mttb.Text;
            set => _mttb.Text = value;
        }

        protected string _placeholderText;
        public string PlaceholderText {
            get => _placeholderText;
            set => SetProperty(ref _placeholderText, value);
        }

        protected Color _foreColor = Color.FromNonPremultiplied(239, 240, 239, 255);
        public Color ForeColor {
            get => _foreColor;
            set => SetProperty(ref _foreColor, value);
        }

        private TimeSpan _lastInvalidate;
        private bool _textWasChanged = false;

        protected bool _caretVisible = false;
        private bool CaretVisible {
            get => _caretVisible;
            set => SetProperty(ref _caretVisible, value);
        }

        protected BitmapFont _font = Content.DefaultFont14;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        /// <summary>
        /// [NOT THREAD-SAFE]
        /// </summary>
        public TextBox() {
            _lastInvalidate = DateTime.MinValue.TimeOfDay;

            _mttb = new System.Windows.Forms.TextBox() {
                Parent                   = BlishHud.Form,
                Size                     = new Size(20, 20),
                Location                 = new System.Drawing.Point(-500),
                AutoCompleteMode         = AutoCompleteMode.Append,
                AutoCompleteSource       = AutoCompleteSource.CustomSource,
                AutoCompleteCustomSource = new AutoCompleteStringCollection(),
                ShortcutsEnabled         = true,
                TabStop                  = false
            };

            _sharedInterceptor.LeftMouseButtonReleased += delegate {
                if (_sharedInterceptor.ActiveControl == this) {
                    Textbox_LeftMouseButtonReleased(null, null);
                }
            };

            _mttb.TextChanged += InternalTextBox_TextChanged;
            _mttb.KeyDown     += InternalTextBox_KeyDown;
            _mttb.KeyUp       += InternalTextBox_KeyUp;

            this.Size = Standard.Size;

            Input.Mouse.LeftMouseButtonPressed  += Input_MouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += Input_MouseButtonPressed;
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            bool restoreFocus = _mttb.Focused;

            _sharedInterceptor.Show(this);

            if (restoreFocus) {
                Textbox_LeftMouseButtonReleased(null, null);
            }

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            if (_sharedInterceptor.ActiveControl == this) {
                _sharedInterceptor.Hide();
            }

            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput() { return CaptureType.Mouse; }

        private void InternalTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            /* Supress up and down keys because they move the cursor left and
               right for some silly reason */
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) {
                e.SuppressKeyPress = true;
            }
            
            this.KeyDown?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);

            _textWasChanged = true;
            Invalidate();
        }

        private void InternalTextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                this.EnterPressed?.Invoke(this, new EventArgs());
            } else {
                /* Supress up and down keys because they move the cursor left and
                   right for some silly reason */
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    e.SuppressKeyPress = true;

                this.KeyUp?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);
                this.KeyPressed?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);
            }

            _textWasChanged = true;
            Invalidate();
        }

        private void Input_MouseButtonPressed(object sender, MouseEventArgs e) {
            if (_mttb.Focused && !this.MouseOver) {
                _sharedUnfocusLabel.Select();
                GameService.GameIntegration.FocusGw2();
                Invalidate();
            }
        }

        private void InternalTextBox_TextChanged(object sender, EventArgs e) {
            string finalText = _mttb.Text;

            foreach (char c in _mttb.Text) {
                if (this.Font.GetCharacterRegion(c) == null) { 
                    finalText = finalText.Replace(c.ToString(), "");
                }
            }

            // TODO: Make sure to prevent this from looping forever if the textbox is too skinny for any characters (need to evaluate all cases)
            float textWidth = _font.MeasureString(finalText).Width;
            while (_size.X - 20 > 0 && textWidth > _size.X - 20) {
                finalText = finalText.Substring(0, finalText.Length - 1);
                textWidth = _font.MeasureString(finalText).Width;
            }

            if (_mttb.Text != finalText) {
                _mttb.Text = finalText;
                _mttb.SelectionStart = _mttb.TextLength;
                _mttb.SelectionLength = 0;
                return;
            }

            Invalidate();

            _textWasChanged = true;

            this.TextChanged?.Invoke(this, e);
        }

        private void Textbox_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            BlishHud.Form.Activate();

            _mttb.Select(_mttb.Text.Length, 0);
            _mttb.Focus();
            this.CaretVisible = true;
        }

        public override void DoUpdate(GameTime gameTime) {
            // Keep MouseInterceptor on top of us
            if (_sharedInterceptor.Visible && _sharedInterceptor.ActiveControl == this) {
                _sharedInterceptor.Show(this);
            }

            // Determines if the blinking caret is currently visible
            this.CaretVisible = _mttb.Focused && (Math.Round(gameTime.TotalGameTime.TotalSeconds) % 2 == 1 || gameTime.TotalGameTime.Subtract(_lastInvalidate).TotalSeconds < 0.75);

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
                                   new Rectangle(
                                                 _textureTextbox.Width - 5, 0,
                                                 5, _textureTextbox.Height
                                                ));

            var textBounds = new Rectangle(Point.Zero, _size);
            textBounds.Inflate(-10, -2);

            // Draw the Textbox placeholder text
            if (!_mttb.Focused && this.Text.Length == 0) {
                var phFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic);
                spriteBatch.DrawStringOnCtrl(this, _placeholderText, phFont, textBounds, Color.LightGray);
            }

            // Draw the Textbox text
            spriteBatch.DrawStringOnCtrl(this, this.Text, _font, textBounds, Color.FromNonPremultiplied(239, 240, 239, 255));
            
            if (_mttb.SelectionLength > 0 ) {
                float highlightLeftOffset = _font.MeasureString(_mttb.Text.Substring(0, _mttb.SelectionStart)).Width + textBounds.Left;
                float highlightRightOffset = _font.MeasureString(_mttb.Text.Substring(0, _mttb.SelectionStart + _mttb.SelectionLength)).Width;
                    
                spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        new Rectangle((int) highlightLeftOffset - 1, 3, (int) highlightRightOffset, _size.Y - 9),
                                        new Color(92, 80, 103, 150));
            } else if (_mttb.Focused && this.CaretVisible) {
                int cursorPos = _mttb.SelectionStart;
                float textOffset = this.Font.MeasureString(_mttb.Text.Substring(0, cursorPos)).Width;
                var caretOffset = new Rectangle(textBounds.X + (int)textOffset - 2, textBounds.Y, textBounds.Width, textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", _font, caretOffset, this.ForeColor);
            }
        }

    }
}
