using System;
using System.Linq;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {

    public class Scrollbar:Control {

        private const int CONTROL_WIDTH = 12;
        private const int MIN_LENGTH    = 32;
        private const int CAP_SLACK     = 6;

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

        private double _scrollbarPercent = 1.0;

        private Container _associatedContainer;
        public Container AssociatedContainer {
            get => _associatedContainer;
            set => SetProperty(ref _associatedContainer, value);
        }

        private int TrackLength => (_size.Y - _textureUpArrow.Height - _textureDownArrow.Height);

        private bool _scrolling       = false;
        private int  _scrollingOffset = 0;

        private Rectangle _upArrowBounds;
        private Rectangle _downArrowBounds;
        private Rectangle _barBounds;
        private Rectangle _trackBounds;

        public Scrollbar(Container container) {
            _associatedContainer = container;

            _upArrowBounds   = Rectangle.Empty;
            _downArrowBounds = Rectangle.Empty;
            _barBounds       = Rectangle.Empty;
            _trackBounds     = Rectangle.Empty;

            this.Width = CONTROL_WIDTH;

            Input.Mouse.LeftMouseButtonReleased += MouseOnLeftMouseButtonReleased;

            _associatedContainer.MouseWheelScrolled += HandleWheelScroll;
        }

        private void MouseOnLeftMouseButtonReleased(object sender, MouseEventArgs e) {
            _scrolling = false;
        }

        protected override void OnMouseWheelScrolled(MouseEventArgs e) {
            HandleWheelScroll(this, e);

            base.OnMouseWheelScrolled(e);
        }

        private void HandleWheelScroll(object sender, MouseEventArgs e) {
            // Don't scroll if the scrollbar isn't visible
            if (!this.Visible || _scrollbarPercent > 0.99) return;

            // Avoid scrolling nested panels
            var ctrl = (Control) sender;
            while (ctrl != _associatedContainer && ctrl != null) {
                if (ctrl is Panel) return;
                ctrl = ctrl.Parent;
            }

            if (e.MouseState.ScrollWheelValue == 0) return;

            float normalScroll = GameService.Input.Mouse.State.ScrollWheelValue / (float)Math.Abs(GameService.Input.Mouse.State.ScrollWheelValue);
            
            _targetScrollDistanceAnim?.Cancel();
            
            this.TargetScrollDistance += normalScroll * -0.08f * System.Windows.Forms.SystemInformation.MouseWheelScrollLines;

            _targetScrollDistanceAnim = Animation.Tweener
                                                 .Tween(this, new {ScrollDistance = this.TargetScrollDistance}, 0.35f)
                                                 .Ease(Ease.QuadOut);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel;
        }
        
        private void UpdateAssocContainer() {
            AssociatedContainer.VerticalScrollOffset = (int)Math.Floor((_containerLowestContent - AssociatedContainer.ContentRegion.Height) * this.ScrollDistance);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            base.OnLeftMouseButtonPressed(e);

            var relMousePos = Input.Mouse.Position - this.AbsoluteBounds.Location;

            if (_barBounds.Contains(relMousePos)) {
                _scrolling                = true;
                _scrollingOffset          = relMousePos.Y - _barBounds.Y;
                this.TargetScrollDistance = this.ScrollDistance;
            } else if (_upArrowBounds.Contains(relMousePos)) {
                this.ScrollDistance -= 0.02f;
            } else if (_downArrowBounds.Contains(relMousePos)) {
                this.ScrollDistance += 0.02f;
            }
        }

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            if (_scrolling) {
                var relMousePos = Input.Mouse.Position - this.AbsoluteBounds.Location - new Point(0, _scrollingOffset) - _trackBounds.Location;
                
                this.ScrollDistance = relMousePos.Y / (float)(this.TrackLength - this.ScrollbarHeight);
                this.TargetScrollDistance = this.ScrollDistance;
            }

            Invalidate();
        }

        public override void Invalidate() {
            var lastVal = _scrollbarPercent;
            RecalculateScrollbarSize();

            if (lastVal != _scrollbarPercent && _associatedContainer != null) {
                this.ScrollDistance = 0;
                this.TargetScrollDistance = 0;
            }

            _upArrowBounds   = new Rectangle(this.Width / 2 - _textureUpArrow.Width   / 2, 0,                                                                                                 _textureUpArrow.Width,   _textureUpArrow.Height);
            _downArrowBounds = new Rectangle(this.Width / 2 - _textureDownArrow.Width / 2, this.Height                                                            - _textureDownArrow.Height, _textureDownArrow.Width, _textureDownArrow.Height);
            _barBounds       = new Rectangle(this.Width / 2 - _textureBar.Width       / 2, (int)(this.ScrollDistance * (this.TrackLength - this.ScrollbarHeight)) + _textureUpArrow.Height,   _textureBar.Width,       this.ScrollbarHeight);
            _trackBounds     = new Rectangle(this.Width / 2 - _textureTrack.Width     / 2, _upArrowBounds.Bottom,                                                                             _textureTrack.Width,     this.TrackLength);

            base.Invalidate();
        }

        private int _containerLowestContent;

        private void RecalculateScrollbarSize() {
            if (_associatedContainer == null) return;

            var tempContainerChidlren = _associatedContainer.Children.ToArray();

            _containerLowestContent = 0;

            for (int i = 0; i < tempContainerChidlren.Length; i++) {
                ref var child = ref tempContainerChidlren[i];

                if (child.Visible) {
                    _containerLowestContent = Math.Max(_containerLowestContent, child.Bottom);
                }
            }

            _containerLowestContent = Math.Max(_containerLowestContent, _associatedContainer.ContentRegion.Height);

            _scrollbarPercent = _associatedContainer.ContentRegion.Height / (double)_containerLowestContent;

            this.ScrollbarHeight = (int)Math.Max(Math.Floor(this.TrackLength * _scrollbarPercent) - 1, MIN_LENGTH);

            UpdateAssocContainer();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Don't show the scrollbar if there is nothing to scroll
            if (_scrollbarPercent > 0.99) return;

            var drawTint = !_scrolling && this.MouseOver || (_associatedContainer != null && _associatedContainer.MouseOver)
                               ? Color.White
                               : ContentService.Colors.Darkened(0.6f);

            drawTint = _scrolling
                           ? ContentService.Colors.Darkened(0.9f)
                           : drawTint;

            spriteBatch.DrawOnCtrl(this, _textureTrack, _trackBounds);

            spriteBatch.DrawOnCtrl(this, _textureUpArrow,   _upArrowBounds,   drawTint);
            spriteBatch.DrawOnCtrl(this, _textureDownArrow, _downArrowBounds, drawTint);

            spriteBatch.DrawOnCtrl(this, _textureBar,       _barBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, _textureTopCap,    new Rectangle(this.Width / 2 - _textureTopCap.Width    / 2, _barBounds.Top                               - CAP_SLACK,                                             _textureTopCap.Width,    _textureTopCap.Height));
            spriteBatch.DrawOnCtrl(this, _textureBottomCap, new Rectangle(this.Width / 2 - _textureBottomCap.Width / 2, _barBounds.Bottom - _textureBottomCap.Height + CAP_SLACK,                                             _textureBottomCap.Width, _textureBottomCap.Height));
            spriteBatch.DrawOnCtrl(this, _textureThumb,     new Rectangle(this.Width / 2 - _textureThumb.Width     / 2, _barBounds.Top                               + (this.ScrollbarHeight / 2 - _textureThumb.Height / 2), _textureThumb.Width,     _textureThumb.Height), drawTint);
        }

    }
}
