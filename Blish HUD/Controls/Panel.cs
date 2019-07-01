using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used to group collections of controls. Can have an accented border and title, if enabled.
    /// </summary>
    public class Panel : Container, IAccordion {

        // Used when border is enabled
        public const int TOP_MARGIN    = 0;
        public const int RIGHT_MARGIN  = 5;
        public const int BOTTOM_MARGIN = 10;
        public const int LEFT_MARGIN   = 8;

        private const int ARROW_SIZE = 32;

        #region Load Static

        private static readonly Texture2D _texturePanelHeader;
        private static readonly Texture2D _texturePanelHeaderActive;

        private static readonly Texture2D _textureTopLeftAccent;
        private static readonly Texture2D _textureLeftSideAccent;
        private static readonly Texture2D _textureBottomRightAccent;
        private static readonly Texture2D _textureRightSideAccent;

        private static readonly Texture2D _textureAccordionArrow;

        static Panel() {
            _texturePanelHeader       = Content.GetTexture("1032325");
            _texturePanelHeaderActive = Content.GetTexture("1032324");

            _textureTopLeftAccent     = Content.GetTexture("1002144");
            _textureLeftSideAccent    = Content.GetTexture("605025");
            _textureBottomRightAccent = Content.GetTexture("1002142");
            _textureRightSideAccent   = Content.GetTexture("scrollbar-track");

            _textureAccordionArrow = Content.GetTexture("155953");
        }

        #endregion

        protected bool _canScroll = false;
        public bool CanScroll {
            get => _canScroll;
            set {
                if (!SetProperty(ref _canScroll, value)) return;

                UpdateRegions();
                UpdateScrollbar();
            }
        }
        
        protected string _title;
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        protected bool _showBorder;
        public bool ShowBorder {
            get => _showBorder;
            set => SetProperty(ref _showBorder, value, true);
        }

        protected bool _canCollapse;
        public bool CanCollapse {
            get => _canCollapse;
            set => SetProperty(ref _canCollapse, value, true);
        }

        protected bool _collapsed;
        [JsonIgnore]
        public bool Collapsed {
            get => _collapsed;
            set {
                if (value) {
                    Collapse();
                } else {
                    Expand();
                }
            }
        }

        // Must remain internal for Glide to be able to access the property
        [JsonIgnore]
        public float ArrowRotation { get; set; } = 0f;

        /// <inheritdoc />
        public bool ToggleAccordionState() {
            this.Collapsed = !_collapsed;

            return _collapsed;
        }

        private Glide.Tween _collapseAnim;
        private Scrollbar   _panelScrollbar;

        // Used to fast complete the collapse animation, if set to collapsed while the control is not visible
        private bool _hasRendered;

        public delegate void BuildUIDelegate(Panel buildPanel, object obj);

        public void NavigateToBuiltPanel(BuildUIDelegate buildCall, object obj) {
            this.Children.ToList().ForEach(c => c.Dispose());

            var buildPanel = new Panel() {
                Size = _size
            };

            buildCall(buildPanel, obj);

            buildPanel.Parent = this;
        }

        protected override void OnMoved(MovedEventArgs e) {
            base.OnMoved(e);

            // Mostly needed to update the scrollbar location, if it's visible
            UpdateRegions();
        }

        /// <inheritdoc />
        protected override void OnClick(MouseEventArgs e) {
            if (_canCollapse && _layoutHeaderBounds.Contains(this.RelativeMousePosition)) {
                this.ToggleAccordionState();
            }

            base.OnClick(e);
        }

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);
            UpdateRegions();
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);

            e.ChangedChild.Resized += UpdateContentRegionBounds;
            e.ChangedChild.Moved   += UpdateContentRegionBounds;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);

            e.ChangedChild.Resized -= UpdateContentRegionBounds;
            e.ChangedChild.Moved   -= UpdateContentRegionBounds;
        }

        /// <inheritdoc />
        public void Expand() {
            if (!_collapsed) return;

            _collapseAnim?.CancelAndComplete();

            SetProperty(ref _collapsed, false);

            _collapseAnim = Animation.Tweener
                                     .Tween(this,
                                            new { Height = _preCollapseHeight, ArrowRotation = 0f },
                                            0.15f)
                                     .Ease(Glide.Ease.QuadOut);
        }

        private int _preCollapseHeight;

        /// <inheritdoc />
        public void Collapse() {
            if (_collapsed) return;

            // Prevent us from setting the _preCollapseHeight midtransition by accident
            if (_collapseAnim != null && _collapseAnim.Completion < 1) {
                _collapseAnim.CancelAndComplete();
            } else {
                _preCollapseHeight = _size.Y;
            }

            SetProperty(ref _canCollapse, true);
            SetProperty(ref _collapsed,   true);

            _collapseAnim = Animation.Tweener
                                     .Tween(this,
                                            new { Height = _layoutHeaderBounds.Bottom, ArrowRotation = -MathHelper.PiOver2 },
                                            0.15f)
                                     .Ease(Glide.Ease.QuadOut);
        }

        private void UpdateContentRegionBounds(object sender, EventArgs e) {
            UpdateScrollbar();
        }

        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutAccordionArrowBounds;

        /// <inheritdoc />
        public override void RecalculateLayout() {
            if (!_hasRendered) {
                _collapseAnim?.CancelAndComplete();
            }

            UpdateRegions();

            base.RecalculateLayout();
        }

        private void UpdateRegions() {
            int topOffset = !string.IsNullOrEmpty(_title) ? 36 : 0;
            int rightOffset = 0;
            int bottomOffset = 0;
            int leftOffset = 0;

            if (this.ShowBorder) {
                // If we have a title, then we don't need an margin (as the title region will be that offset)
                topOffset = Math.Max(topOffset, TOP_MARGIN);

                rightOffset  += RIGHT_MARGIN;
                bottomOffset += BOTTOM_MARGIN;
                leftOffset   += LEFT_MARGIN;
            }

            if (this.CanScroll)
                rightOffset += (this.ShowBorder ? 0 : 20);

            if (!_collapsed) {
                this.ContentRegion = new Rectangle(leftOffset,
                                                   topOffset,
                                                   _size.X - leftOffset - rightOffset,
                                                   _size.Y - topOffset  - bottomOffset);
            }

            _layoutHeaderBounds = new Rectangle(leftOffset,
                                                0,
                                                ContentRegion.Width,
                                                ContentRegion.Top);

            _layoutAccordionArrowBounds = new Rectangle(_layoutHeaderBounds.Right - ARROW_SIZE,
                                                        (topOffset - ARROW_SIZE) / 2,
                                                        ARROW_SIZE,
                                                        ARROW_SIZE);
        }

        private readonly List<Adhesive.Binding> _scrollbarBindings = new List<Adhesive.Binding>();

        private void UpdateScrollbar() {
            /* TODO: Fix .CanScroll: currently you have to set it after you set other region changing settings for it
               to work correctly */
            if (this.CanScroll) {
                if (_panelScrollbar == null) 
                    _panelScrollbar = new Scrollbar(this);

                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                _scrollbarBindings.ForEach((bind) => bind.Disable());
                _scrollbarBindings.Clear();

                int psHOffset = this.ShowBorder ? -20 : 0;
                int psYOffset = this.ShowBorder ? 10 : 0;
                int psXOffset = this.ShowBorder ? -RIGHT_MARGIN - 2 : -20;

                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Parent, () => this.Parent, applyLeft: true));

                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Height, () => this.Height, (h) => this.ContentRegion.Height - 20, applyLeft: true));

                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Right, () => this.Right, (r) => r - _panelScrollbar.Width / 2, applyLeft: true));

                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Top, () => this.Top, (t) => t + this.ContentRegion.Top + 10, applyLeft: true));

                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Visible, () => this.Visible, applyLeft: true));

                // Ensure scrollbar is visible
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.ZIndex, () => this.ZIndex, (z) => z + 2, applyLeft: true));
            } else {
                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                _scrollbarBindings.ForEach((bind) => bind.Disable());
                _scrollbarBindings.Clear();

                _panelScrollbar?.Dispose();
                _panelScrollbar = null;
            }
        }

        private void DrawAccordionArrow(SpriteBatch spriteBatch, Rectangle bounds) {
            var arrowOrigin = new Vector2((float)ARROW_SIZE / 2, (float)ARROW_SIZE / 2);

            spriteBatch.DrawOnCtrl(this,
                                   _textureAccordionArrow,
                                   bounds.OffsetBy(arrowOrigin.ToPoint()),
                                   null,
                                   Color.White,
                                   this.ArrowRotation,
                                   arrowOrigin);
        }
        
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            _hasRendered = true;

            var headerRect = _layoutHeaderBounds;

            if (!string.IsNullOrEmpty(_title)) {
                // Panel header
                if (_canCollapse && _mouseOver) {
                    spriteBatch.DrawOnCtrl(this,
                                           _texturePanelHeaderActive,
                                           headerRect);
                } else {
                    spriteBatch.DrawOnCtrl(this,
                                           _texturePanelHeader,
                                           headerRect);
                }

                // Panel header text
                spriteBatch.DrawStringOnCtrl(this,
                                         _title,
                                         Content.DefaultFont16,
                                         headerRect.OffsetBy(10, 0),
                                         Color.White);

                if (_canCollapse) {
                    // Collapse arrow
                    DrawAccordionArrow(spriteBatch, _layoutAccordionArrowBounds);
                }
            }

            headerRect.Inflate(-10, 0);

            if (this.ShowBorder) {
                // Lightly tint the background of the panel
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, ContentRegion, Color.Black * 0.1f);

                // Top left accent
                spriteBatch.DrawOnCtrl(this, _textureTopLeftAccent,
                                 new Rectangle(ContentRegion.Left - 6,
                                               headerRect.Bottom - 12,
                                               Math.Min(ContentRegion.Width, 256),
                                               64),
                                 null,
                                 Color.White,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.FlipHorizontally);

                // Bottom right accent
                spriteBatch.DrawOnCtrl(this, _textureBottomRightAccent,
                                 new Rectangle(ContentRegion.Right - 249,
                                               ContentRegion.Bottom - 53,
                                               Math.Min(ContentRegion.Width, 256),
                                               64),
                                 null,
                                 Color.White,
                                 0,
                                 Vector2.Zero);

                // Left side accent
                spriteBatch.DrawOnCtrl(this, _textureLeftSideAccent,
                                 new Rectangle(ContentRegion.Left - 8,
                                               ContentRegion.Top,
                                               16,
                                               ContentRegion.Height),
                                 null,
                                 Color.Black,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.FlipVertically);
            }

            // Right side accent (if scrollbar isn't visible)
            if (this.CanScroll && !_panelScrollbar.Visible) {
                spriteBatch.DrawOnCtrl(this, _textureRightSideAccent,
                                 new Rectangle(ContentRegion.Right - 2,
                                               ContentRegion.Top,
                                               _textureRightSideAccent.Width,
                                               ContentRegion.Height),
                                 Color.Black);
            }
        }

        protected override void DisposeControl() {
            _panelScrollbar?.Dispose();

            base.DisposeControl();
        }

    }
}
