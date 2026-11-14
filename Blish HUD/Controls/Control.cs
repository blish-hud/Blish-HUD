using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Input;
using Newtonsoft.Json;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;
using System.Threading;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Defines how input should be captured and if that input should be blocked from passing onward to Guild Wars 2.
    /// </summary>
    [Flags]
    public enum CaptureType {
        None = 0x0,

        /// <summary>
        /// Mouse and mouse wheel inputs are intercepted but this control will not block input from passing to Guild Wars 2 or to other Blish HUD controls.
        /// </summary>
        Filter = 0x1,

        /// <summary>
        /// Not implemented.
        /// </summary>
        [Obsolete("This capture type is not implemented.  Do not use it.", true)]
        Keyboard = 0x2,

        /// <summary>
        /// The default <c>CaptureType</c>.
        /// Intercepts mouse input (allowing for: <see cref="Control.LeftMouseButtonPressed"/>, <see cref="Control.LeftMouseButtonReleased"/>, <see cref="Control.RightMouseButtonPressed"/>,
        /// <see cref="Control.RightMouseButtonReleased"/>, <see cref="Control.MouseMoved"/>, <see cref="Control.MouseEntered"/>, <see cref="Control.MouseLeft"/>).
        /// Input is also blocked from being sent through to Guild Wars 2 unless the <see cref="DoNotBlock"/> flag is also set.
        /// </summary>
        Mouse = 0x4,

        /// <summary>
        /// Intercepts mouse wheel scrolling (allowing for: <see cref="Control.MouseWheelScrolled"/>).
        /// Mouse wheel input is also blocked from being sent through to Guild Wars 2 unless the <see cref="DoNotBlock"/> flag is also set.
        /// </summary>
        MouseWheel = 0x8,

        /// <summary>
        /// If applied, the control will not block mouse or mouse wheel input from passing to Guild Wars 2 regardless of what input it detects from other flags.
        /// </summary>
        DoNotBlock = 0x16
    }

    public abstract class Control : INotifyPropertyChanged, IDisposable {

        #region Static References

        #region Standards

        public static readonly DesignStandard ControlStandard = new DesignStandard(/*          Size */ Point.Zero,
                                                                                   /*   PanelOffset */ new Point(09, 28),
                                                                                   /* ControlOffset */ new Point(10, 10));

        public static class StandardColors {

            /// <summary>
            /// Color of standard text or untinted elements in an enabled control.
            ///
            /// The color is white (#FFFFFF).
            /// </summary>
            public static Color Default => new Color(0xffffffff);

            /// <summary>
            /// Color of standard text in a disabled control.
            ///
            /// The color is a dark gray (#AAAAAA).
            /// </summary>
            public static Color DisabledText => new Color(0xffaaaaaa);

            /// <summary>
            /// A tint often applied to control elements while the control is hovered over.
            ///
            /// The color is a light peach (#FFE4B5).
            /// </summary>
            public static Color Tinted => Color.FromNonPremultiplied(255, 228, 181, 255);

            /// <summary>
            /// Color of text or element shadows.
            ///
            /// The color is black (#000000).
            /// </summary>
            public static Color Shadow => new Color(0xff000000);

            /// <summary>
            /// Color of warning and alert text.  Also the color of lots of floating text.
            ///
            /// The color is yellow (#FFFF00).
            /// </summary>
            public static Color Yellow => Color.FromNonPremultiplied(255, 255, 0, 255);

            /// <summary>
            /// The color of error text.
            ///
            /// The color is red (#F20D13).
            /// </summary>
            public static Color Red => Color.FromNonPremultiplied(242, 13, 19, 255);

        }

        #endregion

        #region Resources

        private static readonly SpriteBatchParameters _defaultSpriteBatchParameters;

        #endregion

        static Control() {
            _defaultSpriteBatchParameters = new SpriteBatchParameters();

            Tooltip.EnableTooltips();
        }

        #endregion

        #region Static Control Events

        public static event EventHandler<ControlActivatedEventArgs> ActiveControlChanged;

        private static Control _activeControl;
        public static Control ActiveControl {
            get => _activeControl;
            set {
                if (_activeControl == value) return;

                _activeControl = value;

                OnActiveControlChanged(new ControlActivatedEventArgs(_activeControl));
            }
        }

        private static void OnActiveControlChanged(ControlActivatedEventArgs e) {
            ActiveControlChanged?.Invoke(null, e);
        }

        public static event EventHandler<ControlActivatedEventArgs> FocusedControlChanged;

        private static Control _focusedControl;
        public static Control FocusedControl {
            get => _focusedControl;
            set {
                if (_focusedControl == value) return;

                _focusedControl = value;

                OnFocusedControlChanged(new ControlActivatedEventArgs(_focusedControl));
            }
        }

        private static void OnFocusedControlChanged(ControlActivatedEventArgs e) {
            ActiveControlChanged?.Invoke(null, e);
        }

        #endregion

        #region Control Events

        #region Mouse Events

        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseMoved;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseWheelScrolled;
        public event EventHandler<MouseEventArgs> MouseEntered;
        public event EventHandler<MouseEventArgs> MouseLeft;

        /// <summary>
        /// Alias for <see cref="LeftMouseButtonReleased"/> with the difference that it only fires if <see cref="Enabled"/> is true.
        /// </summary>
        /// <remarks>Fires after <see cref="LeftMouseButtonReleased"/> fires.</remarks>
        public event EventHandler<MouseEventArgs> Click;

        /// <summary>
        /// Called when a left mouse button press occurs on the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnLeftMouseButtonPressed(MouseEventArgs e) {
            this.LeftMouseButtonPressed?.Invoke(this, e);

            _clickPrimed = true;
        }

        private double _lastClickTime = 0;
        private bool _clickPrimed = false;

        /// <summary>
        /// Called when a left mouse button release occurs on the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnLeftMouseButtonReleased(MouseEventArgs e) {
            this.LeftMouseButtonReleased?.Invoke(this, e);

            if (_enabled && _clickPrimed) {
                // Distinguish click from double-click
                if (GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _lastClickTime < SystemInformation.DoubleClickTime) {
                    var doubleClickArgs = new MouseEventArgs(e.EventType, true);
                    OnClick(doubleClickArgs);

                    // Ensure that when double-click propagation is stopped, it also
                    // stops propagating the single-click event
                    if (doubleClickArgs.PropagationStopped) {
                        e.StopPropagation();
                    }
                } else {
                    _lastClickTime = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
                    OnClick(e);
                }
            }
        }

        /// <summary>
        /// Called when the mouse moves over the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnMouseMoved(MouseEventArgs e) {
            this.MouseMoved?.Invoke(this, e);
        }

        /// <summary>
        /// Called when a right mouse button press occurs on the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnRightMouseButtonPressed(MouseEventArgs e) {
            this.RightMouseButtonPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Called when a right mouse button release occurs on the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnRightMouseButtonReleased(MouseEventArgs e) {
            this.RightMouseButtonReleased?.Invoke(this, e);
        }

        /// <summary>
        /// Called the mouse wheel is scrolled while the mouse is over the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnMouseWheelScrolled(MouseEventArgs e) {
            this.MouseWheelScrolled?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the mouse enters into the bounds of the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnMouseEntered(MouseEventArgs e) {
            this.MouseEntered?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the mouse exits from the bounds of the <see cref="Control"/>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnMouseLeft(MouseEventArgs e) {
            this.MouseLeft?.Invoke(this, e);

            _clickPrimed = false;
        }

        /// <summary>
        /// Called when a left mouse button press occurs on the <see cref="Control"/> while <see cref="Enabled"/> is <c>true</c>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnClick(MouseEventArgs e) {
            this.Click?.Invoke(this, e);
        }

        #endregion

        public event EventHandler<EventArgs> Shown;
        public event EventHandler<EventArgs> Hidden;
        public event EventHandler<ResizedEventArgs> Resized;
        public event EventHandler<MovedEventArgs> Moved;
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// Called when the value of <see cref="Visible"/> is changed to <c>true</c>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnShown(EventArgs e) {
            this.Shown?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the value of <see cref="Visible"/> is changed to <c>false</c>.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnHidden(EventArgs e) {
            this.Hidden?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="Size"/> of the <see cref="Control"/> is changed.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnResized(ResizedEventArgs e) {
            this.Resized?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="Location"/> of the <see cref="Control"/> is changed.
        /// If overriding, ensure you call the base method.
        /// </summary>
        protected virtual void OnMoved(MovedEventArgs e) {
            this.Moved?.Invoke(this, e);
        }

        #endregion

        protected bool _mouseOver = false;
        /// <summary>
        /// Indicates that the mouse cursor is actively over the control.
        /// </summary>
        [JsonIgnore]
        public bool MouseOver {
            get => _mouseOver;
            private set {
                if (SetProperty(ref _mouseOver, value)) {
                    if (_mouseOver) {
                        OnMouseEntered(new MouseEventArgs(MouseEventType.MouseEntered));
                    } else {
                        OnMouseLeft(new MouseEventArgs(MouseEventType.MouseLeft));
                    }
                }
            }
        }

        protected Point _location;
        /// <summary>
        /// Indicates the location of the <see cref="Control"/> relative to its <see cref="Parent"/>.
        /// </summary>
        public Point Location {
            get => _location;
            set {
                if (_location == value) return;

                var previousLocation = _location;

                _location = value;

                OnPropertyChanged();

                // We do this to make sure we raise PropertyChanged events for alias properties
                if (previousLocation.Y != _location.Y)
                    OnPropertyChanged(nameof(this.Top));
                if (previousLocation.X != _location.X)
                    OnPropertyChanged(nameof(this.Left));
                if (previousLocation.Y + _size.Y != _location.Y + _size.Y)
                    OnPropertyChanged(nameof(this.Bottom));
                if (previousLocation.X + _size.X != _location.X + _size.X)
                    OnPropertyChanged(nameof(this.Right));

                OnMoved(new MovedEventArgs(previousLocation, _location));
            }
        }

        #region Location Aliases

        [JsonIgnore]
        public int Top {
            get => _location.Y;
            set {
                if (_location.Y == value) return;

                this.Location = new Point(_location.X, value);
            }
        }

        [JsonIgnore]
        public int Right {
            get => _location.X + _size.X;
            set {
                if (value == _location.X + _size.X) return;

                this.Location = new Point(value - this.Width, _location.Y);
            }
        }

        [JsonIgnore]
        public int Bottom {
            get => _location.Y + _size.Y;
            set {
                if (value == _location.Y + _size.Y) return;

                this.Location = new Point(_location.X, value - this.Height);
            }
        }

        [JsonIgnore]
        public int Left {
            get => _location.X;
            set {
                if (value == _location.X) return;

                this.Location = new Point(value, _location.Y);
            }
        }

        #endregion

        protected Point _size = new Point(40, 20);
        /// <summary>
        /// The size of the control.  Both the X and Y component must be greater than 0.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either the X or Y component are 0 or less.</exception>
        public Point Size {
            get => _size;
            set {
                if (_size == value) return;

                // To render, the control must have positive dimensions
                if (value.X < 0 || value.Y < 0) return;

                var previousSize = _size;

                _size = value;

                OnPropertyChanged();

                if (previousSize.Y != _size.Y)
                    OnPropertyChanged(nameof(this.Height), true);
                if (previousSize.X != _size.X)
                    OnPropertyChanged(nameof(this.Width), true);
                if (_location.Y + previousSize.Y != _location.Y + _size.Y)
                    OnPropertyChanged(nameof(this.Bottom), true);
                if (_location.X + previousSize.X != _location.X + _size.X)
                    OnPropertyChanged(nameof(this.Right), true);

                OnResized(new ResizedEventArgs(previousSize, _size));

                this.Invalidate();
            }
        }

        #region Size aliases

        /// <summary>
        /// The width of the <see cref="Control"/>.
        /// </summary>
        [JsonIgnore]
        public int Width {
            get => _size.X;
            set {
                if (_size.X == value) return;

                this.Size = new Point(value, _size.Y);
            }
        }

        /// <summary>
        /// The height of the <see cref="Control"/>.
        /// </summary>
        [JsonIgnore]
        public int Height {
            get => _size.Y;
            set {
                if (_size.Y == value) return;

                this.Size = new Point(_size.X, value);
            }
        }

        #endregion

        private bool _clipsBounds = true;
        /// <summary>
        /// If <c>true</c>, the control's render is clipped to the <see cref="Container.ContentRegion"/> of its <see cref="Parent"/>.
        /// <br/><br/>Default: true
        /// </summary>
        public bool ClipsBounds {
            get => _clipsBounds;
            set => SetProperty(ref _clipsBounds, value);
        }

        private ControlEffect _effectBehind;
        /// <summary>
        /// The <see cref="ControlEffect"/> to apply behind the control.
        /// </summary>
        protected ControlEffect EffectBehind {
            get => _effectBehind;
            set => SetProperty(ref _effectBehind, value);
        }

        private ControlEffect _effectInFront;
        /// <summary>
        /// [NOT IMPLEMENTED] The <see cref="ControlEffect"/> to apply on top of the control.
        /// </summary>
        protected ControlEffect EffectInFront {
            get => _effectInFront;
            set => SetProperty(ref _effectInFront, value);
        }

        public virtual void UnsetFocus() {
            FocusedControl = null;
        }

        public virtual bool GetFocusState() { return false; }

        /// <summary>
        /// The bounds of the control, relative to the parent control.
        /// </summary>
        [JsonIgnore]
        public Rectangle LocalBounds => new Rectangle(this.Location, this.Size);

        // TODO: Ensure that when properties AbsoluteBounds derives from change, this one also raises a PropertyChanged event
        /// <summary>
        /// The bounds of the control, relative to the overlay window / SpriteScreen.
        /// </summary>
        [JsonIgnore]
        public Rectangle AbsoluteBounds {
            get {
                var parent = this.Parent;

                if (parent == null) return this.LocalBounds;

                var parentBounds        = parent.AbsoluteBounds;
                var parentContentRegion = parent.ContentRegion;

                // Clean this up
                // This is really the absolute bounds of the ContentRegion currently because mouse
                // input is currently using this to determine if the click was within the region
                return new Rectangle(parentBounds.X + parentContentRegion.X + _location.X - parent.HorizontalScrollOffset,
                                     parentBounds.Y + parentContentRegion.Y + _location.Y - parent.VerticalScrollOffset,
                                     _size.X,
                                     _size.Y);
            }
        }

        /// <summary>
        /// The position of the mouse cursor relative to the local bounds of this <see cref="Control"/>.
        /// </summary>
        [JsonIgnore]
        public Point RelativeMousePosition => Input.Mouse.Position - this.AbsoluteBounds.Location;

        private ContextMenuStrip _menu;
        /// <summary>
        /// If provided, the menu will display when the control is right-clicked on.
        /// </summary>
        public ContextMenuStrip Menu {
            get => _menu;
            set => SetProperty(ref _menu, value);
        }

        private Tooltip _tooltip;
        /// <summary>
        /// If provided, the Tooltip will display when the mouse is over the control.
        /// Do not use this if you are already using <see cref="BasicTooltipText"/>.
        /// </summary>
        public Tooltip Tooltip {
            get {
                if (_tooltip != null && !_tooltip._disposedValue) return _tooltip;

                return !string.IsNullOrWhiteSpace(_basicTooltipText)
                    ? _tooltip = new Tooltip(new BasicTooltipView(_basicTooltipText))
                    : null;
            }
            set => SetProperty(ref _tooltip, value);
        }

        private string _basicTooltipText;
        /// <summary>
        /// If provided, a tooltip will be shown with the provided text when the mouse is over the control.
        /// Do not use this if you are already using <see cref="Tooltip"/>.
        /// </summary>
        public string BasicTooltipText {
            get => _basicTooltipText;
            set {
                if (!SetProperty(ref _basicTooltipText, value)) return;

                if (Control.ActiveControl == this && _tooltip != null) {
                    // In the event that the tooltip text is changed while it's
                    // being shown (or the mouse is over the control), this
                    // portion will update it and display it (if it isn't already).

                    if (_tooltip.CurrentView is BasicTooltipView tooltipView && !string.IsNullOrWhiteSpace(value)) {
                        tooltipView.Text = value;
                        return;
                    }
                }

                _tooltip?.Hide();
                _tooltip = null;
            }
        }

        protected float _opacity = 1f;
        /// <summary>
        /// The opacity of the <see cref="Control"/>.
        /// </summary>
        public float Opacity {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        protected bool _visible = true;
        /// <summary>
        /// Indicates if the control is allowed to render or not.
        /// </summary>
        public bool Visible {
            get => _visible;
            set {
                if (SetProperty(ref _visible, value)) {
                    if (_visible) {
                        OnShown(EventArgs.Empty);
                    } else {
                        OnHidden(EventArgs.Empty);
                    }
                }
            }
        }

        protected Thickness _padding = Thickness.Zero;
        public Thickness Padding {
            get => _padding;
            set => SetProperty(ref _padding, value, true);
        }

        private Container _parent;
        /// <summary>
        /// The <see cref="Container"/> that this <see cref="Control"/> resides within.
        /// </summary>
        public Container Parent {
            get => _parent;
            set {
                var currentParent = _parent;

                if (SetProperty(ref _parent, value)) {
                    currentParent?.RemoveChild(this);

                    _parent = value;

                    if (this.Parent == null || !this.Parent.AddChild(this)) {
                        _parent = null;
                    }
                }
            }
        }

        protected int _zIndex = 5;
        /// <summary>
        /// Used to determine the render order of controls within a <see cref="Container"/> and when handling input.
        /// Higher values will be rendered later (causing them to be shown on top of other controls) and have priority for input.
        /// <br/><br/>Default: 5
        /// </summary>
        public virtual int ZIndex {
            get => _zIndex;
            set => SetProperty(ref _zIndex, value);
        }

        protected bool _enabled = true;
        /// <summary>
        /// If <c>true</c>, the <see cref="Control"/> can accept user input.
        /// <br/><br/>Default: true
        /// </summary>
        public bool Enabled {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        protected Color _backgroundColor = Color.Transparent;
        /// <summary>
        /// The <see cref="Color"/> rendered behind the <see cref="Control"/>.
        /// <br/><br/>Default: <see cref="Color.Transparent"/>
        /// </summary>
        [JsonIgnore]
        public Color BackgroundColor {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        #region Render Properties

        private int _layoutSuspendCount = 0;
        [JsonIgnore]
        internal bool IsLayoutSuspended => Interlocked.CompareExchange(ref _layoutSuspendCount, 0, 0) != 0 || (Parent?.IsLayoutSuspended).GetValueOrDefault();

        [JsonIgnore]
        internal LayoutState LayoutState { get; private set; } = LayoutState.SkipDraw;

        protected SpriteBatchParameters _spriteBatchParameters;

        [JsonIgnore]
        public SpriteBatchParameters SpriteBatchParameters {
            get => _spriteBatchParameters ?? _defaultSpriteBatchParameters;
            set {
                if (_spriteBatchParameters != _defaultSpriteBatchParameters)
                    _spriteBatchParameters = value;
            }
        }

        #endregion


        // TODO: Not sure if these are needed anymore since GameServices are much easier to reference now
        // Aliases to make life easier
        protected static ContentService   Content   => GameService.Content;
        protected static InputService     Input     => GameService.Input;
        protected static AnimationService Animation => GameService.Animation;
        protected static GraphicsService  Graphics  => GameService.Graphics;

        protected Control() {
            // TODO: This needs to get handled by the menustrip itself, not by the control
            /* They activate on the mouse down for the right-click menus which
               deviates from their norm (seems as if everything else activates on
               mouse button up - 🤷‍) */
            this.RightMouseButtonPressed += ActivateContextMenuStrip;
        }

        // TODO: This needs to be moved into the ContextMenuStrip class - the control itself shouldn't be doing this work
        private void ActivateContextMenuStrip(object sender, MouseEventArgs e) {
            if (this.Menu == null || !this.Enabled) return;

            this.Menu.Show(Input.Mouse.Position);
        }
        
        /// <summary>
        /// Avoid overriding <see cref="Invalidate"/> as it has the potential to be called multiple times prior to a render taking place.
        /// </summary>
        public virtual void Invalidate() {
            this.LayoutState = LayoutState.Invalidated;

            UpdateLayout();
        }

        /// <summary>
        /// Prefer <see cref="SuspendLayoutContext"/> if possible, as this ensures
        /// layout is resumed in all circumstances.
        /// <br/>
        /// The layout of the <see cref="Control"/> will be suspended until
        /// <see cref="ResumeLayout"/> is called.
        /// </summary>
        public void SuspendLayout() {
            Interlocked.Increment(ref _layoutSuspendCount);
        }

        /// <summary>
        /// Suspends the layout of the <see cref="Control"/> until the context is
        /// disposed (i.e. when exiting a <see langword="using"/> block) and ensures
        /// correct resumption of layout in the event of exceptions.
        /// </summary>
        public IDisposable SuspendLayoutContext() {
            return new SuspendLayoutScope(this);
        }

        /// <summary>
        /// Allows the layout of the control to be calculated on the next
        /// invalidate, if the layout is currently suspended.
        /// </summary>
        /// <param name="forceRecalculate">If <c>true</c>, will force the layout to update now instead of on the next invalidation.</param>
        public void ResumeLayout(bool forceRecalculate = false) {
            if (Interlocked.Decrement(ref _layoutSuspendCount) == 0 && forceRecalculate) {
                UpdateLayout();
            }
        }

        private void UpdateLayout() {
            try {
                if (Interlocked.Increment(ref this._layoutSuspendCount) == 1 &&
                    !(Parent?.IsLayoutSuspended).GetValueOrDefault() &&
                    this.LayoutState != LayoutState.Ready) {

                    RecalculateLayout();
                    this.LayoutState = LayoutState.Ready;
                }
            } finally {
                Interlocked.Decrement(ref this._layoutSuspendCount);
            }
        }

        /// <summary>
        /// Called whenever the size or location of the control is changed.  Is also called if <see cref="Invalidate"/> is called and should contain any calculations needed for the call to Draw the <see cref="Control"/>.
        /// </summary>
        public virtual void RecalculateLayout() {
            /* NOOP */
        }
        
        protected float AbsoluteOpacity(bool isInternal) {
            var parent = this.Parent;

            if (parent == null) return _opacity;

            return isInternal
                       ? parent.AbsoluteOpacity(true) - (1f - _opacity)
                       : MathHelper.Clamp(parent.AbsoluteOpacity(true) - (1f - _opacity), 0f, 1f);
        }

        public float AbsoluteOpacity() {
            return AbsoluteOpacity(false);
        }

        /// <summary>
        /// Specifies which type of input this <see cref="Control"/> accepts, possibly blocks from other <see cref="Control"/>s, and prevents the game from seeing.
        /// </summary>
        public CaptureType Captures => CapturesInput();
        
        /// <summary>
        /// Override to specify which type of input this <see cref="Control"/> accepts or intercepts.
        /// </summary>
        /// <seealso cref="Captures"/>
        protected virtual CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        [Obsolete("This overload does not support stopping propagation, use the overload that takes MouseEventArgs.")]
        protected void TriggerMouseEvent(MouseEventType mouseEventType) {
            TriggerMouseEvent(new MouseEventArgs(mouseEventType));
        }

        protected void TriggerMouseEvent(MouseEventArgs args) {
            switch (args.EventType) {
                case MouseEventType.LeftMouseButtonPressed:
                    OnLeftMouseButtonPressed(args);
                    break;
                case MouseEventType.LeftMouseButtonReleased:
                    OnLeftMouseButtonReleased(args);
                    break;
                case MouseEventType.RightMouseButtonPressed:
                    OnRightMouseButtonPressed(args);
                    break;
                case MouseEventType.RightMouseButtonReleased:
                    OnRightMouseButtonReleased(args);
                    break;
                case MouseEventType.MouseMoved:
                    OnMouseMoved(args);
                    this.MouseOver = true;
                    break;
                case MouseEventType.MouseWheelScrolled:
                    OnMouseWheelScrolled(args);
                    break;
            }
        }

        [Obsolete("This overload does not support stopping propagation, use the overload that takes MouseEventArgs.")]
        public virtual Control TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
            return TriggerMouseInput(new MouseEventArgs(mouseEventType), ms);
        }

        public virtual Control TriggerMouseInput(MouseEventArgs args, MouseState ms) {
            var inputCapture = CapturesInput();

            switch (args.EventType) {
                case MouseEventType.MouseMoved:
                case MouseEventType.LeftMouseButtonPressed:
                case MouseEventType.LeftMouseButtonReleased:
                case MouseEventType.RightMouseButtonPressed:
                case MouseEventType.RightMouseButtonReleased:
                case MouseEventType.MouseEntered:
                case MouseEventType.MouseLeft:
                    if (inputCapture.HasFlag(CaptureType.Mouse) || inputCapture.HasFlag(CaptureType.Filter)) {
                        TriggerMouseEvent(args);
                        return this;
                    }
                    break;
                case MouseEventType.MouseWheelScrolled:
                    if (inputCapture.HasFlag(CaptureType.MouseWheel) || inputCapture.HasFlag(CaptureType.Filter)) {
                        TriggerMouseEvent(args);
                        return this;
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// Makes the control visible.
        /// </summary>
        public virtual void Show() {
            this.Visible = true;
        }

        /// <summary>
        /// Hides the control so that it is no longer visible.
        /// </summary>
        public virtual void Hide() {
            this.Visible = false;
        }

        public virtual void DoUpdate(GameTime gameTime) { /* NOOP */ }

        public void Update(GameTime gameTime) {
            DoUpdate(gameTime);

            UpdateLayout();

            CheckMouseLeft();
        }

        private void CheckMouseLeft() {
            if (_mouseOver && !this.AbsoluteBounds.Contains(Input.Mouse.Position)) {
                this.MouseOver = false;
            }
        }

        /// <summary>
        /// Draw the control.
        /// </summary>
        /// <param name="bounds">The draw region of the control.  Anything outside of this region will be clipped.  If this control is the child of a container, it could potentially be clipped even further by <see cref="GraphicsDevice.ScissorRectangle" />.</param>
        protected abstract void Paint(SpriteBatch spriteBatch, Rectangle bounds);

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor) {
            var controlScissor = Rectangle.Intersect(scissor, this.AbsoluteBounds.WithPadding(_padding)).ScaleBy(Graphics.UIScaleMultiplier);

            spriteBatch.GraphicsDevice.ScissorRectangle = controlScissor;

            this.EffectBehind?.Draw(spriteBatch, drawBounds);

            spriteBatch.Begin(this.SpriteBatchParameters);
                
            // Draw background
            if (_backgroundColor != Color.Transparent)
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, drawBounds, _backgroundColor);

            if (!this.ClipsBounds) {
                spriteBatch.GraphicsDevice.ScissorRectangle = Graphics.SpriteScreen.LocalBounds.ScaleBy(Graphics.UIScaleMultiplier);
            }

            // Draw control
            Paint(spriteBatch, drawBounds);

            spriteBatch.End();

            //this.EffectInFront?.Draw(drawBounds);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void DisposeControl() { /* NOOP */ }

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // Let everything know we're disposing (esp. Data Bindings)
                    this.Disposed?.Invoke(this, EventArgs.Empty);

                    // Unassociate any subcontrols
                    this.EffectBehind  = null;
                    this.EffectInFront = null;

                    // Disconnect all existing event handlers
                    this.LeftMouseButtonPressed   = null;
                    this.LeftMouseButtonReleased  = null;
                    this.MouseMoved               = null;
                    this.RightMouseButtonPressed  = null;
                    this.RightMouseButtonReleased = null;
                    this.MouseWheelScrolled       = null;
                    this.MouseEntered             = null;
                    this.MouseLeft                = null;
                    this.Click                    = null;

                    this.Resized         = null;
                    this.Moved           = null;
                    this.Disposed        = null;
                    this.PropertyChanged = null;

                    this.Shown  = null;
                    this.Hidden = null;

                    // Cancel any animations that were currently running on this object
                    Animation.Tweener.TargetCancel(this);

                    // Remove self from parent object
                    this.Parent = null;

                    this.DisposeControl();
                }

                _disposedValue = true;
            }
        }

        public IEnumerable<Control> GetAncestors() {
            var current = this.Parent;

            while (current != null) {
                yield return current;
                current = current.Parent;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        #region Property Management and Binding

        protected bool SetProperty<T>(ref T property, T newValue, bool invalidateLayout = false, [CallerMemberName] string propertyName = null) {
            if (Equals(property, newValue) || propertyName == null) return false;

            property = newValue;

            OnPropertyChanged(propertyName, invalidateLayout);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName, bool invalidateLayout) {
            if (string.IsNullOrEmpty(propertyName)) return;

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (invalidateLayout) {
                Invalidate();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            OnPropertyChanged(propertyName, false);
        }

        #endregion

        #region Helper Classes
        private readonly struct SuspendLayoutScope : IDisposable {
            private readonly Control _owner;
            private readonly bool    _forceRecalculate;

            public SuspendLayoutScope(Control owner, bool forceRecalculate = false) {
                _owner            = owner;
                _forceRecalculate = forceRecalculate;
                _owner.SuspendLayout();
            }

            public void Dispose() {
                _owner.ResumeLayout(_forceRecalculate);
            }
        }
        #endregion
    }
}
