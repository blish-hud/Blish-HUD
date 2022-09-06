using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {

    // TODO: Ensure that container objects, when disposed, first dispose of their children

    /// <summary>
    /// A control that is capable of having child controls that are drawn when the container is drawn.
    /// Classes that inherit should be packaged controls that that manage their own controls internally.
    /// </summary>
    public abstract class Container : Control, IEnumerable<Control> {

        public event EventHandler<ChildChangedEventArgs> ChildAdded;
        public event EventHandler<ChildChangedEventArgs> ChildRemoved;

        public event EventHandler<RegionChangedEventArgs> ContentResized;

        protected ControlCollection<Control> _children;

        [Newtonsoft.Json.JsonIgnore]
        public ControlCollection<Control> Children => _children;

        protected Container() {
            _children = new ControlCollection<Control>();
        }

        protected virtual void OnChildAdded(ChildChangedEventArgs e) {
            this.ChildAdded?.Invoke(this, e);
        }

        protected virtual void OnChildRemoved(ChildChangedEventArgs e) {
            this.ChildRemoved?.Invoke(this, e);
        }

        protected virtual void OnContentResized(RegionChangedEventArgs e) {
            this.ContentResized?.Invoke(this, e);
        }

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);

            /* ContentRegion defaults to match our control size until one is manually set,
               so we do squeeze in OnPropertyChanged for it if the control hasn't had a
               ContentRegion specified and then resizes */
            if (!_contentRegion.HasValue) {
                OnPropertyChanged(nameof(this.ContentRegion));

                OnContentResized(new RegionChangedEventArgs(new Rectangle(Point.Zero, e.PreviousSize),
                                                            new Rectangle(Point.Zero, e.CurrentSize)));
            }
        }

        protected Rectangle? _contentRegion;
        public Rectangle ContentRegion {
            get => _contentRegion ?? new Rectangle(Point.Zero, this.Size);
            protected set {
                var previousRegion = this.ContentRegion;

                if (SetProperty(ref _contentRegion, value, true)) {
                    OnContentResized(new RegionChangedEventArgs(previousRegion, this.ContentRegion));
                }
            }
        }

        protected Point _contentBounds = Point.Zero;
        public Point ContentBounds => _contentBounds;
        
        private int _verticalScrollOffset;
        public int VerticalScrollOffset {
            get => _verticalScrollOffset;
            set => SetProperty(ref _verticalScrollOffset, value);
        }

        private int _horizontalScrollOffset;
        public int HorizontalScrollOffset {
            get => _horizontalScrollOffset;
            set => SetProperty(ref _horizontalScrollOffset, value);
        }

        private SizingMode _widthSizingMode = SizingMode.Standard;

        /// <summary>
        /// Determines how the width of this
        /// container should be handled.
        /// </summary>
        public virtual SizingMode WidthSizingMode {
            get => _widthSizingMode;
            set => SetProperty(ref _widthSizingMode, value);
        }

        private SizingMode _heightSizingMode = SizingMode.Standard;

        /// <summary>
        /// Determines how the height of this
        /// container should be handled.
        /// </summary>
        public virtual SizingMode HeightSizingMode {
            get => _heightSizingMode;
            set => SetProperty(ref _heightSizingMode, value);
        }
        
        private Point _autoSizePadding = Point.Zero;

        /// <summary>
        /// If <see cref="HeightSizingMode"/> or <see cref="WidthSizingMode"/> is set to
        /// <see cref="SizingMode.AutoSize"/>, then <see cref="AutoSizePadding"/> is
        /// added to the size.
        /// </summary>
        public Point AutoSizePadding {
            get => _autoSizePadding;
            set => SetProperty(ref _autoSizePadding, value);
        }

        protected Scrollbar _panelScrollbar;

        protected Vector2 _scrollPadding = new Vector2(4, 0);
        /// <summary>
        /// Padding on the left side of the <see cref="Scrollbar"/>.
        /// </summary>
        public Vector2 ScrollPadding {
            get => _scrollPadding;
            set => SetProperty(ref _scrollPadding, value, true);
        }

        /// <summary>
        /// Indicates whether or not there is a <see cref="Scrollbar"/>  drawn inside the <see cref="Panel"/>.
        /// </summary>
        public bool ScrollbarVisible {
            //get => _panelScrollbar != null && _panelScrollbar.Drawn;
            //get => _panelScrollbar != null;
            get => _scrollbarVisible;
            protected set => SetProperty(ref _scrollbarVisible, value, true);
        }
        protected bool _scrollbarVisible = false;

        /// <summary>
        /// Space which is taken from the <see cref="Scrollbar"/> to be drawn inside the <see cref="Panel"/>.
        /// </summary>
        public int ScrollbarWidth {
            get => _panelScrollbar != null ? (_panelScrollbar.ScrollbarWidth + (int) ScrollPadding.X) : 0;
        }

        protected Point _maxSize = Point.Zero;
        /// <summary>
        /// Maximum Size the <see cref="Container"/> can autoresize to.
        /// </summary>
        public Point MaxSize {
            get => _maxSize;
            protected set => SetProperty(ref _maxSize, value, true);
        }

        /// <summary>
        /// Maximum Width the <see cref="Container"/> can autoresize to.
        /// </summary>
        public int MaxWidth {
            get => _maxSize.X;
            protected set => SetProperty(ref _maxSize.X, value, true);
        }

        /// <summary>
        /// Maximum Height the <see cref="Container"/> can autoresize to.
        /// </summary>
        public int MaxHeight{
            get => _maxSize.Y;
            protected set => SetProperty(ref _maxSize.Y, value, true);
        }

        protected override CaptureType CapturesInput() => CaptureType.Mouse | CaptureType.MouseWheel;

        public IEnumerable<Control> GetDescendants() {
            // Breadth-first unrolling without the inefficiency of recursion.
            var remainingChildren = new Queue<Control>(this.Children.ToArray());

            while (remainingChildren.Count > 0) {
                var child = remainingChildren.Dequeue();
                yield return child;

                if (child is Container container) {
                    foreach (var containerChild in container) {
                        remainingChildren.Enqueue(containerChild);
                    }
                }
            }
        }

        public IEnumerable<T> GetChildrenOfType<T>() {
            foreach (var child in this.Children.ToArray()) {
                if (child is T tChild) {
                    yield return tChild;
                }
            }
        }

        public bool AddChild(Control child) {
            if (_children.Contains(child)) return true;
            if (_panelScrollbar != null && child == _panelScrollbar) return true;

            var resultingChildren = _children.ToList();
            resultingChildren.Add(child);

            var evRes = new ChildChangedEventArgs(this, child, true, resultingChildren);
            OnChildAdded(evRes);

            if (evRes.Cancel) return false;

            _children.Add(child);

            Invalidate();

            return true;
        }

        public bool RemoveChild(Control child) {
            if (!_children.Contains(child)) return true;

            var resultingChildren = _children.ToList();
            resultingChildren.Remove(child);

            var evRes = new ChildChangedEventArgs(this, child, false, resultingChildren);
            OnChildRemoved(evRes);

            // TODO: Currently if a child removal is canceled, the child control will still set their parent to null, despite still being listed as a child here
            if (evRes.Cancel) return false;

            _children.Remove(child);

            Invalidate();

            return true;
        }

        public void ClearChildren() {
            while (_children.Count > 0) {
                _children[0].Parent = null;
            }
        }

        public override Control TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
            Control thisResult  = null;
            Control childResult = null;

            if (CapturesInput() != CaptureType.None) {
                thisResult = base.TriggerMouseInput(mouseEventType, ms);
            }

            List<Control>               children        = _children.ToList();
            IOrderedEnumerable<Control> zSortedChildren = children.OrderByDescending(i => i.ZIndex).ThenByDescending(c => children.IndexOf(c));

            foreach (var childControl in zSortedChildren) {
                if (childControl.AbsoluteBounds.Contains(ms.Position) && childControl.Visible) {
                    childResult = childControl.TriggerMouseInput(mouseEventType, ms);

                    if (childResult != null) {
                        if (!childResult.Captures.HasFlag(CaptureType.Filter)) {
                            break;
                        }

                        // Child has Filter flag so we have to pretend we didn't see it
                        childResult = null;
                    }
                }
            }

            return childResult ?? thisResult;
        }

        public virtual void UpdateContainer(GameTime gameTime) { /* NOOP */ }

        private int GetUpdatedSizing(SizingMode sizingMode, int currentSize, int maxSize, int fillSize) {
            switch (sizingMode) {
                default:
                case SizingMode.Standard:
                    return currentSize;
                case SizingMode.AutoSize:
                    return maxSize;
                case SizingMode.Fill:
                    return fillSize;
            }
        }

        public sealed override void DoUpdate(GameTime gameTime) {
            UpdateContainer(gameTime);

            Control[] children = _children.ToArray();
            var filteredChildren = children.Where(c => c.GetType() != typeof(Scrollbar) && c.Visible).ToArray();

            _contentBounds = ControlUtil.GetControlBounds(filteredChildren);

            var limitSize = _maxSize != Point.Zero;

            // Update our size based on the sizing mode
            var parent = this.Parent;
            if (parent != null) {
                this.Size = new Point(GetUpdatedSizing(this.WidthSizingMode,
                                                      limitSize ? Math.Min(_maxSize.X, this.Width) : this.Width,
                                                      limitSize ? Math.Min(_maxSize.X, _contentBounds.X + (this.Width - this.ContentRegion.Width) + _autoSizePadding.X) : _contentBounds.X           + (this.Width - this.ContentRegion.Width) + _autoSizePadding.X,
                                                     limitSize ? Math.Min(_maxSize.X, parent.ContentRegion.Width - this.Left) : parent.ContentRegion.Width - this.Left),
                                      GetUpdatedSizing(this.HeightSizingMode,
                                                      limitSize ? Math.Min(_maxSize.Y, this.Height) : this.Height,
                                                      limitSize ? Math.Min(_maxSize.Y, _contentBounds.Y + (this.Height - this.ContentRegion.Height) + _autoSizePadding.Y) : _contentBounds.Y            + (this.Height - this.ContentRegion.Height) + _autoSizePadding.Y,
                                                      limitSize ? Math.Min(_maxSize.Y, parent.ContentRegion.Height - this.Top) : parent.ContentRegion.Height - this.Top));

                ScrollbarVisible = _contentBounds.Y > this.Height;
            }

            // Update our children
            foreach (var childControl in children) {
                // Update child if it is visible or if it hasn't rendered yet (needs a first time calc)
                if (childControl.Visible || childControl.LayoutState != LayoutState.Ready) {
                    childControl.Update(gameTime);
                }
            }
        }

        protected sealed override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var controlScissor = spriteBatch.GraphicsDevice.ScissorRectangle.ScaleBy(1 / Graphics.UIScaleMultiplier);

            // Draw container background
            PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.End();

            PaintChildren(spriteBatch, bounds, controlScissor);

            // Restore scissor
            spriteBatch.GraphicsDevice.ScissorRectangle = controlScissor.ScaleBy(Graphics.UIScaleMultiplier);

            spriteBatch.Begin(this.SpriteBatchParameters);

            PaintAfterChildren(spriteBatch, bounds);
        }

        public virtual void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) { /* NOOP */ }

        protected void PaintChildren(SpriteBatch spriteBatch, Rectangle bounds, Rectangle scissor) {
            var cR = ContentRegion.ToBounds(this.AbsoluteBounds);
            var contentScissor = Rectangle.Intersect(scissor, cR);

            var zSortedChildren = _children.ToArray().OrderBy(i => i.ZIndex);

            // Render each visible child
            foreach (var childControl in zSortedChildren) {
                if (childControl.Visible && childControl.LayoutState != LayoutState.SkipDraw) {
                    var childBounds = new Rectangle(Point.Zero, childControl.Size);

                    if (childControl.AbsoluteBounds.Intersects(contentScissor) || !childControl.ClipsBounds) {
                        childControl.Draw(spriteBatch, childBounds, contentScissor);
                    }
                }
            }
        }

        public virtual void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) { /* NOOP */ }

        protected override void DisposeControl() {
            foreach (var descendant in GetDescendants()) {
                descendant.Dispose();
            }

            _panelScrollbar?.Dispose();

            base.DisposeControl();
        }

        #region IEnumerable Implementation

        public IEnumerator<Control> GetEnumerator() {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}