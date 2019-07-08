using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class MultilineTextBox : Control {

        #region Old Code
        //public MultilineTextBox() : base() {
        //    _mttb.Location = new System.Drawing.Point(50, 400);
        //    _mttb.Size = new System.Drawing.Size(_size.X, _size.Y);
        //    _mttb.Multiline = true;
        //    _mttb.AcceptsReturn = true;
        //    _mttb.BorderStyle = System.Windows.Forms.BorderStyle.None;
        //    _mttb.WordWrap = false;

        //    _mttb.Font = new System.Drawing.Font(_fontCollection.Families[0], 14);
        //}

        //public override void Update(GameTime gameTime) {
        //    base.Update(gameTime);

        //    _mttb.BringToFront();
        //    _mttb.Size = new System.Drawing.Size(_size.X, _size.Y);
        //}

        //protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
        //    base.Paint(spriteBatch, bounds);
        //}

        #endregion

        private System.Windows.Forms.TextBox _mttb;
        private System.Windows.Forms.Form _ctrlForm;



        protected static System.Drawing.Text.PrivateFontCollection _fontCollection;

        static MultilineTextBox() {
            _fontCollection = new System.Drawing.Text.PrivateFontCollection();
            _fontCollection.AddFontFile("menomonia.ttf");
        }

        public MultilineTextBox() {
            _ctrlForm = new System.Windows.Forms.Form {
                TopMost = true,
                Size = new System.Drawing.Size(1, 1),
                Location = new System.Drawing.Point(-200, -200),
                ShowInTaskbar = false,
                AllowTransparency = true,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                BackColor = System.Drawing.Color.Blue,
                Opacity = 0.95, 
            };

            _mttb = new System.Windows.Forms.TextBox() {
                Parent = _ctrlForm,
                Size = new System.Drawing.Size(300, 20),
                Location = new System.Drawing.Point(BlishHud.Form.Left - 500),
                AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append,
                AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource,
                AutoCompleteCustomSource = new System.Windows.Forms.AutoCompleteStringCollection(),
                ShortcutsEnabled = true,
                TabStop = false,
                Dock = System.Windows.Forms.DockStyle.Fill,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font(_fontCollection.Families[0], 14),
                BackColor = System.Drawing.Color.Black,
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                Multiline = true
            };

            _ctrlForm.Hide();
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);

            _ctrlForm.Show();
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            _ctrlForm.Hide();
        }

        public override void DoUpdate(GameTime gameTime) {
            var focusLocation = this.AbsoluteBounds.Location.ScaleToUi().ToSystemDrawingPoint();
            focusLocation.Offset(BlishHud.Form.Location);

            _ctrlForm.Location = focusLocation;
            _ctrlForm.Size = this.AbsoluteBounds.Size.ScaleToUi().ToSystemDrawingSize();

            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            
        }
    }
}
