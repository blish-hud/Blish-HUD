using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public abstract class WindowBase2 : Container, IWindow, IViewContainer {

        private const int STANDARD_TITLEBAR_HEIGHT = 40;

        private const int STANDARD_TITLEBAR_VERTICAL_OFFSET        = 11; // The vertical distance into the titlebar textures before it starts
        private const int STANDARD_LEFTTITLEBAR_HORIZONTAL_OFFSET  = 2;  // The horizontal distance into the left titlebar texture until it starts
        private const int STANDARD_RIGHTTITLEBAR_HORIZONTAL_OFFSET = 16; // The horizontal distance into the right titlebar texture until it starts

        private const int STANDARD_TITLEOFFSET    = 80;
        private const int STANDARD_SUBTITLEOFFSET = 20;

        private const int STANDARD_MARGIN = 16; // Standard margin used to space the "X" button, etc.

        private const int SIDEBAR_WIDTH = 46;

        #region Load Static

        private const string WINDOW_SETTINGS = "WindowSettings2";

        private static readonly Texture2D _textureTitleBarLeft;
        private static readonly Texture2D _textureTitleBarRight;
        private static readonly Texture2D _textureTitleBarLeftActive;
        private static readonly Texture2D _textureTitleBarRightActive;

        private static readonly Texture2D _textureExitButton;
        private static readonly Texture2D _textureExitButtonActive;

        private static readonly Texture2D _textureWindowCorner;
        private static readonly Texture2D _textureWindowResizableCorner;
        private static readonly Texture2D _textureWindowResizableCornerActive;

        private static readonly Texture2D _textureBlackFade;
        private static readonly Texture2D _textureSplitLine;

        private static readonly SettingCollection _windowSettings;

        static WindowBase2() {
            _textureTitleBarLeft        = Content.GetTexture("titlebar-inactive");
            _textureTitleBarRight       = Content.GetTexture("window-topright");
            _textureTitleBarLeftActive  = Content.GetTexture("titlebar-active");
            _textureTitleBarRightActive = Content.GetTexture("window-topright-active");

            _textureExitButton       = Content.GetTexture("button-exit");
            _textureExitButtonActive = Content.GetTexture("button-exit-active");

            _textureWindowCorner                = Content.GetTexture(@"controls/window/156008");
            _textureWindowResizableCorner       = Content.GetTexture(@"controls/window/156009");
            _textureWindowResizableCornerActive = Content.GetTexture(@"controls/window/156010");

            _textureSplitLine = Content.GetTexture("605026");
            _textureBlackFade = Content.GetTexture("fade-down-46");

            _windowSettings = GameService.Settings.Settings.AddSubCollection(WINDOW_SETTINGS);
        }

        #endregion

        #region Static Window Management

        private static readonly List<IWindow> _windows = new List<IWindow>();

        /// <summary>
        /// Registers the window so that its zindex can be calculated against other windows.
        /// </summary>
        public static void RegisterWindow(IWindow window) {
            _windows.Add(window);
        }

        /// <summary>
        /// Unregisters the window so that its zindex is not longer calculated against other windows.
        /// </summary>
        public static void UnregisterWindow(IWindow window) {
            _windows.Remove(window);
        }

        /// <summary>
        /// Returns the calculated zindex offset.  This should be added to the base zindex (typically <see cref="Screen.WINDOW_BASEZINDEX"/>) and returned as the zindex.
        /// </summary>
        public static int GetZIndex(IWindow thisWindow) {
            if (!_windows.Contains(thisWindow)) {
                throw new InvalidOperationException($"{nameof(thisWindow)} must be registered with {nameof(RegisterWindow)} before ZIndex can automatically be calculated.");
            }

            return Screen.WINDOW_BASEZINDEX + _windows.OrderBy(window => window.TopMost)
                                                      .ThenBy(window => window.LastInteraction)
                                                      .TakeWhile(window => window != thisWindow)
                                                      .Count();
        }

        #endregion

        public override int ZIndex {
            get => _zIndex + WindowBase2.GetZIndex(this);
            set => SetProperty(ref _zIndex, value);
        }

        private string _title = "No Title";
        /// <summary>
        /// The text shown at the top of the window.
        /// </summary>
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        private string _subtitle = "";
        /// <summary>
        /// The text shown to the right of the title in the title bar.
        /// This text is smaller and is normally used to show the current tab name and/or hotkey used to open the window.
        /// </summary>
        public string Subtitle {
            get => _subtitle;
            set => SetProperty(ref _subtitle, value, true);
        }

        private bool _canClose = true;
        public bool CanClose {
            get => _canClose;
            set => SetProperty(ref _canClose, value);
        }

        private bool _canResize = false;
        /// <summary>
        /// Allows the window to be resized by dragging the bottom right corner.
        /// </summary>
        /// <remarks>This property has not been implemented.</remarks>
        public bool CanResize {
            get => _canResize;
            set => SetProperty(ref _canResize, value);
        }

        private Texture2D _emblem = null;
        /// <summary>
        /// The emblem/badge displayed in the top left corner of the window.
        /// </summary>
        public Texture2D Emblem {
            get => _emblem;
            set => SetProperty(ref _emblem, value);
        }

        private bool _topMost;
        /// <summary>
        /// If this window will show on top of all other windows, regardless of which one had focus last.
        /// </summary>
        public bool TopMost {
            get => _topMost;
            set => SetProperty(ref _topMost, value);
        }

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

        private bool _dragging;
        /// <summary>
        /// Indicates if the window is actively being dragged.
        /// </summary>
        public bool Dragging {
            get => _dragging;
            private set => SetProperty(ref _dragging, value);
        }

        private bool _resizing;
        /// <summary>
        /// Indicates if the window is actively being resized.
        /// </summary>
        public bool Resizing {
            get => _resizing;
            private set => SetProperty(ref _resizing, value);
        }

        private readonly Glide.Tween _animFade;

        protected WindowBase2() {
            WindowBase2.RegisterWindow(this);

            this.Opacity = 0f;
            this.Visible = false;

            _zIndex = Screen.WINDOW_BASEZINDEX;

            this.ClipsBounds = false;

            GameService.Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseRelease;

            // TODO: Use window mask when fading windows in and out instead of this lame opacity transition
            _animFade = Animation.Tweener.Tween(this, new { Opacity = 1f }, 0.2f).Repeat().Reflect();
            _animFade.Pause();

            _animFade.OnComplete(() => {
                _animFade.Pause();
                if (_opacity <= 0) this.Visible = false;
            });
        }

        public override void UpdateContainer(GameTime gameTime) {
            if (this.Dragging) {
                var nOffset = Input.Mouse.Position - _dragStart;
                Location += nOffset;

                _dragStart = Input.Mouse.Position;
            } else if (this.Resizing) {
                var nOffset = Input.Mouse.Position - _dragStart;
                this.Size = HandleWindowResize(_resizeStart + nOffset);
            }
        }

        #region Show & Hide

        /// <summary>
        /// Shows the window if it is hidden.
        /// Hides the window if it is currently showing.
        /// </summary>
        public void ToggleWindow() {
            if (this.Visible) {
                Hide();
            } else {
                Show();
            }
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public override void Show() {
            BringWindowToFront();

            if (this.Visible) return;

            // Restore position from previous session
            if (this.SavesPosition && this.Id != null) {
                if (_windowSettings.TryGetSetting(this.Id, out var windowPosition)) {
                    this.Location = (windowPosition as SettingEntry<Point> ?? new SettingEntry<Point>()).Value;
                }
            }

            // Ensure that the window is actually on the screen (accounts for screen size changes, etc.)
            this.Location = new Point(MathHelper.Clamp(_location.X, 0, GameService.Graphics.SpriteScreen.Width  - 64),
                                      MathHelper.Clamp(_location.Y, 0, GameService.Graphics.SpriteScreen.Height - 64));

            this.Opacity = 0;
            this.Visible = true;

            _animFade.Resume();
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public override void Hide() {
            if (!this.Visible) return;

            this.Dragging = false;
            _animFade.Resume();
            Content.PlaySoundEffectByName(@"window-close");
        }

        #endregion

        #region ViewContainer

        public ViewState ViewState   { get; protected set; } = ViewState.None;
        public IView     CurrentView { get; protected set; }

        protected void ShowView(IView view) {
            ClearView();

            if (view == null) return;

            this.ViewState = ViewState.Loading;

            this.CurrentView = view;

            var progressIndicator = new Progress<string>((progressReport) => { /* NOOP */ });

            view.Loaded += OnViewBuilt;
            view.DoLoad(progressIndicator).ContinueWith(BuildView);
        }

        protected void ClearView() {
            this.ClearChildren();
            this.ViewState = ViewState.None;
        }

        private void OnViewBuilt(object sender, EventArgs e) {
            this.CurrentView.Loaded -= OnViewBuilt;

            ViewState = ViewState.Loaded;
        }

        private void BuildView(Task<bool> loadResult) {
            if (loadResult.Result) {
                this.CurrentView.DoBuild(this);
            }
        }

        #endregion

        #region Implementation Properties

        private bool _showSideBar = false;
        protected bool ShowSideBar {
            get => _showSideBar;
            set => SetProperty(ref _showSideBar, value);
        }

        private int _sideBarHeight = 100;
        protected int SideBarHeight {
            get => _sideBarHeight;
            set => SetProperty(ref _sideBarHeight, value, true);
        }

        private double _lastWindowInteract;
        double IWindow.LastInteraction => _lastWindowInteract;

        #endregion

        #region Window Regions

        // Mouse regions

        protected Rectangle TitleBarBounds      { get; private set; } = Rectangle.Empty;
        protected Rectangle ExitButtonBounds    { get; private set; } = Rectangle.Empty;
        protected Rectangle ResizeHandleBounds  { get; private set; } = Rectangle.Empty;
        protected Rectangle SidebarActiveBounds { get; private set; } = Rectangle.Empty;

        // Draw regions

        private Rectangle _backgroundDrawBounds      = Rectangle.Empty;
        private Rectangle _leftTitleBarDrawBounds    = Rectangle.Empty;
        private Rectangle _rightTitleBarDrawBounds   = Rectangle.Empty;
        private Rectangle _subtitleDrawBounds        = Rectangle.Empty;
        private Rectangle _emblemDrawBounds          = Rectangle.Empty;
        private Rectangle _sidebarInactiveDrawBounds = Rectangle.Empty;
        
        public override void RecalculateLayout() {
            // Title bar bounds
            _rightTitleBarDrawBounds = new Rectangle(this.TitleBarBounds.Width - _textureTitleBarRight.Width + STANDARD_RIGHTTITLEBAR_HORIZONTAL_OFFSET,
                                                     this.TitleBarBounds.Y - STANDARD_TITLEBAR_VERTICAL_OFFSET,
                                                     _textureTitleBarRight.Width,
                                                     _textureTitleBarRight.Height);

            // The left bar could end up too long, so we shrink its width down some to avoid drawing too far into the right titlebar
            _leftTitleBarDrawBounds = new Rectangle(this.TitleBarBounds.Location.X - STANDARD_LEFTTITLEBAR_HORIZONTAL_OFFSET,
                                                    this.TitleBarBounds.Location.Y - STANDARD_TITLEBAR_VERTICAL_OFFSET,
                                                    Math.Min(_textureTitleBarLeft.Width, _rightTitleBarDrawBounds.Left - STANDARD_LEFTTITLEBAR_HORIZONTAL_OFFSET),
                                                    _textureTitleBarLeft.Height);

            // Title bar text bounds
            if (!string.IsNullOrWhiteSpace(this.Title) && !string.IsNullOrWhiteSpace(this.Subtitle)) {
                int titleTextWidth = (int) Content.DefaultFont32.MeasureString(this.Title).Width;

                _subtitleDrawBounds = _leftTitleBarDrawBounds.OffsetBy(STANDARD_TITLEOFFSET + titleTextWidth + STANDARD_SUBTITLEOFFSET, 0);
            }

            // Emblem bounds
            if (_emblem != null) {
                _emblemDrawBounds = new Rectangle(_leftTitleBarDrawBounds.X + STANDARD_TITLEOFFSET / 2 - _emblem.Width               / 2 - STANDARD_MARGIN,
                                                  _leftTitleBarDrawBounds.Bottom                       - _textureTitleBarLeft.Height / 2 - _emblem.Height / 2,
                                                  _emblem.Width,
                                                  _emblem.Height);
            }

            // Exit button bounds
            this.ExitButtonBounds = new Rectangle(_rightTitleBarDrawBounds.Right - (STANDARD_MARGIN * 2) - _textureExitButton.Width,
                                                  _rightTitleBarDrawBounds.Y     + STANDARD_MARGIN,
                                                  _textureExitButton.Width,
                                                  _textureExitButton.Height);

            // Side bar bounds
            int sideBarTop        = _leftTitleBarDrawBounds.Bottom - STANDARD_TITLEBAR_VERTICAL_OFFSET;
            int sideBarHeight     = this.WindowRegion.Height       + STANDARD_TITLEBAR_VERTICAL_OFFSET;

            this.SidebarActiveBounds   = new Rectangle(_leftTitleBarDrawBounds.X, sideBarTop, SIDEBAR_WIDTH, this.SideBarHeight);
            _sidebarInactiveDrawBounds = new Rectangle(_leftTitleBarDrawBounds.X, sideBarTop + this.SideBarHeight, SIDEBAR_WIDTH, sideBarHeight - this.SideBarHeight);

            // Corner bounds
            this.ResizeHandleBounds = new Rectangle(this.Width  - _textureWindowCorner.Width,
                                                    this.Height - _textureWindowCorner.Height,
                                                    _textureWindowCorner.Width,
                                                    _textureWindowCorner.Height);
        }

        #endregion

        #region Window States

        protected bool MouseOverTitleBar     { get; private set; }
        protected bool MouseOverExitButton   { get; private set; }
        protected bool MouseOverResizeHandle { get; private set; }

        private Point _dragStart   = Point.Zero;
        private Point _resizeStart = Point.Zero;

        protected override void OnMouseMoved(MouseEventArgs e) {
            ResetMouseRegionStates();

            if (this.RelativeMousePosition.Y < this.TitleBarBounds.Bottom) {
                if (this.ExitButtonBounds.Contains(this.RelativeMousePosition)) {
                    this.MouseOverExitButton = true;
                } else {
                    this.MouseOverTitleBar = true;
                }
            } else if (_canResize && this.ResizeHandleBounds.Contains(this.RelativeMousePosition)) {
                this.MouseOverResizeHandle = true;
            }

            base.OnMouseMoved(e);
        }

        private void OnGlobalMouseRelease(object sender, MouseEventArgs e) {
            if (this.Visible && (this.Dragging || this.Resizing)) {
                // Save position for next launch
                if (this.SavesPosition && this.Id != null) {
                    (_windowSettings[this.Id] as SettingEntry<Point> ?? _windowSettings.DefineSetting(this.Id, this.Location)).Value = this.Location;
                }

                this.Dragging = false;
                this.Resizing = false;
            }
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            ResetMouseRegionStates();
            base.OnMouseLeft(e);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            BringWindowToFront();

            if (this.MouseOverTitleBar) {
                this.Dragging = true;
                _dragStart    = Input.Mouse.Position;
            } else if (this.MouseOverResizeHandle) {
                this.Resizing = true;
                _resizeStart  = this.Size;
                _dragStart    = Input.Mouse.Position;
            } else if (this.MouseOverExitButton && this.CanClose) {
                Hide();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        private void ResetMouseRegionStates() {
            this.MouseOverTitleBar     = false;
            this.MouseOverExitButton   = false;
            this.MouseOverResizeHandle = false;
        }

        /// <summary>
        /// Modifies the window size as it's being resized.
        /// Override to lock the window size at specific intervals or implement other resize behaviors.
        /// </summary>
        protected virtual Point HandleWindowResize(Point newSize) {
            return new Point(MathHelper.Clamp(newSize.X, SidebarActiveBounds.Right + STANDARD_MARGIN, 1024),
                             MathHelper.Clamp(newSize.Y, TitleBarBounds.Bottom     + STANDARD_MARGIN, 1024));
        }

        public void BringWindowToFront() {
            _lastWindowInteract = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        #endregion

        #region Window Construction

        protected Texture2D WindowBackground            { get; set; }
        protected Rectangle WindowRegion                { get; set; }
        protected Rectangle WindowRelativeContentRegion { get; set; }

        protected void ConstructWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) {
            this.WindowBackground = background;

            this.WindowRegion                = windowRegion;
            this.WindowRelativeContentRegion = contentRegion;
            this.TitleBarBounds              = new Rectangle(0, 0, windowRegion.Width, STANDARD_TITLEBAR_HEIGHT);

            this.Size = new Point(windowRegion.Width, windowRegion.Height + STANDARD_TITLEBAR_HEIGHT);

            _backgroundDrawBounds = new Rectangle(-windowRegion.Left, -windowRegion.Top + STANDARD_TITLEBAR_HEIGHT, background.Width, background.Height);

            this.Padding = new Thickness(Math.Max(windowRegion.Top - STANDARD_TITLEBAR_HEIGHT, STANDARD_TITLEBAR_VERTICAL_OFFSET), // We have to include the padding of the titlebar just in case
                                         background.Width                        - windowRegion.Right,
                                         background.Height - windowRegion.Bottom + STANDARD_TITLEBAR_HEIGHT,
                                         windowRegion.Left);

            this.ContentRegion = new Rectangle(contentRegion.X                            - (int)this.Padding.Left,
                                               contentRegion.Y + STANDARD_TITLEBAR_HEIGHT - (int)this.Padding.Top,
                                               contentRegion.Width,
                                               contentRegion.Height);
        }

        #endregion

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            PaintWindowBackground(spriteBatch);
            PaintSideBar(spriteBatch);
            PaintTitleBar(spriteBatch);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            PaintEmblem(spriteBatch);
            PaintTitleText(spriteBatch);
            PaintExitButton(spriteBatch);
            PaintCorner(spriteBatch);
        }

        private void PaintCorner(SpriteBatch spriteBatch) {
            if (this.CanResize) {
                spriteBatch.DrawOnCtrl(this,
                                       this.MouseOverResizeHandle || this.Resizing
                                       ? _textureWindowResizableCornerActive
                                       : _textureWindowResizableCorner,
                                       this.ResizeHandleBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, _textureWindowCorner, this.ResizeHandleBounds);
            }
        }

        private void PaintSideBar(SpriteBatch spriteBatch) {
            if (this.ShowSideBar) {
                // Draw solid side bar (top half)
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this.SidebarActiveBounds, Color.Black);

                // Draw faded side bar (bottom half)
                spriteBatch.DrawOnCtrl(this, _textureBlackFade, this._sidebarInactiveDrawBounds);

                // Draw the splitter
                spriteBatch.DrawOnCtrl(this, _textureSplitLine, new Rectangle(this.SidebarActiveBounds.Right - _textureSplitLine.Width / 2, this.SidebarActiveBounds.Top, _textureSplitLine.Width, this._sidebarInactiveDrawBounds.Bottom - this.SidebarActiveBounds.Top));
            }
        }

        private void PaintWindowBackground(SpriteBatch spriteBatch) {
            if (this.ShowSideBar) {
                spriteBatch.DrawOnCtrl(this, this.WindowBackground, _backgroundDrawBounds.OffsetBy(this.WindowRegion.Left + SIDEBAR_WIDTH, 0), new Rectangle(this.WindowRegion.Left + SIDEBAR_WIDTH, 0, this.WindowBackground.Width - this.WindowRegion.Left + SIDEBAR_WIDTH, this.WindowBackground.Height));
            } else {
                spriteBatch.DrawOnCtrl(this, this.WindowBackground, _backgroundDrawBounds);
            }
        }

        private void PaintTitleBar(SpriteBatch spriteBatch) {
            if (this.MouseOver && this.MouseOverTitleBar) {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeftActive,  _leftTitleBarDrawBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRightActive, _rightTitleBarDrawBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, _textureTitleBarLeft,  _leftTitleBarDrawBounds);
                spriteBatch.DrawOnCtrl(this, _textureTitleBarRight, _rightTitleBarDrawBounds);
            }
        }

        private void PaintTitleText(SpriteBatch spriteBatch) {
            if (!string.IsNullOrWhiteSpace(this.Title)) {
                spriteBatch.DrawStringOnCtrl(this, this.Title, Content.DefaultFont32, _leftTitleBarDrawBounds.OffsetBy(STANDARD_TITLEOFFSET, 0), ContentService.Colors.ColonialWhite);

                if (!string.IsNullOrWhiteSpace(this.Subtitle)) {
                    spriteBatch.DrawStringOnCtrl(this, this.Subtitle, Content.DefaultFont16, _subtitleDrawBounds, Color.White);
                }
            }
        }

        private void PaintExitButton(SpriteBatch spriteBatch) {
            if (this.CanClose) {
                spriteBatch.DrawOnCtrl(this, MouseOverExitButton
                                                 ? _textureExitButtonActive
                                                 : _textureExitButton,
                                       this.ExitButtonBounds);
            }
        }

        private void PaintEmblem(SpriteBatch spriteBatch) {
            if (_emblem != null) {
                spriteBatch.DrawOnCtrl(this, this.Emblem, _emblemDrawBounds);
            }
        }

        protected override void DisposeControl() {
            if (this.CurrentView != null) {
                this.CurrentView.Loaded -= OnViewBuilt;
                this.CurrentView.DoUnload();
            }

            WindowBase2.UnregisterWindow(this);

            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseRelease;

            base.DisposeControl();
        }

    }
}
