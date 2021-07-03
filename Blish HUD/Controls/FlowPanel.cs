using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {
    
    public enum ControlFlowDirection {
        /// <summary>
        /// Child controls are organized left to right.
        /// When the width of the container is exceeded,
        /// the remaining children are brought to the next
        /// row to continue to be organized.
        /// </summary>
        LeftToRight,

        /// <summary>
        /// Child controls are organized right to left.
        /// When the width of the container is exceeded,
        /// the remaining children are brought to the next
        /// row to continue to be organized.
        /// </summary>
        RightToLeft,

        /// <summary>
        /// Child controls are organized top to bottom.
        /// When the height of the container is exceeded,
        /// the remaining children are brought to the next
        /// column to continue to be organized.
        /// </summary>
        TopToBottom,

        /// <summary>
        /// Child controls are organized bottom to top.
        /// When the height of the container is exceeded,
        /// the remaining children are brought to the next
        /// column to continue to be organized.
        /// </summary>
        BottomToTop,

        /// <summary>
        /// Child controls are organized left to right.
        /// They will be organized into a single row
        /// regardless of the horizontal space available.
        /// </summary>
        SingleLeftToRight,

        /// <summary>
        /// Child controls are organized right to left.
        /// They will be organized into a single row
        /// regardless of the horizontal space available.
        /// </summary>
        SingleRightToLeft,

        /// <summary>
        /// Child controls are organized top to bottom.
        /// They will be organized into a single column
        /// regardless of the vertical space available.
        /// </summary>
        SingleTopToBottom,

        /// <summary>
        /// Child controls are organized bottom to top.
        /// They will be organized into a single column
        /// regardless of the vertical space available.
        /// </summary>
        SingleBottomToTop,
    }

    public class FlowPanel : Panel {
        
        protected Vector2 _controlPadding = Vector2.Zero;
        public Vector2 ControlPadding {
            get => _controlPadding;
            set => SetProperty(ref _controlPadding, value, true);
        }

        protected Vector2 _outerControlPadding = Vector2.Zero;
        public Vector2 OuterControlPadding {
            get => _outerControlPadding;
            set => SetProperty(ref _outerControlPadding, value, true);
        }

        protected bool _padLeftBeforeControl = false;
        [Obsolete("Use OuterControlPadding instead.")]
        public bool PadLeftBeforeControl {
            get => _padLeftBeforeControl;
            set => SetProperty(ref _padLeftBeforeControl, value, true);
        }

        protected bool _padTopBeforeControl = false;
        [Obsolete("Use OuterControlPadding instead.")]
        public bool PadTopBeforeControl {
            get => _padTopBeforeControl;
            set => SetProperty(ref _padTopBeforeControl, value, true);
        }

        protected ControlFlowDirection _flowDirection = ControlFlowDirection.LeftToRight;

        /// <summary>
        /// The method / direction that should be used when flowing controls.
        /// </summary>
        public ControlFlowDirection FlowDirection {
            get => _flowDirection;
            set => SetProperty(ref _flowDirection, value, true);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);
            OnChildrenChanged(e);

            e.ChangedChild.Resized += ChangedChildOnResized;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);
            OnChildrenChanged(e);

            e.ChangedChild.Resized -= ChangedChildOnResized;
        }

        private void ChangedChildOnResized(object sender, ResizedEventArgs e) {
            ReflowChildLayout(_children);
        }

        private void OnChildrenChanged(ChildChangedEventArgs e) {
            ReflowChildLayout(e.ResultingChildren);
        }

        public override void RecalculateLayout() {
            ReflowChildLayout(_children);

            base.RecalculateLayout();
        }

        /// <summary>
        /// Filters children of the flow panel by setting those
        /// that don't match the provided filter function to be
        /// not visible.
        /// </summary>
        public void FilterChildren<TControl>(Func<TControl, bool> filter) where TControl : Control {
            _children.Cast<TControl>().ToList().ForEach(tc => tc.Visible = filter(tc));
            ReflowChildLayout(_children);
        }

        /// <summary>
        /// Sorts children of the flow panel using the provided
        /// comparison function.
        /// </summary>
        /// <typeparam name="TControl"></typeparam>
        /// <param name="comparison"></param>
        public void SortChildren<TControl>(Comparison<TControl> comparison) where TControl : Control {
            var tempChildren = _children.Cast<TControl>().ToList();
            tempChildren.Sort(comparison);

            _children = tempChildren.Cast<Control>().ToList();

            ReflowChildLayout(_children);
        }

        private void ReflowChildLayoutLeftToRight(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastRight = outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next row
                if (child.Width >= this.Width - lastRight) {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastRight = outerPadX;
                }

                child.Location = new Point((int) lastRight, (int) currentBottom);

                lastRight = child.Right + _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutRightToLeft(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastLeft = this.Width - outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next row
                if (outerPadX > lastLeft - child.Width) {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastLeft = this.Width - outerPadX;
                }

                child.Location = new Point((int) (lastLeft - child.Width), (int) currentBottom);

                lastLeft = child.Left - _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastBottom = outerPadY;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next column
                if (child.Height >= this.Height - lastBottom) {
                    currentRight = nextRight + _controlPadding.X;
                    lastBottom = outerPadY;
                }

                child.Location = new Point((int) currentRight, (int) lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutBottomToTop(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastTop = this.Height - outerPadY;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next column
                if (outerPadY > lastTop - child.Height) {
                    currentRight = nextRight + _controlPadding.X;
                    lastTop = this.Height - outerPadY;
                }

                child.Location = new Point((int) currentRight, (int) (lastTop - child.Height));

                lastTop = child.Top - _controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutSingleLeftToRight(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            var lastLeft = outerPadX;

            foreach (var child in allChildren) {
                child.Location = new Point((int) lastLeft, (int) outerPadY);

                lastLeft = child.Right + _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleRightToLeft(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            var lastLeft = this.Width - outerPadX;

            foreach (var child in allChildren) {
                child.Location = new Point((int) (lastLeft - child.Width), (int) outerPadY);

                lastLeft = child.Left - _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleTopToBottom(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            var lastBottom = outerPadY;

            foreach (var child in allChildren) {
                child.Location = new Point((int) outerPadX, (int) lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;
            }
        }

        private void ReflowChildLayoutSingleBottomToTop(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            var lastTop = this.Height - outerPadY;

            foreach (var child in allChildren) {
                child.Location = new Point((int) outerPadX, (int) (lastTop - child.Height));

                lastTop = child.Top - _controlPadding.Y;
            }
        }

        private void ReflowChildLayout(List<Control> allChildren) {
            var filteredChildren = allChildren.ToList().Where(c => c.GetType() != typeof(Scrollbar)
                                                                && c.Visible);

            switch (_flowDirection) {
                case ControlFlowDirection.LeftToRight:
                    ReflowChildLayoutLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.RightToLeft:
                    ReflowChildLayoutRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.TopToBottom:
                    ReflowChildLayoutTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.BottomToTop:
                    ReflowChildLayoutBottomToTop(filteredChildren);
                    break;
                case ControlFlowDirection.SingleLeftToRight:
                    ReflowChildLayoutSingleLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.SingleRightToLeft:
                    ReflowChildLayoutSingleRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.SingleTopToBottom:
                    ReflowChildLayoutSingleTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.SingleBottomToTop:
                    ReflowChildLayoutSingleBottomToTop(filteredChildren);
                    break;
            }
        }
    }
}
