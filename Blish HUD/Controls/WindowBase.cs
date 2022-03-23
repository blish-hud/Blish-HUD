using System;
using System.Collections.Generic;
using Blish_HUD.Graphics;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    [Obsolete("This control will be removed in the future.  Use WindowBase2 instead.")]
    public abstract class WindowBase : Container, IWindow {

        private const int COMMON_MARGIN = 16;
        private const int TITLE_OFFSET = 80;
        private const int SUBTITLE_OFFSET = 20;

        #region Load Static

        private const string WINDOW_SETTINGS = "WindowSettings";

        private static readonly Texture2D _textureTitleBarLeft        = Content.GetTexture("titlebar-inactive");
        private static readonly Texture2D _textureTitleBarRight       = Content.GetTexture("window-topright");
        private static readonly Texture2D _textureTitleBarLeftActive  = Content.GetTexture("titlebar-active");
        private static readonly Texture2D _textureTitleBarRightActive = Content.GetTexture("window-topright-active");

        private static readonly Texture2D _textureExitButton       = Content.GetTexture("button-exit");
        private static readonly Texture2D _textureExitButtonActive = Content.GetTexture("button-exit-active");

        private static readonly Texture2D _textureWindowCorner                = Content.GetTexture(@"controls/window/156008");
        private static readonly Texture2D _textureWindowResizableCorner       = Content.GetTexture(@"controls/window/156009");
        private static readonly Texture2D _textureWindowResizableCornerActive = Content.GetTexture(@"controls/window/156010");

        private static readonly SettingCollection _windowSettings = GameService.Settings.Settings.AddSubCollection(WINDOW_SETTINGS);

        #endregion

        public override int ZIndex {
            get => _zIndex + WindowBase2.GetZIndex(this);
            set => SetProperty(ref _zIndex, value);
        }

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

        public double LastInteraction => _lastInteraction;

        protected bool _savesPosition;
        /// <summary>
        /// If <c>true</c>, the window will remember its position between Blish HUD sessions.
        /// Requires that <see cref="Id"/> be set.
        /// </summary>
        public bool SavesPosition {
            get => _savesPosition;
            set => SetProperty(ref _savesPosition, value);
        }

        private string _id;
        /// <summary>
        /// A unique id to identify the window.  Used with <see cref="SavesPosition"/> as a unique
        /// identifier to remember where the window is positioned.
        /// </summary>
        public string Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private bool _savedVisibility = false;

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
            WindowBase2.RegisterWindow(this);

            this.Opacity = 0f;
            this.Visible = false;

            _zIndex = Screen.WINDOW_BASEZINDEX;

            Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseRelease;

            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += delegate { UpdateWindowBaseDynamicHUDCombatState(this); };
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += delegate { UpdateWindowBaseDynamicHUDLoadingState(this); };

            _animFade = Animation.Tweener.Tween(this, new { Opacity = 1f }, 0.2f).Repeat().Reflect();
            _animFade.Pause();

            _animFade.OnComplete(() => {
                _animFade.Pause();
                if (_opacity <= 0) this.Visible = false;
            });
        }

        public static void UpdateWindowBaseDynamicHUDCombatState(WindowBase wb) {
            if (GameService.Overlay.DynamicHUDWindows == DynamicHUDMethod.ShowPeaceful && GameService.Gw2Mumble.PlayerCharacter.IsInCombat) {
                wb._savedVisibility = wb.Visible;
                if (wb._savedVisibility) wb.Hide();
            } else {
                if (wb._savedVisibility) wb.Show();
            }
        }

        public static void UpdateWindowBaseDynamicHUDLoadingState(WindowBase wb) {
            if (GameService.Overlay.DynamicHUDLoading == DynamicHUDMethod.NeverShow && !GameService.GameIntegration.Gw2Instance.IsInGame) {
                wb._savedVisibility = wb.Visible;
                if (wb._savedVisibility) wb.Hide();
            } else {
                if (wb._savedVisibility) wb.Show();
            }
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

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            BringWindowToFront();

            if (MouseOverTitleBar) {
                Dragging  = true;
                DragStart = Input.Mouse.Position;
            } else if (MouseOverExitButton) {
                Hide();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        private void OnGlobalMouseRelease(object sender, MouseEventArgs e) {
            if (this.Visible && this.Dragging) {
                // Save position for next launch
                if (this.SavesPosition && this.Id != null) {
                    (_windowSettings[this.Id] as SettingEntry<Point> ?? _windowSettings.DefineSetting(this.Id, this.Location)).Value = this.Location;
                }

                Dragging = false;
            }
        }

        public void BringWindowToFront() {
            _lastInteraction = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        public bool CanClose => true;

        #region Window Navigation

        private readonly LinkedList<Panel> _currentNav = new LinkedList<Panel>();
        private          double            _lastInteraction;

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
            BringWindowToFront();

            if (_visible) return;

            // Restore position from previous session
            if (this.SavesPosition && this.Id != null && _windowSettings.TryGetSetting(this.Id, out var windowPosition)) {
                this.Location = (windowPosition as SettingEntry<Point> ?? new SettingEntry<Point>()).Value;
            }

            // Ensure that the window is actually on the screen (accounts for screen size changes, etc.)
            this.Location = new Point(
                MathHelper.Clamp(_location.X, 0, GameService.Graphics.SpriteScreen.Width - 64),
                MathHelper.Clamp(_location.Y, 0, GameService.Graphics.SpriteScreen.Height - 64)
            );

            this.Opacity = 0;
            this.Visible = true;

            _animFade.Resume();
        }
        
        public override void Hide() {
            if (!this.Visible) return;

            this.Dragging = false;
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
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRightActive, _layoutRightTitleBarBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeft,  _layoutLeftTitleBarBounds);
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

        protected override void DisposeControl() {
            WindowBase2.UnregisterWindow(this);

            Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseRelease;

            base.DisposeControl();
        }

    }
}
