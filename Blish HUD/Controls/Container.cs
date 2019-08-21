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

        protected readonly List<Control> _children;
        [Newtonsoft.Json.JsonIgnore]
        public IReadOnlyCollection<Control> Children => _children.AsReadOnly();

        protected Container() {
            _children = new List<Control>();
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
            }
        }

        public bool AddChild(Control child) {
            if (_children.Contains(child)) return true;

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

        public List<Control> GetDescendants() {
            var allDescendants = _children.ToList();

            foreach (var child in _children) {
                if (!(child is Container container)) continue;

                allDescendants.AddRange(container.GetDescendants());
            }

            return allDescendants;
        }

        public override Control TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
            List<Control> zSortedChildren = _children.OrderByDescending(i => i.ZIndex).ThenByDescending(c => _children.IndexOf(c)).ToList();

            Control thisResult  = null;
            Control childResult = null;

            if (CapturesInput() != CaptureType.None) {
                thisResult = base.TriggerMouseInput(mouseEventType, ms);
            }

            foreach (var childControl in zSortedChildren) {
                if (childControl.AbsoluteBounds.Contains(ms.Position) && childControl.Visible) {
                    childResult = childControl.TriggerMouseInput(mouseEventType, ms);

                    if (childResult != null) {
                        break;
                    }
                }
            }

            return childResult ?? thisResult;
        }

        public virtual void UpdateContainer(GameTime gameTime) { /* NOOP */ }

        public sealed override void DoUpdate(GameTime gameTime) {
            UpdateContainer(gameTime);

            // Update our children
            foreach (var childControl in _children.ToList()) {
                // Update child if it is visible or if it hasn't rendered yet (needs a first time calc)
                if (childControl.Visible || childControl.LayoutState != LayoutState.Ready)
                    childControl.Update(gameTime);
            }
        }

        protected sealed override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var controlScissor = Graphics.GraphicsDevice.ScissorRectangle.ScaleBy(1 / Graphics.UIScaleMultiplier);

            // Draw container background
            PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.End();

            PaintChildren(spriteBatch, bounds, controlScissor);

            // Restore scissor
            Graphics.GraphicsDevice.ScissorRectangle = controlScissor.ScaleBy(Graphics.UIScaleMultiplier);

            spriteBatch.Begin(this.SpriteBatchParameters);

            PaintAfterChildren(spriteBatch, bounds);
        }

        public virtual void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) { /* NOOP */ }

        protected void PaintChildren(SpriteBatch spriteBatch, Rectangle bounds, Rectangle scissor) {
            var contentScissor = Rectangle.Intersect(scissor, ContentRegion.ToBounds(this.AbsoluteBounds));

            List<Control> zSortedChildren = _children.OrderBy(i => i.ZIndex).ToList();

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

        #region IEnumerable Implementation

        public IEnumerator<Control> GetEnumerator() {
            return this.Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}