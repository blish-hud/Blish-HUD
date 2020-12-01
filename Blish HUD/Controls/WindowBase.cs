using System;
using System.Collections.Generic;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public abstract class WindowBase : Container {

        private const int COMMON_MARGIN = 16;
        private const int TITLE_OFFSET = 80;
        private const int SUBTITLE_OFFSET = 20;

        #region Load Static

        private static readonly Texture2D _textureTitleBarLeft;
        private static readonly Texture2D _textureTitleBarRight;
        private static readonly Texture2D _textureTitleBarLeftActive;
        private static readonly Texture2D _textureTitleBarRightActive;

        private static readonly Texture2D _textureExitButton;
        private static readonly Texture2D _textureExitButtonActive;

        private static readonly Texture2D _textureWindowCorner;
        private static readonly Texture2D _textureWindowResizableCorner;
        private static readonly Texture2D _textureWindowResizableCornerActive;

        static WindowBase() {
            _textureTitleBarLeft = Content.GetTexture("titlebar-inactive");
            _textureTitleBarRight = Content.GetTexture("window-topright");
            _textureTitleBarLeftActive = Content.GetTexture("titlebar-active");
            _textureTitleBarRightActive = Content.GetTexture("window-topright-active");

            _textureExitButton = Content.GetTexture("button-exit");
            _textureExitButtonActive = Content.GetTexture("button-exit-active");

            _textureWindowCorner = Content.GetTexture(@"controls/window/156008");
            _textureWindowResizableCorner = Content.GetTexture(@"controls/window/156009");
            _textureWindowResizableCornerActive = Content.GetTexture(@"controls/window/156010");
        }

        #endregion

        protected string _title = "No Title";
        /// <summary>
        /// The text shown at the top of the window.
        /// </summary>
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        protected string _subtitle = "";
        /// <summary>
        /// The text shown to the right of the title in the title bar.
        /// This text is smaller and is normally used to show the current tab name and/or hotkey used to open the window.
        /// </summary>
        public string Subtitle {
            get => _subtitle;
            set => SetProperty(ref _subtitle, value);
        }

        protected bool _canResize = false;
        /// <summary>
        /// Allows the window to be resized by dragging the bottom right corner.
        /// </summary>
        /// <remarks>This property has not been implemented.</remarks>
        public bool CanResize {
            get => _canResize;
            set => SetProperty(ref _canResize, value);
        }

        protected Texture2D _emblem = null;
        /// <summary>
        /// The emblem/badge displayed in the top left corner of the window.
        /// </summary>
        public Texture2D Emblem {
            get => _emblem;
            set => SetProperty(ref _emblem, value);
        }

        protected bool _topMost;
        /// <summary>
        /// If this window will show on top of all other windows, regardless of which one had focus last.
        /// </summary>
        /// <remarks>This property has not been implemented.</remarks>
        public bool TopMost {
            get => _topMost;
            set => SetProperty(ref _topMost, value);
        }

        protected bool StandardWindow = false;

        private Panel _activePanel;
        public Panel ActivePanel {
            get => _activePanel;
            set {
                if (_activePanel != null) {
                    _activePanel.Hide();
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

        private readonly Glide.Tween _animFade;

        protected bool  Dragging  = false;
        protected Point DragStart = Point.Zero;

        protected bool _hoverClose = false;
        protected bool HoverClose {
            get => _hoverClose;
            private set => SetProperty(ref _hoverClose, value);
        }

        #region Window Construction

        protected Texture2D _windowBackground;
        protected Vector2   _windowBackgroundOrigin;
        protected Rectangle _windowBackgroundBounds;
        protected Rectangle _titleBarBounds;

        #endregion

        #region Calculated Layout

        private Rectangle _layoutLeftTitleBarBounds;
        private Rectangle _layoutRightTitleBarBounds;

        private Rectangle _layoutSubtitleBounds;

        private Rectangle _layoutExitButtonBounds;

        private Rectangle _layoutWindowCornerBounds;

        #endregion

        #region Region States

        protected bool MouseOverTitleBar     = false;
        protected bool MouseOverExitButton   = false;
        protected bool MouseOverCornerResize = false;

        #endregion

        protected WindowBase() {
            this.Opacity = 0f;
            this.Visible = false;

            this.ZIndex = Screen.WINDOW_BASEZINDEX;

            Input.Mouse.LeftMouseButtonReleased += delegate { Dragging = false; };

            _animFade = Animation.Tweener.Tween(this, new { Opacity = 1f }, 0.2f).Repeat().Reflect();
            _animFade.Pause();

            _animFade.OnComplete(() => {
                _animFade.Pause();
                if (_opacity <= 0) this.Visible = false;
            });
        }

        protected virtual void ConstructWindow(Texture2D background, Vector2 backgroundOrigin, Rectangle? windowBackgroundBounds = null, Thickness outerPadding = default, int titleBarHeight = 0, bool standardWindow = true) {
            StandardWindow = standardWindow;

            _windowBackground       = background;
            _windowBackgroundOrigin = backgroundOrigin;

            Rectangle tempBounds = windowBackgroundBounds ?? background.Bounds;

            _titleBarBounds = new Rectangle(0, 0, tempBounds.Width, titleBarHeight);

            this.Size    = tempBounds.Size;
            this.Padding = outerPadding;

            _windowBackgroundBounds = new Rectangle(0, titleBarHeight, tempBounds.Width + (int)_padding.Right + (int)_padding.Left, tempBounds.Height + (int)_padding.Bottom);
        }

        public override void RecalculateLayout() {
            // Title bar bounds
            int titleBarDrawOffset = _titleBarBounds.Y - (_textureTitleBarLeft.Height / 2 - _titleBarBounds.Height / 2);
            int titleBarRightWidth = _textureTitleBarRight.Width - COMMON_MARGIN;

            _layoutLeftTitleBarBounds  = new Rectangle(_titleBarBounds.X,                          titleBarDrawOffset, Math.Min(_titleBarBounds.Width - titleBarRightWidth, _windowBackgroundBounds.Width - titleBarRightWidth), _textureTitleBarLeft.Height);
            _layoutRightTitleBarBounds = new Rectangle(_titleBarBounds.Right - titleBarRightWidth, titleBarDrawOffset, _textureTitleBarRight.Width,                                                                              _textureTitleBarRight.Height);

            // Title bar text bounds
            if (!string.IsNullOrEmpty(_title) && !string.IsNullOrEmpty(_subtitle)) {
                int titleTextWidth = (int)Content.DefaultFont32.MeasureString(_title).Width;

                _layoutSubtitleBounds = _layoutLeftTitleBarBounds.OffsetBy(TITLE_OFFSET + titleTextWidth + SUBTITLE_OFFSET, 0);
            }


            // Title bar exit button bounds
            _layoutExitButtonBounds = new Rectangle(_layoutRightTitleBarBounds.Right - (COMMON_MARGIN * 2) - _textureExitButton.Width,
                                                    _layoutRightTitleBarBounds.Y + COMMON_MARGIN,
                                                    _textureExitButton.Width,
                                                    _textureExitButton.Height);

            // Corner edge bounds
            _layoutWindowCornerBounds = new Rectangle(_layoutRightTitleBarBounds.Right - _textureWindowCorner.Width - COMMON_MARGIN,
                                                      this.ContentRegion.Bottom - _textureWindowCorner.Height + COMMON_MARGIN,
                                                      _textureWindowCorner.Width,
                                                      _textureWindowCorner.Height);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            MouseOverTitleBar     = false;
            MouseOverExitButton   = false;
            MouseOverCornerResize = false;

            if (this.RelativeMousePosition.Y < _titleBarBounds.Bottom) {
                if (_layoutExitButtonBounds.Contains(this.RelativeMousePosition)) {
                    MouseOverExitButton = true;
                } else {
                    MouseOverTitleBar = true;
                }
            } else if (_canResize && _layoutWindowCornerBounds.Contains(this.RelativeMousePosition)) {
                // TODO: Reduce the size of the corner resize area - compare to in game region for reference
                MouseOverCornerResize = true;
            }

            base.OnMouseMoved(e);
        }

        /// <inheritdoc />
        protected override void OnMouseLeft(MouseEventArgs e) {
            MouseOverTitleBar     = false;
            MouseOverExitButton   = false;
            MouseOverCornerResize = false;

            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            if (MouseOverTitleBar) {
                Dragging  = true;
                DragStart = Input.Mouse.Position;
            } else if (MouseOverExitButton) {
                Hide();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            Dragging = false;

            base.OnLeftMouseButtonReleased(e);
        }

        #region Window Navigation

        private readonly LinkedList<Panel> _currentNav = new LinkedList<Panel>();

        public virtual void Navigate(Panel newPanel, bool keepHistory = true) {
            if (!keepHistory)
                _currentNav.Clear();

            _currentNav.AddLast(newPanel);

            this.ActivePanel = newPanel;
        }

        public virtual void NavigateBack() {
            if (_currentNav.Count > 1)
                _currentNav.RemoveLast();

            this.ActivePanel = _currentNav.Last.Value;
        }

        public virtual void NavigateHome() {
            this.ActivePanel = _currentNav.First.Value;

            _currentNav.Clear();

            _currentNav.AddFirst(this.ActivePanel);
        }

        #endregion

        public void ToggleWindow() {
            if (_visible) Hide();
            else Show();
        }

        public override void Show() {
            if (_visible) return;

            // TODO: Ensure window can't also go off too far to the right or bottom
            this.Location = new Point(
                Math.Max(0, _location.X),
                Math.Max(0, _location.Y)
            );

            this.Opacity = 0;
            this.Visible = true;

            _animFade.Resume();
        }
        
        public override void Hide() {
            if (!this.Visible) return;

            _animFade.Resume();
            Content.PlaySoundEffectByName(@"window-close");
        }

        public override void UpdateContainer(GameTime gameTime) {
            if (Dragging) {
                var nOffset = Input.Mouse.Position - DragStart;
                Location += nOffset;

                DragStart = Input.Mouse.Position;
            }
        }

        #region Paint Window

        protected virtual void PaintWindowBackground(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this,
                                   _windowBackground,
                                   bounds,
                                   null,
                                   Color.White,
                                   0f,
                                   _windowBackgroundOrigin);
        }

        protected virtual void PaintExitButton(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, MouseOverExitButton
                                             ? _textureExitButtonActive
                                             : _textureExitButton,
                                   bounds);
        }

        protected virtual void PaintTitleBar(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_mouseOver && MouseOverTitleBar) {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeftActive,  _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeftActive,  _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRightActive, _layoutRightTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRightActive, _layoutRightTitleBarBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeft,  _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeft,  _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRight, _layoutRightTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRight, _layoutRightTitleBarBounds);
            }

            if (!string.IsNullOrEmpty(_title)) {
                spriteBatch.DrawStringOnCtrl(this,
                                             _title,
                                             Content.DefaultFont32,
                                             _layoutLeftTitleBarBounds.OffsetBy(80, 0),
                                             ContentService.Colors.ColonialWhite);

                if (!string.IsNullOrEmpty(_subtitle)) {
                    spriteBatch.DrawStringOnCtrl(this,
                                                 _subtitle,
                                                 Content.DefaultFont16,
                                                 _layoutSubtitleBounds,
                                                 Color.White);
                }
            }
        }

        protected virtual void PaintEmblem(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_emblem != null) {
                spriteBatch.DrawOnCtrl(this,
                                       _emblem,
                                       _emblem.Bounds.Subtract(new Rectangle(_emblem.Width / 8, _emblem.Height / 4, 0, 0)));
            }
        }

        protected virtual void PaintCorner(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_canResize) {
                spriteBatch.DrawOnCtrl(this,
                                       MouseOverCornerResize
                                           ? _textureWindowResizableCornerActive
                                           : _textureWindowResizableCorner,
                                       _layoutWindowCornerBounds);
            } else {
                spriteBatch.DrawOnCtrl(this,
                                       _textureWindowCorner,
                                       _layoutWindowCornerBounds);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (StandardWindow) {
                PaintWindowBackground(spriteBatch, _windowBackgroundBounds.Subtract(new Rectangle(0, -4, 0, 0)));
                PaintTitleBar(spriteBatch, bounds);
            }

            PaintExitButton(spriteBatch, _layoutExitButtonBounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (StandardWindow) {
                PaintEmblem(spriteBatch, bounds);

                PaintCorner(spriteBatch, bounds);
            }
        }

#endregion

    }
}
