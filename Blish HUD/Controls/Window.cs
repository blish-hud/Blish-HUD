using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Blish_HUD.Controls {
    public class Window:Container {

        public event EventHandler<EventArgs> Shown;

        private string _title = "No Title";
        public string Title { get { return _title; } set { _title = value; Invalidate(); } }

        protected bool Dragging = false;
        protected Point DragStart = Point.Zero;

        private bool _hoverClose = false;
        protected bool HoverClose {
            get => _hoverClose;
            private set {
                if (_hoverClose == value) return;

                _hoverClose = value;
                Invalidate();
            }
        }

        protected bool TopMost { get; set; }

        protected Rectangle ExitBounds;

        protected int TitleBarHeight = 0;
        private Glide.Tween fade;

        protected Rectangle TitleBarBounds;

        private Panel _activePanel;
        public Panel ActivePanel {
            get => _activePanel;
            set {
                // TODO: All controls should have a `Hide()` and `Show()` method
                if (_activePanel != null) {
                    _activePanel.Visible = false;
                    _activePanel.Parent = null;
                }

                if (value == null) return;

                _activePanel = value;

                _activePanel.Parent = this;
                _activePanel.Location = Point.Zero;
                _activePanel.Size = this.ContentRegion.Size;

                _activePanel.Visible = true;
            }
        }

        public Window() {
            this.LeftMouseButtonPressed += Window_LeftMouseButtonPressed;
            this.LeftMouseButtonReleased += Window_LeftMouseButtonReleased;
            this.Opacity = 0f;
            this.Visible = false;

            this.ZIndex = Screen.WINDOW_BASEZINDEX;

            GameServices.GetService<InputService>().LeftMouseButtonReleased += Window_LeftMouseButtonReleased;

            fade = Animation.Tweener.Tween(this, new { Opacity = 1f }, 0.2f).Repeat().Reflect();
            fade.Pause();

            fade.OnComplete(() => {
                fade.Pause();
                if (this.Opacity <= 0) this.Visible = false;
            });
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        private void Window_LeftMouseButtonPressed(object sender, MouseEventArgs e) {
            if (Input.MouseState.Position.Y < this.Top + TitleBarHeight && Input.MouseState.Position.Y > this.Top) {
                if (this.HoverClose) {
                    HideWindow();
                    return;
                }
                Dragging = true;
                DragStart = Input.MouseState.Position;
            }
        }

        private void Window_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            Dragging = false;
        }

        private readonly LinkedList<Panel> currentNav = new LinkedList<Panel>();

        public void Navigate(Panel newPanel, bool keepHistory = true) {
            if (!keepHistory)
                currentNav.Clear();

            currentNav.AddLast(newPanel);

            this.ActivePanel = newPanel;
        }

        public void NavigateBack() {
            if (currentNav.Count > 1)
                currentNav.RemoveLast();

            this.ActivePanel = currentNav.Last.Value;
        }

        public void NavigateHome() {
            this.ActivePanel = currentNav.First.Value;

            currentNav.Clear();

            currentNav.AddFirst(this.ActivePanel);
        }

        public void ToggleWindow() {
            if (this.Visible) HideWindow();
            else ShowWindow();
        }

        public void ShowWindow(bool topMost = false) {
            if (this.Visible) return;

            // TODO: Ensure window can't also go off too far to the right or bottom
            this.Location = new Point(
                Math.Max(0, this.Left),
                Math.Max(0, this.Top)
            );

            this.Opacity = 0;
            this.TopMost = topMost;
            this.Visible = true;

            fade.Resume();
        }
        
        public void HideWindow() {
            if (!this.Visible) return;

            fade.Resume();
            GameServices.GetService<ContentService>().PlaySoundEffectByName(@"audio\window-close");
        }

        public override void Update(GameTime gameTime) {
            if (Dragging) {
                var nOffset = GameServices.GetService<InputService>().MouseState.Position - DragStart;
                this.Location += nOffset;

                DragStart = Input.MouseState.Position;
            }

            this.HoverClose = new Rectangle(this.AbsoluteBounds.Location + ExitBounds.Location, ExitBounds.Size).Contains(Input.MouseState.Position);

            base.Update(gameTime);
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.HoverClose)
                spriteBatch.Draw(Content.GetTexture("button-exit-active"), ExitBounds.OffsetBy(bounds.Location), Color.White);
            else
                spriteBatch.Draw(Content.GetTexture("button-exit"), ExitBounds.OffsetBy(bounds.Location), Color.White);

            Utils.DrawUtil.DrawAlignedText(spriteBatch,
                Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size32, ContentService.FontStyle.Regular), this.Title,
                TitleBarBounds.OffsetBy(80, 0),
                ContentService.Colors.ColonialWhite,
                Utils.DrawUtil.HorizontalAlignment.Left,
                Utils.DrawUtil.VerticalAlignment.Middle
            );
        }

    }
}
