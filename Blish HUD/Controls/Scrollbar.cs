using System;
using System.Linq;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {

    public class Scrollbar:Control {

        private const int CONTROL_WIDTH = 12;
        private const int MIN_LENGTH = 32;
        private const int CAP_SLACK = 6;
        
        #region Load Static

        private static readonly TextureRegion2D _textureTrack;
        private static readonly TextureRegion2D _textureUpArrow;
        private static readonly TextureRegion2D _textureDownArrow;
        private static readonly TextureRegion2D _textureBar;
        private static readonly TextureRegion2D _textureThumb;
        private static readonly TextureRegion2D _textureTopCap;
        private static readonly TextureRegion2D _textureBottomCap;

        static Scrollbar() {
            _textureTrack     = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-track");
            _textureUpArrow   = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-arrow-up");
            _textureDownArrow = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-arrow-down");
            _textureBar       = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-bar-active");
            _textureThumb     = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-thumb");
            _textureTopCap    = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-cap-top");
            _textureBottomCap = Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-cap-bottom");
        }

        #endregion

        private Tween _targetScrollDistanceAnim = null;

        private float _targetScrollDistance;
        private float TargetScrollDistance {
            get {
                if (_targetScrollDistanceAnim == null) return _scrollDistance;

                return _targetScrollDistance;
            }
            set {
                float aVal = MathHelper.Clamp(value, 0f, 1f);
                if (_associatedContainer != null && _targetScrollDistance != aVal)
                    _targetScrollDistance = aVal;

                Invalidate();
            }
        }

        private float _scrollDistance = 0f;
        public float ScrollDistance {
            get => _scrollDistance;
            set {
                if (!SetProperty(ref _scrollDistance, MathHelper.Clamp(value, 0f, 1f), true)) return;

                UpdateAssocContainer();
            }
        }

        private int _scrollbarHeight = MIN_LENGTH;
        private int ScrollbarHeight {
            get => _scrollbarHeight;
            set {
                if (!SetProperty(ref _scrollbarHeight, value, true)) return;

                // Reclamps the scrolling content
                RecalculateScrollbarSize();
                UpdateAssocContainer();
            }
        }

        private double ScrollbarPercent = 1.0;

        private Container _associatedContainer;
        public Container AssociatedContainer {
            get => _associatedContainer;
            set => SetProperty(ref _associatedContainer, value);
        }

        private int TrackLength => (_size.Y - _textureUpArrow.Height - _textureDownArrow.Height);

        private bool Scrolling = false;
        private int ScrollingOffset = 0;

        private Rectangle UpArrowBounds;
        private Rectangle DownArrowBounds;
        private Rectangle BarBounds;
        private Rectangle TrackBounds;

        public Scrollbar(Container container) : base() {
            _associatedContainer = container;

            UpArrowBounds = Rectangle.Empty;
            DownArrowBounds = Rectangle.Empty;
            BarBounds = Rectangle.Empty;
            TrackBounds = Rectangle.Empty;

            this.Width = CONTROL_WIDTH;

            Input.LeftMouseButtonReleased += delegate { if (Scrolling) { Scrolling = false; /* Invalidate(); */ } };

            _associatedContainer.MouseWheelScrolled += HandleWheelScroll;
        }

        protected override void OnMouseWheelScrolled(MouseEventArgs e) {
            HandleWheelScroll(this, e);

            base.OnMouseWheelScrolled(e);
        }

        private void HandleWheelScroll(object sender, MouseEventArgs e) {
            // Don't scroll if the scrollbar isn't visible
            if (!this.Visible || ScrollbarPercent > 0.99) return;

            // Avoid scrolling nested panels
            var ctrl = (Control) sender;
            while (ctrl != _associatedContainer && ctrl != null) {
                if (ctrl is Panel) return;
                ctrl = ctrl.Parent;
            }

            float normalScroll = (float)Input.ClickState.EventDetails.wheelDelta / (float)Math.Abs(Input.ClickState.EventDetails.wheelDelta);
            
            _targetScrollDistanceAnim?.Cancel();
            
            this.TargetScrollDistance += normalScroll * -0.08f;

            _targetScrollDistanceAnim = Animation.Tweener
                                                .Tween(this, new {ScrollDistance = this.TargetScrollDistance}, 0.35f)
                                                .Ease(Glide.Ease.QuadOut)
                                                .OnComplete(() => _targetScrollDistanceAnim = null);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel;
        }
        
        private void UpdateAssocContainer() {
            // TODO: What is the 612 in the scrollbar update?
            AssociatedContainer.VerticalScrollOffset = (int)Math.Floor(Math.Max(_containerLowestContent - AssociatedContainer.ContentRegion.Height, 612) * this.ScrollDistance);
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

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            if (Scrolling) {
                var relMousePos = Input.MouseState.Position - this.AbsoluteBounds.Location - new Point(0, ScrollingOffset) - TrackBounds.Location;
                
                this.ScrollDistance = (float)relMousePos.Y / (float)(this.TrackLength - this.ScrollbarHeight);
                this.TargetScrollDistance = this.ScrollDistance;
            }

            Invalidate();
        }

        public override void Invalidate() {
            var _lastVal = this.ScrollbarPercent;
            RecalculateScrollbarSize();

            if (_lastVal != this.ScrollbarPercent && _associatedContainer != null) {
                this.ScrollDistance = 0;
                this.TargetScrollDistance = 0;
            }

            UpArrowBounds = new Rectangle(this.Width / 2 - _textureUpArrow.Width / 2, 0, _textureUpArrow.Width, _textureUpArrow.Height);
            DownArrowBounds = new Rectangle(this.Width / 2 - _textureDownArrow.Width / 2, this.Height - _textureDownArrow.Height, _textureDownArrow.Width, _textureDownArrow.Height);
            BarBounds = new Rectangle(this.Width / 2 - _textureBar.Width / 2, (int)(this.ScrollDistance * (this.TrackLength - this.ScrollbarHeight)) + _textureUpArrow.Height, _textureBar.Width, this.ScrollbarHeight);
            TrackBounds = new Rectangle(this.Width / 2 - _textureTrack.Width / 2, UpArrowBounds.Bottom, _textureTrack.Width, this.TrackLength);
            
            base.Invalidate();
        }

        private int _containerLowestContent;

        private void RecalculateScrollbarSize() {
            if (_associatedContainer == null) return;

            _containerLowestContent = Math.Max(_associatedContainer.Children.Any()
                                                   ? _associatedContainer.Children.Where(c => c.Visible).Max(c => c.Bottom)
                                                   : 0,
                                               _associatedContainer.ContentRegion.Height);

            ScrollbarPercent = (double)_associatedContainer.ContentRegion.Height / (double)_containerLowestContent;

            this.ScrollbarHeight = (int)Math.Max(Math.Floor(this.TrackLength * ScrollbarPercent) - 1, MIN_LENGTH);

            UpdateAssocContainer();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Don't show the scrollbar if there is nothing to scroll
            if (ScrollbarPercent > 0.99) return;

            var drawTint = !Scrolling && this.MouseOver || (_associatedContainer != null && _associatedContainer.MouseOver)
                               ? Color.White
                               : ContentService.Colors.Darkened(0.6f);

            drawTint = Scrolling
                           ? ContentService.Colors.Darkened(0.9f)
                           : drawTint;

            spriteBatch.DrawOnCtrl(this, _textureTrack, TrackBounds);

            spriteBatch.DrawOnCtrl(this, _textureUpArrow, UpArrowBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, _textureDownArrow, DownArrowBounds, drawTint);

            spriteBatch.DrawOnCtrl(this, _textureBar, BarBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, _textureTopCap, new Rectangle(this.Width / 2 - _textureTopCap.Width / 2, BarBounds.Top - CAP_SLACK, _textureTopCap.Width, _textureTopCap.Height));
            spriteBatch.DrawOnCtrl(this, _textureBottomCap, new Rectangle(this.Width / 2 - _textureBottomCap.Width / 2, BarBounds.Bottom - _textureBottomCap.Height + CAP_SLACK, _textureBottomCap.Width, _textureBottomCap.Height));
            spriteBatch.DrawOnCtrl(this, _textureThumb, new Rectangle(this.Width / 2 - _textureThumb.Width / 2, BarBounds.Top + (this.ScrollbarHeight / 2 - _textureThumb.Height / 2), _textureThumb.Width, _textureThumb.Height), drawTint);
        }

    }
}
