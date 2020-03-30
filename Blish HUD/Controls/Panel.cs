using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used to group collections of controls. Can have an accented border and title, if enabled.
    /// </summary>
    public class Panel : Container, IAccordion {

        public static readonly DesignStandard MenuStandard = new DesignStandard(/*          Size */ new Point(265, 700),
                                                                                /*   PanelOffset */ new Point(9, 28),
                                                                                /* ControlOffset */ Control.ControlStandard.ControlOffset);
        
        // Used when border is enabled
        private const int TOP_PADDING    = 7;
        private const int RIGHT_PADDING  = 4;
        private const int BOTTOM_PADDING = 7;
        private const int LEFT_PADDING   = 4;

        private const int HEADER_HEIGHT    = 36;
        private const int ARROW_SIZE       = 32;
        private const int MAX_ACCENT_WIDTH = 256;

        #region Load Static

        private static readonly Texture2D _texturePanelHeader;
        private static readonly Texture2D _texturePanelHeaderActive;

        private static readonly Texture2D _textureCornerAccent;
        private static readonly Texture2D _textureLeftSideAccent;

        private static readonly Texture2D _textureAccordionArrow;

        static Panel() {
            _texturePanelHeader       = Content.GetTexture(@"controls\panel\1032325");
            _texturePanelHeaderActive = Content.GetTexture(@"controls\panel\1032324");

            _textureCornerAccent     = Content.GetTexture(@"controls\panel\1002144");
            _textureLeftSideAccent    = Content.GetTexture("605025");

            _textureAccordionArrow = Content.GetTexture(@"controls\panel\155953");
        }

        #endregion

        public delegate void BuildUIDelegate(Panel buildPanel, object obj);

        protected bool _canScroll = false;
        public bool CanScroll {
            get => _canScroll;
            set {
                if (!SetProperty(ref _canScroll, value, true)) return;

                UpdateScrollbar();
            }
        }
        
        protected string _title;
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        protected AsyncTexture2D _backgroundTexture;

        /// <summary>
        /// A texture to be drawn on the <see cref="Panel"/> before children are drawn.
        /// </summary>
        public AsyncTexture2D BackgroundTexture {
            get => _backgroundTexture;
            set => SetProperty(ref _backgroundTexture, value);
        }

        protected bool _showBorder;
        public bool ShowBorder {
            get => _showBorder;
            set => SetProperty(ref _showBorder, value, true);
        }

        protected bool _showTint;
        public bool ShowTint {
            get => _showTint;
            set => SetProperty(ref _showTint, value);
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

        // Must remain public for Glide to be able to access the property
        [JsonIgnore] public float ArrowRotation { get; set; } = 0f;
        [JsonIgnore] public float AccentOpacity { get; set; } = 1f;

        private Glide.Tween _collapseAnim;
        private Scrollbar   _panelScrollbar;

        /// <inheritdoc />
        public bool ToggleAccordionState() {
            this.Collapsed = !_collapsed;

            return _collapsed;
        }

        public void NavigateToBuiltPanel(BuildUIDelegate buildCall, object obj) {
            this.Children.ToList().ForEach(c => c.Dispose());

            var buildPanel = new Panel() {
                Size = _size
            };

            buildCall(buildPanel, obj);

            buildPanel.Parent = this;
        }

        /// <inheritdoc />
        protected override void OnClick(MouseEventArgs e) {
            if (_canCollapse && _layoutHeaderBounds.Contains(this.RelativeMousePosition)) {
                this.ToggleAccordionState();
            }

            base.OnClick(e);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);

            e.ChangedChild.Resized += UpdateContentRegionBounds;
            e.ChangedChild.Moved += UpdateContentRegionBounds;
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
                                            new { Height = _preCollapseHeight, ArrowRotation = 0f, AccentOpacity = 1f },
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
                                            new { Height = _layoutHeaderBounds.Bottom, ArrowRotation = -MathHelper.PiOver2, AccentOpacity = 0f },
                                            0.15f)
                                     .Ease(Glide.Ease.QuadOut);
        }

        private void UpdateContentRegionBounds(object sender, EventArgs e) {
            UpdateScrollbar();
        }

        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutHeaderTextBounds;

        private Vector2   _layoutAccordionArrowOrigin;
        private Rectangle _layoutAccordionArrowBounds;

        private Rectangle _layoutTopLeftAccentBounds;
        private Rectangle _layoutBottomRightAccentBounds;
        private Rectangle _layoutCornerAccentSrc;

        private Rectangle _layoutLeftAccentBounds;
        private Rectangle _layoutLeftAccentSrc;

        /// <inheritdoc />
        public override void RecalculateLayout() {
            bool showsHeader = !string.IsNullOrEmpty(_title);

            int topOffset    = showsHeader ? HEADER_HEIGHT : 0;
            int rightOffset  = 0;
            int bottomOffset = 0;
            int leftOffset   = 0;

            if (this.ShowBorder) {
                topOffset    = Math.Max(TOP_PADDING, topOffset);
                rightOffset  = RIGHT_PADDING;
                bottomOffset = BOTTOM_PADDING;
                leftOffset   = LEFT_PADDING;

                // Corner accents
                int cornerAccentWidth = Math.Min(_size.X, MAX_ACCENT_WIDTH);
                _layoutTopLeftAccentBounds = new Rectangle(-2, topOffset - 12, cornerAccentWidth, _textureCornerAccent.Height);

                _layoutBottomRightAccentBounds = new Rectangle(_size.X - cornerAccentWidth + 2, _size.Y - 59, cornerAccentWidth, _textureCornerAccent.Height);

                _layoutCornerAccentSrc = new Rectangle(MAX_ACCENT_WIDTH - cornerAccentWidth, 0, cornerAccentWidth, _textureCornerAccent.Height);

                // Left side accent
                _layoutLeftAccentBounds = new Rectangle(leftOffset - 7, topOffset, _textureLeftSideAccent.Width, Math.Min(_size.Y - topOffset - bottomOffset, _textureLeftSideAccent.Height));
                _layoutLeftAccentSrc    = new Rectangle(0,  0,         _textureLeftSideAccent.Width, _layoutLeftAccentBounds.Height);
            }

            this.ContentRegion = new Rectangle(leftOffset,
                                               topOffset,
                                               _size.X - leftOffset - rightOffset,
                                               _size.Y - topOffset - bottomOffset);

            _layoutHeaderBounds     = new Rectangle(this.ContentRegion.Left,       0, this.ContentRegion.Width,       HEADER_HEIGHT);
            _layoutHeaderTextBounds = new Rectangle(_layoutHeaderBounds.Left + 10, 0, _layoutHeaderBounds.Width - 10, HEADER_HEIGHT);

            _layoutAccordionArrowOrigin = new Vector2((float)ARROW_SIZE / 2, (float)ARROW_SIZE / 2);
            _layoutAccordionArrowBounds = new Rectangle(_layoutHeaderBounds.Right - ARROW_SIZE,
                                                        (topOffset - ARROW_SIZE) / 2,
                                                        ARROW_SIZE,
                                                        ARROW_SIZE).OffsetBy(_layoutAccordionArrowOrigin.ToPoint());
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
        
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_backgroundTexture != null) {
                spriteBatch.DrawOnCtrl(this, _backgroundTexture, bounds);
            }

            if (_showTint) {
                spriteBatch.DrawOnCtrl(this,
                                       ContentService.Textures.Pixel,
                                       this.ContentRegion,
                                       Color.Black * 0.4f);
            }

            if (!string.IsNullOrEmpty(_title)) {
                spriteBatch.DrawOnCtrl(this,
                                       _texturePanelHeader,
                                       _layoutHeaderBounds);

                // Panel header
                if (_canCollapse && _mouseOver && this.RelativeMousePosition.Y <= HEADER_HEIGHT) {
                    spriteBatch.DrawOnCtrl(this,
                                           _texturePanelHeaderActive,
                                           _layoutHeaderBounds);
                } else {
                    spriteBatch.DrawOnCtrl(this,
                                           _texturePanelHeader,
                                           _layoutHeaderBounds);
                }

                // Panel header text
                spriteBatch.DrawStringOnCtrl(this,
                                             _title,
                                             Content.DefaultFont16,
                                             _layoutHeaderTextBounds,
                                             Color.White);

                if (_canCollapse) {
                    // Collapse arrow
                    spriteBatch.DrawOnCtrl(this,
                                           _textureAccordionArrow,
                                           _layoutAccordionArrowBounds,
                                           null,
                                           Color.White,
                                           this.ArrowRotation,
                                           _layoutAccordionArrowOrigin);
                }
            }

            if (this.ShowBorder) {
                // Lightly tint the background of the panel
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this.ContentRegion, Color.Black * (0.1f * AccentOpacity));

                // Top left accent
                spriteBatch.DrawOnCtrl(this,
                                       _textureCornerAccent,
                                       _layoutTopLeftAccentBounds,
                                       _layoutCornerAccentSrc,
                                       Color.White * AccentOpacity,
                                       0,
                                       Vector2.Zero,
                                       SpriteEffects.FlipHorizontally);

                // Bottom right accent
                spriteBatch.DrawOnCtrl(this,
                                       _textureCornerAccent,
                                       _layoutBottomRightAccentBounds,
                                       _layoutCornerAccentSrc,
                                       Color.White * AccentOpacity,
                                       0,
                                       Vector2.Zero,
                                       SpriteEffects.FlipVertically);

                // Left side accent
                spriteBatch.DrawOnCtrl(this,
                                       _textureLeftSideAccent,
                                       _layoutLeftAccentBounds,
                                       _layoutLeftAccentSrc,
                                       Color.Black * AccentOpacity,
                                       0,
                                       Vector2.Zero,
                                       SpriteEffects.FlipVertically);
            }
        }

        protected override void DisposeControl() {
            _panelScrollbar?.Dispose();

            base.DisposeControl();
        }

    }
}
