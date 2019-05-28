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
                if (_associatedContainer != null && _targetScrollDistance != aVal)
                    _targetScrollDistance = aVal;

                Invalidate();
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

                Invalidate();
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

        private int TrackLength => (_size.Y - spriteUpArrow.Height - spriteDownArrow.Height);

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

            Input.LeftMouseButtonReleased += delegate { if (Scrolling) { Scrolling = false; /* Invalidate(); */ } };

            //_associatedContainer.MouseEntered += delegate { Invalidate(); };
            //_associatedContainer.MouseLeft += delegate { Invalidate(); };
            //_associatedContainer.ChildAdded += delegate { Invalidate(); };
            //_associatedContainer.ChildRemoved += delegate { Invalidate(); };
            //_associatedContainer.ContentResized += delegate { Invalidate(); };
            _associatedContainer.MouseWheelScrolled += OnWheelScroll;

            this.MouseWheelScrolled     += OnWheelScroll;
        }

        protected void OnWheelScroll(object sender, MouseEventArgs e) {
            // Don't scroll if the scrollbar isn't visible
            if (!this.Visible || ScrollbarPercent > 0.99) return;

            // Avoid scrolling nested panels
            var ctrl = (Control) sender;
            while (ctrl != _associatedContainer && ctrl != null) {
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

            UpArrowBounds = new Rectangle(this.Width / 2 - spriteUpArrow.Width / 2, 0, spriteUpArrow.Width, spriteUpArrow.Height);
            DownArrowBounds = new Rectangle(this.Width / 2 - spriteDownArrow.Width / 2, this.Height - spriteDownArrow.Height, spriteDownArrow.Width, spriteDownArrow.Height);
            BarBounds = new Rectangle(this.Width / 2 - spriteBar.Width / 2, (int)(this.ScrollDistance * (this.TrackLength - this.ScrollbarHeight)) + spriteUpArrow.Height, spriteBar.Width, this.ScrollbarHeight);
            TrackBounds = new Rectangle(this.Width / 2 - spriteTrack.Width / 2, UpArrowBounds.Bottom, spriteTrack.Width, this.TrackLength);
            
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

            spriteBatch.DrawOnCtrl(this, spriteTrack, TrackBounds);

            spriteBatch.DrawOnCtrl(this, spriteUpArrow, UpArrowBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, spriteDownArrow, DownArrowBounds, drawTint);

            spriteBatch.DrawOnCtrl(this, spriteBar, BarBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, spriteTopCap, new Rectangle(this.Width / 2 - spriteTopCap.Width / 2, BarBounds.Top - CAP_SLACK, spriteTopCap.Width, spriteTopCap.Height));
            spriteBatch.DrawOnCtrl(this, spriteBottomCap, new Rectangle(this.Width / 2 - spriteBottomCap.Width / 2, BarBounds.Bottom - spriteBottomCap.Height + CAP_SLACK, spriteBottomCap.Width, spriteBottomCap.Height));
            spriteBatch.DrawOnCtrl(this, spriteThumb, new Rectangle(this.Width / 2 - spriteThumb.Width / 2, BarBounds.Top + (this.ScrollbarHeight / 2 - spriteThumb.Height / 2), spriteThumb.Width, spriteThumb.Height), drawTint);
        }

    }
}
