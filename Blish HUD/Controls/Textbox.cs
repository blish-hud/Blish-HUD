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
    public class Textbox:Control {

        public event EventHandler<EventArgs> OnTextChanged;
        public event EventHandler<EventArgs> OnEnterPressed;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> OnKeyPressed;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> OnKeyDown;
        public event EventHandler<Microsoft.Xna.Framework.Input.Keys> OnKeyUp;

        private System.Windows.Forms.TextBox _mttb;
        private System.Windows.Forms.Form _focusForm;

        public string Text {
            get { return _mttb.Text; }
            set { _mttb.Text = value; }
        }

        private string _placeholderText = "";
        public string PlaceholderText { get { return _placeholderText; } set { if (_placeholderText != value) { _placeholderText = value; Invalidate(); } } }

        private Color _foreColor = Color.FromNonPremultiplied(239, 240, 239, 255);
        public Color ForeColor { get { return _foreColor; } set { if (_foreColor != value) { _foreColor = value; Invalidate(); } } }

        private TimeSpan lastInvalidate;
        private bool textWasChanged = false;

        private bool _caretVisible = false;
        private bool CaretVisible { get { return _caretVisible; } set { if (_caretVisible != value) { _caretVisible = value; Invalidate(); } } }

        private BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular);
        public BitmapFont Font {
            get => _font;
            set {
                if (_font == value) return;

                _font = value;

                OnPropertyChanged();
            }
        }

        public Textbox() {
            lastInvalidate = DateTime.MinValue.TimeOfDay;

            _mttb = new System.Windows.Forms.TextBox();
            _mttb.Parent = Overlay.Form;
            _mttb.Size = new System.Drawing.Size(300, 20);
            _mttb.Location = new System.Drawing.Point(Overlay.Form.Left - 500);
            _mttb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            _mttb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            _mttb.AutoCompleteCustomSource = new System.Windows.Forms.AutoCompleteStringCollection();
            _mttb.ShortcutsEnabled = true;
            _mttb.TabStop = false;

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
                /* This method is pretty hacked up, so I want to make sure we can keep tabs on
                   it as the application evolves. */
                _focusForm.Opacity   = 0.2f;
                _focusForm.BackColor = System.Drawing.Color.Magenta;
            #endif
            _focusForm.Hide();
            _focusForm.Click += delegate { Textbox_LeftMouseButtonReleased(null, null); };

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
               right (for some silly reason) */
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                e.SuppressKeyPress = true;
            
            this.OnKeyDown?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);

            textWasChanged = true;
            Invalidate();
        }

        private void _mttb_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                this.OnEnterPressed?.Invoke(this, new EventArgs());
            } else {
                // Supress up and down keys because they move the cursor left and
                // right (for some silly reason)
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    e.SuppressKeyPress = true;

                this.OnKeyUp?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);
                this.OnKeyPressed?.Invoke(this, (Microsoft.Xna.Framework.Input.Keys)e.KeyCode);
            }

            textWasChanged = true;
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
            float textWidth = this.Font.MeasureString(finalText).Width;
            while (this.Width - 20 > 0 && textWidth > this.Width - 20) {
                finalText = finalText.Substring(0, finalText.Length - 1);
                textWidth = this.Font.MeasureString(finalText).Width;
            }

            if (_mttb.Text != finalText) {
                _mttb.Text = finalText;
                _mttb.SelectionStart = _mttb.TextLength;
                _mttb.SelectionLength = 0;
                return;
            }

            Invalidate();

            textWasChanged = true;

            this.OnTextChanged?.Invoke(this, e);
        }

        private void Textbox_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            Overlay.Form.Activate();

            _mttb.Select(_mttb.Text.Length, 0);
            _mttb.Focus();
            this.CaretVisible = true;
        }

        public override void Update(GameTime gameTime) {
            _focusForm.Location = this.AbsoluteBounds.Location.ScaleToUi().ToSystemDrawingPoint();
            _focusForm.Size = this.AbsoluteBounds.Size.ScaleToUi().ToSystemDrawingSize();

            this.CaretVisible = _mttb.Focused && (Math.Round(gameTime.TotalGameTime.TotalSeconds) % 2 == 1 || gameTime.TotalGameTime.Subtract(lastInvalidate).TotalSeconds < 0.75);

            if (this.NeedsRedraw && textWasChanged) {
                lastInvalidate = gameTime.TotalGameTime;
                textWasChanged = false;
            }

            base.Update(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(Content.GetTexture("textbox"), bounds.Subtract(new Rectangle(0, 0, 5, 0)), new Rectangle(0, 0, Math.Min(Content.GetTexture("textbox").Width - 5, this.Width - 5), Content.GetTexture("textbox").Height), Color.White);
            spriteBatch.Draw(Content.GetTexture("textbox"), new Rectangle(bounds.Right - 5, bounds.Y, 5, bounds.Height), new Rectangle(Content.GetTexture("textbox").Width - 5, 0, 5, Content.GetTexture("textbox").Height), Color.White);

            bounds.Inflate(-10, -2);

            var phFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Italic);
            

            if (!_mttb.Focused && this.Text.Length == 0)
                Utils.DrawUtil.DrawAlignedText(spriteBatch, phFont, this.PlaceholderText, bounds, Color.LightGray, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);

            Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, this.Text, bounds, Color.FromNonPremultiplied(239, 240, 239, 255), Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);

            if (_mttb.SelectionLength > 0 || _mttb.Focused && this.CaretVisible) {
                int cursorPos = _mttb.SelectionStart;
                float textOffset = this.Font.MeasureString(_mttb.Text.Substring(0, cursorPos)).Width;
                var caretOffset = new Rectangle(bounds.X + (int)textOffset - 2, bounds.Y, bounds.Width, bounds.Height);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, "|", caretOffset, this.ForeColor, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);

                if (_mttb.SelectionLength > 0) {
                    float highlightLeftOffset = this.Font.MeasureString(_mttb.Text.Substring(0, cursorPos)).Width;
                    float highlightRightOffset = this.Font.MeasureString(_mttb.Text.Substring(0, _mttb.SelectionStart + _mttb.SelectionLength)).Width;

                    var selectRegion = new Rectangle(bounds.Left + (int)highlightLeftOffset + 1, bounds.Top + 3, (int)(highlightRightOffset - highlightLeftOffset), bounds.Height - 9);

                    spriteBatch.Draw(ContentService.Textures.Pixel, selectRegion, new Color(92, 80, 103, 150));
                }
            }
        }

    }
}
