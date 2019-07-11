using System;
using System.Windows.Forms;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;

namespace Blish_HUD {

    /// <summary>
    /// Used to force application focus when the mouse clicks in a specific area.
    /// </summary>
    public sealed class MouseInterceptor {

        #region Mouse Events

        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseEntered;
        public event EventHandler<MouseEventArgs> MouseLeft;

        private void OnLeftMouseButtonPressed(MouseEventArgs e) {
            LeftMouseButtonPressed?.Invoke(this, e);
        }

        private void OnLeftMouseButtonReleased(MouseEventArgs e) {
            LeftMouseButtonReleased?.Invoke(this, e);
        }

        private void OnRightMouseButtonPressed(MouseEventArgs e) {
            RightMouseButtonPressed?.Invoke(this, e);
        }

        private void OnRightMouseButtonReleased(MouseEventArgs e) {
            RightMouseButtonReleased?.Invoke(this, e);
        }

        private void OnMouseEntered(MouseEventArgs e) {
            MouseEntered?.Invoke(this, e);
        }

        private void OnMouseLeft(MouseEventArgs e) {
            MouseLeft?.Invoke(this, e);
        }

        #endregion

        private readonly Form _backingForm;

        public Controls.Control ActiveControl { get; private set; }

        public bool Visible => _backingForm.Visible;

        public void Show(Controls.Control control) {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            var focusLocation = control.AbsoluteBounds.Location.ScaleToUi().ToSystemDrawingPoint();
            focusLocation.Offset(BlishHud.Form.Location);

            var focusSize = control.AbsoluteBounds.Size.ScaleToUi().ToSystemDrawingSize();

            this.Show(focusLocation, focusSize);

            this.ActiveControl = control;
        }

        public void Show(System.Drawing.Point location, System.Drawing.Size size) {
            this.ActiveControl = null;

            _backingForm.Location = location;
            _backingForm.Size     = size;

            _backingForm.Show();
        }

        public void Hide() {
            _backingForm.Hide();
        }

        public MouseInterceptor() {
            _backingForm = new System.Windows.Forms.Form {
                TopMost           = true,
                Size              = new System.Drawing.Size(1, 1),
                Location          = new System.Drawing.Point(-200, -200),
                ShowInTaskbar     = false,
                AllowTransparency = true,
                FormBorderStyle   = System.Windows.Forms.FormBorderStyle.None,
#if !DEBUG
                Opacity   = 0.01f,
                BackColor = System.Drawing.Color.Black,
#else
                /* This method is pretty hacked up, so I want to make
                   sure we can keep tabs on it as the application evolves. */
                Opacity   = 0.2f,
                BackColor = System.Drawing.Color.Magenta,
#endif
            };

            _backingForm.Hide();

            _backingForm.MouseEnter += BackingFormOnMouseEnter;
            _backingForm.MouseLeave += BackingFormOnMouseLeave;
            _backingForm.MouseDown  += BackingFormOnMouseDown;
            _backingForm.MouseUp    += BackingFormOnMouseUp;
        }

        private void BackingFormOnMouseEnter(object sender, EventArgs e) {
            OnMouseEntered(new MouseEventArgs(GameService.Input.MouseState));
        }

        private void BackingFormOnMouseLeave(object sender, EventArgs e) {
            OnMouseLeft(new MouseEventArgs(GameService.Input.MouseState));
        }

        private void BackingFormOnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                OnLeftMouseButtonPressed(new MouseEventArgs(GameService.Input.MouseState));
            } else if (e.Button == MouseButtons.Right) {
                OnRightMouseButtonPressed(new MouseEventArgs(GameService.Input.MouseState));
            }
        }

        private void BackingFormOnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                OnLeftMouseButtonReleased(new MouseEventArgs(GameService.Input.MouseState));
            } else if (e.Button == MouseButtons.Right) {
                OnRightMouseButtonReleased(new MouseEventArgs(GameService.Input.MouseState));
            }
        }

    }
}
