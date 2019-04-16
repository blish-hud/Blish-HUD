using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {

    public class Scrollbar:Control {

        private const int CONTROL_WIDTH = 12;
        private const int MIN_LENGTH = 32;
        private const int CAP_SLACK = 6;

        private Tween TargetScrollDistanceAnim = null;

        private float _targetScrollDistance;
        private float TargetScrollDistance {
            get {
                if (TargetScrollDistanceAnim == null) return _scrollDistance;

                return _targetScrollDistance;
            }
            set {
                float aVal = MathHelper.Clamp(value, 0f, 1f);
                if (this.AssociatedContainer != null && _targetScrollDistance != aVal)
                    _targetScrollDistance = aVal;
            }
        }

        private float _scrollDistance = 0f;
        public float ScrollDistance {
            get => _scrollDistance;
            set {
                float aVal = MathHelper.Clamp(value, 0f, 1f);
                if (_scrollDistance != aVal) {
                    _scrollDistance = aVal;
                    UpdateAssocContainer();

                    OnPropertyChanged();
                }
            }
        }

        private int _scrollbarHeight = MIN_LENGTH;
        private int ScrollbarHeight { get => _scrollbarHeight;
            set {
                if (_scrollbarHeight == value) return;

                _scrollbarHeight = value;
                OnPropertyChanged();

                // Reclamps the scrolling content
                RecalculateScrollbarSize();
                UpdateAssocContainer();
            }
        }

        private double ScrollbarPercent = 1.0;

        private Container _associatedContainer;
        public Container AssociatedContainer {
            get => _associatedContainer;
            set {
                if (_associatedContainer == value) return;

                _associatedContainer = value;
                OnPropertyChanged();
            }
        }

        #region Sprites

        private static bool _spritesLoaded = false;

        private static TextureRegion2D spriteTrack;
        private static TextureRegion2D spriteUpArrow;
        private static TextureRegion2D spriteDownArrow;
        private static TextureRegion2D spriteBar;
        private static TextureRegion2D spriteThumb;
        private static TextureRegion2D spriteTopCap;
        private static TextureRegion2D spriteBottomCap;

        private static void LoadSprites() {
            if (_spritesLoaded) return;

            spriteTrack = ControlAtlas.GetRegion("scrollbar/sb-track");
            spriteUpArrow = ControlAtlas.GetRegion("scrollbar/sb-arrow-up");
            spriteDownArrow = ControlAtlas.GetRegion("scrollbar/sb-arrow-down");
            spriteBar = ControlAtlas.GetRegion("scrollbar/sb-bar-active");
            spriteThumb = ControlAtlas.GetRegion("scrollbar/sb-thumb");
            spriteTopCap = ControlAtlas.GetRegion("scrollbar/sb-cap-top");
            spriteBottomCap = ControlAtlas.GetRegion("scrollbar/sb-cap-bottom");

            _spritesLoaded = true;
        }

        #endregion

        private int TrackLength => (this.Height - spriteUpArrow.Height - spriteDownArrow.Height);

        private bool Scrolling = false;
        private int ScrollingOffset = 0;

        private Rectangle UpArrowBounds;
        private Rectangle DownArrowBounds;
        private Rectangle BarBounds;
        private Rectangle TrackBounds;



        public Scrollbar(Container container) : base() {
            _associatedContainer = container;

            LoadSprites();

            UpArrowBounds = Rectangle.Empty;
            DownArrowBounds = Rectangle.Empty;
            BarBounds = Rectangle.Empty;
            TrackBounds = Rectangle.Empty;

            this.Width = CONTROL_WIDTH;

            Input.LeftMouseButtonReleased += delegate { if (Scrolling) { Scrolling = false; Invalidate(); } };

            this.AssociatedContainer.MouseEntered += delegate { Invalidate(); };
            this.AssociatedContainer.MouseLeft += delegate { Invalidate(); };
            this.AssociatedContainer.ChildAdded += delegate { Invalidate(); };
            this.AssociatedContainer.ChildRemoved += delegate { Invalidate(); };
            this.AssociatedContainer.MouseWheelScrolled += OnWheelScroll;
            this.AssociatedContainer.ContentResized += delegate { Invalidate(); };
            
            this.MouseWheelScrolled     += OnWheelScroll;
        }



        protected void OnWheelScroll(object sender, MouseEventArgs e) {
            // Don't scroll if the scrollbar isn't visible
            if (!this.Visible || ScrollbarPercent > 0.99) return;

            // Avoid scrolling nested panels
            var ctrl = (Control) sender;
            while (ctrl != this.AssociatedContainer && ctrl != null) {
                if (ctrl is Panel) return;
                ctrl = ctrl.Parent;
            }

            float normalScroll = (float)Input.ClickState.EventDetails.wheelDelta / (float)Math.Abs(Input.ClickState.EventDetails.wheelDelta);
            
            TargetScrollDistanceAnim?.Cancel();
            
            this.TargetScrollDistance += normalScroll * -0.08f;

            TargetScrollDistanceAnim = Animation.Tweener
                                                .Tween(this, new {ScrollDistance = this.TargetScrollDistance}, 0.35f)
                                                .Ease(Glide.Ease.QuadOut)
                                                .OnComplete(() => TargetScrollDistanceAnim = null);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel;
        }

        private void UpdateAssocContainer() {
            if (this.AssociatedContainer?.ContentRenderCache != null) {
                this.AssociatedContainer.VerticalScrollOffset = (int)Math.Floor(Math.Max(this.AssociatedContainer.ContentRenderCache.Height - this.AssociatedContainer.ContentRegion.Height, 612) * this.ScrollDistance);
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            base.OnLeftMouseButtonPressed(e);

            var relMousePos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (BarBounds.Contains(relMousePos)) {
                Scrolling = true;
                ScrollingOffset = relMousePos.Y - BarBounds.Y;
                this.TargetScrollDistance = this.ScrollDistance;
            } else if (UpArrowBounds.Contains(relMousePos)) {
                this.ScrollDistance -= 0.02f;
            } else if (DownArrowBounds.Contains(relMousePos)) {
                this.ScrollDistance += 0.02f;
            }
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (Scrolling) {
                var relMousePos = Input.MouseState.Position - this.AbsoluteBounds.Location - new Point(0, ScrollingOffset) - TrackBounds.Location;

                this.ScrollDistance = (float)relMousePos.Y / (float)(this.TrackLength - this.ScrollbarHeight);
                this.TargetScrollDistance = this.ScrollDistance;
            }
        }

        public override void Invalidate() {
            var _lastVal = this.ScrollbarPercent;
            RecalculateScrollbarSize();

            if (_lastVal != this.ScrollbarPercent && this.AssociatedContainer != null) {
                this.ScrollDistance = 0;
                this.TargetScrollDistance = 0;
            }

            UpArrowBounds = new Rectangle(this.Width / 2 - spriteUpArrow.Width / 2, 0, spriteUpArrow.Width, spriteUpArrow.Height);
            DownArrowBounds = new Rectangle(this.Width / 2 - spriteDownArrow.Width / 2, this.Height - spriteDownArrow.Height, spriteDownArrow.Width, spriteDownArrow.Height);
            BarBounds = new Rectangle(this.Width / 2 - spriteBar.Width / 2, (int)(this.ScrollDistance * (this.TrackLength - this.ScrollbarHeight)) + spriteUpArrow.Height, spriteBar.Width, this.ScrollbarHeight);
            TrackBounds = new Rectangle(this.Width / 2 - spriteTrack.Width / 2, UpArrowBounds.Bottom, spriteTrack.Width, this.TrackLength);
            
            base.Invalidate();
        }

        private void RecalculateScrollbarSize() {
            if (this.AssociatedContainer == null) return;
            
            int lowestContent = Math.Max(this.AssociatedContainer.Children.Any() ? this.AssociatedContainer.Children.Where(c => c.Visible).Max(c => c.Bottom) : 0, this.AssociatedContainer.ContentRegion.Height);

            ScrollbarPercent = (double) this.AssociatedContainer.ContentRegion.Height / (double)lowestContent;

            this.ScrollbarHeight = (int)Math.Max(Math.Floor(this.TrackLength * ScrollbarPercent) - 1, MIN_LENGTH);

            UpdateAssocContainer();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Don't show the scrollbar if there is nothing to scroll
            if (ScrollbarPercent > 0.99) return;

            var drawTint = !Scrolling && this.MouseOver || (this.AssociatedContainer != null && this.AssociatedContainer.MouseOver) ? Color.White : ContentService.Colors.Darkened(0.6f);
            drawTint = Scrolling ? ContentService.Colors.Darkened(0.9f) : drawTint;

            spriteBatch.Draw(spriteTrack, TrackBounds, Color.White);
            
            spriteBatch.Draw(spriteUpArrow, UpArrowBounds, drawTint);
            spriteBatch.Draw(spriteDownArrow, DownArrowBounds, drawTint);

            spriteBatch.Draw(spriteBar, BarBounds, drawTint);
            spriteBatch.Draw(spriteTopCap, new Rectangle(this.Width / 2 - spriteTopCap.Width / 2, BarBounds.Top - CAP_SLACK, spriteTopCap.Width, spriteTopCap.Height), Color.White);
            spriteBatch.Draw(spriteBottomCap, new Rectangle(this.Width / 2 - spriteBottomCap.Width / 2, BarBounds.Bottom - spriteBottomCap.Height + CAP_SLACK, spriteBottomCap.Width, spriteBottomCap.Height), Color.White);
            spriteBatch.Draw(spriteThumb, new Rectangle(this.Width / 2 - spriteThumb.Width / 2, BarBounds.Top + (this.ScrollbarHeight / 2 - spriteThumb.Height / 2), spriteThumb.Width, spriteThumb.Height), drawTint);
        }

    }
}
