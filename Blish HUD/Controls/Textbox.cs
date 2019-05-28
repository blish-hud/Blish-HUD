using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.WinApi;
using Blish_HUD;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Blish_HUD.Controls {
    public class TextBox:Control {

        public event EventHandler<EventArgs> OnTextChanged;
        public event EventHandler<EventArgs> OnEnterPressed;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> OnKeyPressed;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> OnKeyDown;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> OnKeyUp;

        protected System.Windows.Forms.TextBox _mttb;
        protected Form _focusForm;

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

        public TextBox() {
            _lastInvalidate = DateTime.MinValue.TimeOfDay;

            _mttb = new System.Windows.Forms.TextBox() {
                Parent = Overlay.Form,
                Size = new Size(300, 20),
                Location = new System.Drawing.Point(Overlay.Form.Left - 500),
                AutoCompleteMode = AutoCompleteMode.Append,
                AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource,
                AutoCompleteCustomSource = new System.Windows.Forms.AutoCompleteStringCollection(),
                ShortcutsEnabled = true,
                TabStop = false
            };

            _focusForm = new Form {
                TopMost           = true,
                Size              = new Size(1, 1),
                Location          = new System.Drawing.Point(-200, -200),
                ShowInTaskbar     = false,
                AllowTransparency = true,
                FormBorderStyle   = FormBorderStyle.None,
                Opacity           = 0.01f,
                BackColor         = System.Drawing.Color.Black
            };

            #if DEBUG
                /* This method is pretty hacked up, so I want to make
                   sure we can keep tabs on it as the application evolves. */
                _focusForm.Opacity   = 0.2f;
                _focusForm.BackColor = System.Drawing.Color.Magenta;
            #endif

            _focusForm.Hide();
            _focusForm.Click += delegate { Textbox_LeftMouseButtonReleased(null, null); };
            _focusForm.MouseLeave += delegate { _focusForm.Hide(); };
            
            _mttb.TextChanged += _mttb_TextChanged;
            _mttb.KeyDown += _mttb_KeyDown;
            _mttb.KeyUp += _mttb_KeyUp;

            this.Size = new Point(this.Width, 27);

            Input.LeftMouseButtonPressed += Input_LeftMouseButtonPressed;
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);

            _focusForm.Show();
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            _focusForm.Hide();
        }

        public override void TriggerKeyboardInput(KeyboardMessage e) {
        }

        protected override CaptureType CapturesInput() { return CaptureType.Mouse | CaptureType.ForceNone; }

        private void _mttb_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            /* Supress up and down keys because they move the cursor left and
               right for some silly reason */
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                e.SuppressKeyPress = true;
            
            this.OnKeyDown?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);

            _textWasChanged = true;
            Invalidate();
        }

        private void _mttb_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                this.OnEnterPressed?.Invoke(this, new EventArgs());
            } else {
                /* Supress up and down keys because they move the cursor left and
               right for some silly reason */
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    e.SuppressKeyPress = true;

                this.OnKeyUp?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);
                this.OnKeyPressed?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);
            }

            _textWasChanged = true;
            Invalidate();
        }

        private void Input_LeftMouseButtonPressed(object sender, MouseEventArgs e) {
            if (_mttb.Focused && !this.MouseOver) {
                Overlay.UnfocusLabel.Select();
                Invalidate();
            }
        }

        private void _mttb_TextChanged(object sender, EventArgs e) {
            string finalText = _mttb.Text;

            foreach (char c in _mttb.Text.ToCharArray()) {
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

            this.OnTextChanged?.Invoke(this, e);
        }

        private void Textbox_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            Overlay.Form.Activate();

            _mttb.Select(_mttb.Text.Length, 0);
            _mttb.Focus();
            this.CaretVisible = true;
        }

        public override void DoUpdate(GameTime gameTime) {
            var focusLocation = this.AbsoluteBounds.Location.ScaleToUi().ToSystemDrawingPoint();
            focusLocation.Offset(Overlay.Form.Location);

            _focusForm.Location = focusLocation;
            _focusForm.Size = this.AbsoluteBounds.Size.ScaleToUi().ToSystemDrawingSize();

            // Determines if the blinking caret is currently visible
            this.CaretVisible = _mttb.Focused && (Math.Round(gameTime.TotalGameTime.TotalSeconds) % 2 == 1 || gameTime.TotalGameTime.Subtract(_lastInvalidate).TotalSeconds < 0.75);

            if (this.LayoutIsInvalid && _textWasChanged) {
                _lastInvalidate = gameTime.TotalGameTime;
                _textWasChanged = false;
            }

            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(
                                   this,
                                   Content.GetTexture("textbox"),
                                   new Rectangle(Point.Zero, _size - new Point(5, 0)),
                                   new Rectangle(0, 0, Math.Min(Content.GetTexture("textbox").Width - 5, _size.X - 5), Content.GetTexture("textbox").Height)
                                  );

            spriteBatch.DrawOnCtrl(
                                   this, Content.GetTexture("textbox"),
                                   new Rectangle(_size.X - 5, 0, 5, _size.Y),
                                   new Rectangle(
                                                 Content.GetTexture("textbox").Width - 5, 0,
                                                 5, Content.GetTexture("textbox").Height
                                                )
                                  );

            var textBounds = new Rectangle(Point.Zero, _size);
            textBounds.Inflate(-10, -2);

            var phFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic);

            // Draw the Textbox placeholder text
            if (!_mttb.Focused && this.Text.Length == 0)
                spriteBatch.DrawStringOnCtrl(this, _placeholderText, phFont, textBounds, Color.LightGray);

            // Draw the Textbox text
            spriteBatch.DrawStringOnCtrl(this, this.Text, _font, textBounds, Color.FromNonPremultiplied(239, 240, 239, 255));
            
            if (_mttb.SelectionLength > 0 ) {
                float highlightLeftOffset = _font.MeasureString(_mttb.Text.Substring(0, _mttb.SelectionStart)).Width + textBounds.Left;
                float highlightRightOffset = _font.MeasureString(_mttb.Text.Substring(0, _mttb.SelectionStart + _mttb.SelectionLength)).Width;
                    
                spriteBatch.DrawOnCtrl(
                                        this,
                                        ContentService.Textures.Pixel,
                                        new Rectangle((int) highlightLeftOffset - 1, 3, (int) highlightRightOffset, _size.Y - 9),
                                        new Color(92, 80, 103, 150)
                                        );
            } else if (_mttb.Focused && this.CaretVisible) {
                int cursorPos = _mttb.SelectionStart;
                float textOffset = this.Font.MeasureString(_mttb.Text.Substring(0, cursorPos)).Width;
                var caretOffset = new Rectangle(textBounds.X + (int)textOffset - 2, textBounds.Y, textBounds.Width, textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", _font, caretOffset, this.ForeColor);
            }
        }

    }
}
