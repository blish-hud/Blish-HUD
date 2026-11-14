using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used to group collections of controls. Can have an accented border and title, if enabled.
    /// </summary>
    public class Panel : Container, IAccordion {

        public static readonly DesignStandard MenuStandard = new DesignStandard(/*          Size */ new Point(265, 700),
                                                                                /*   PanelOffset */ new Point(9, 28),
                                                                                /* ControlOffset */ Control.ControlStandard.ControlOffset);

        // Used when border is enabled
        public const int TOP_PADDING    = 7;
        public const int RIGHT_PADDING  = 4;
        public const int BOTTOM_PADDING = 7;
        public const int LEFT_PADDING   = 4;

        public const  int HEADER_HEIGHT    = 36;
        private const int ARROW_SIZE       = 32;
        private const int MAX_ACCENT_WIDTH = 256;

        #region Textures

        private readonly AsyncTexture2D _texturePanelHeader       = AsyncTexture2D.FromAssetId(1032325);
        private readonly AsyncTexture2D _texturePanelHeaderActive = AsyncTexture2D.FromAssetId(1032324);

        private readonly AsyncTexture2D _textureCornerAccent   = AsyncTexture2D.FromAssetId(1002144);
        private readonly AsyncTexture2D _textureLeftSideAccent = AsyncTexture2D.FromAssetId(605025);

        private readonly AsyncTexture2D _textureAccordionArrow = AsyncTexture2D.FromAssetId(155953);

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

        protected AsyncTexture2D _icon;
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
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

        public override SizingMode HeightSizingMode {
            get {
                if (_collapsed
                 || (_collapseAnim != null && _collapseAnim.Completion < 1f)) {
                    return SizingMode.Standard;
                }

                return base.HeightSizingMode;
            }
            set => base.HeightSizingMode = value;
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

        /// <inheritdoc />
        public override Control TriggerMouseInput(MouseEventArgs args, MouseState ms) {
            if (!string.IsNullOrEmpty(_title) && _layoutHeaderBounds.Contains(this.RelativeMousePosition)) {
                args.StopPropagation();
            }

            return base.TriggerMouseInput(args, ms);
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
        private Rectangle _layoutHeaderIconBounds;
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

            if (_icon?.HasTexture == true) {

                _layoutHeaderIconBounds = new Rectangle(_layoutHeaderBounds.Left + 3, 3, HEADER_HEIGHT - 6, HEADER_HEIGHT - 6);
                _layoutHeaderTextBounds = new Rectangle(_layoutHeaderIconBounds.Right + 5, 0, _layoutHeaderBounds.Width - _layoutHeaderIconBounds.Width, HEADER_HEIGHT);

            } else {

                _layoutHeaderIconBounds = Rectangle.Empty;
                _layoutHeaderTextBounds = new Rectangle(_layoutHeaderBounds.Left + 10, 0, _layoutHeaderBounds.Width - 10, HEADER_HEIGHT);

            }

            _layoutAccordionArrowOrigin = new Vector2((float)ARROW_SIZE / 2, (float)ARROW_SIZE / 2);
            _layoutAccordionArrowBounds = new Rectangle(_layoutHeaderBounds.Right - ARROW_SIZE,
                                                        (topOffset - ARROW_SIZE) / 2,
                                                        ARROW_SIZE,
                                                        ARROW_SIZE).OffsetBy(_layoutAccordionArrowOrigin.ToPoint());
        }

        private void UpdateScrollbar() {
            /* TODO: Fix .CanScroll: currently you have to set it after you set other region changing settings for it
               to work correctly */
            if (this.CanScroll) {
                if (_panelScrollbar == null) 
                    _panelScrollbar = new Scrollbar(this);

                this.PropertyChanged -= UpdatePanelScrollbarOnOwnPropertyChanged;
                this.PropertyChanged += UpdatePanelScrollbarOnOwnPropertyChanged;

                _panelScrollbar.Parent  = this.Parent;
                _panelScrollbar.Height  = this.ContentRegion.Height  - 20;
                _panelScrollbar.Right   = this.Right                          - _panelScrollbar.Width / 2;
                _panelScrollbar.Top     = this.Top + this.ContentRegion.Top + 10;
                _panelScrollbar.Visible = this.Visible;
                _panelScrollbar.ZIndex  = this.ZIndex + 2;
            } else {
                this.PropertyChanged -= UpdatePanelScrollbarOnOwnPropertyChanged;
                _panelScrollbar?.Dispose();
                _panelScrollbar = null;
            }
        }

        // TODO Temporary solution to avoid memory leak due to Adhesive bindings before
        // This will be replaced when the Scrollbar is converted to a stateless overlay
        private void UpdatePanelScrollbarOnOwnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Parent":
                    _panelScrollbar.Parent = this.Parent;
                    break;
                case "Height":
                    _panelScrollbar.Height = this.ContentRegion.Height - 20;
                    break;
                case "Right":
                    _panelScrollbar.Right = this.Right - _panelScrollbar.Width / 2;
                    break;
                case "Top":
                    _panelScrollbar.Top = this.Top + this.ContentRegion.Top + 10;
                    break;
                case "Visible":
                    _panelScrollbar.Visible = this.Visible;
                    break;
                case "ZIndex":
                    _panelScrollbar.ZIndex = this.ZIndex + 2;
                    break;
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

                // Panel header icon
                if (_icon?.HasTexture == true) {
                    spriteBatch.DrawOnCtrl(this, _icon, _layoutHeaderIconBounds, Color.White);
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
            
            foreach (var control in this._children) {
                control.Resized -= UpdateContentRegionBounds;
                control.Moved   -= UpdateContentRegionBounds;
            }

            base.DisposeControl();
        }

    }
}
