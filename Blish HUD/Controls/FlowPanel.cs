using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {

    public enum ControlFlowDirection {
        LeftToRight,
        TopToBottom,
        SingleLeftToRight,
        SingleTopToBottom
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

        public void FilterChildren<TControl>(Func<TControl, bool> filter) where TControl : Control {
            _children.Cast<TControl>().ToList().ForEach(tc => tc.Visible = filter(tc));
            ReflowChildLayout(_children);
        }

        public void SortChildren<TControl>(Comparison<TControl> comparison) where TControl : Control {
            var tempChildren = _children.Cast<TControl>().ToList();
            tempChildren.Sort(comparison);

            _children = tempChildren.Cast<Control>().ToList();

            ReflowChildLayout(_children);
        }

        private void ReflowChildLayoutLeftToRight(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            float nextBottom    = outerPadY;
            float currentBottom = outerPadY;
            float lastRight     = outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next line
                if (child.Width >= this.Width - lastRight) {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastRight     = outerPadX;
                }

                child.Location = new Point((int)lastRight, (int)currentBottom);

                lastRight = child.Right + _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(IEnumerable<Control> allChildren) {
            // TODO: Implement FlowPanel Flow TopToBottom
        }

        private void ReflowChildLayoutSingleLeftToRight(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            var lastLeft = outerPadX;

            foreach (var child in allChildren) {
                child.Location = new Point((int)lastLeft, (int)outerPadY);

                lastLeft = child.Right + _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleTopToBottom(IEnumerable<Control> allChildren) {
            float outerPadX = _padLeftBeforeControl ? _controlPadding.X : _outerControlPadding.X;
            float outerPadY = _padTopBeforeControl ? _controlPadding.Y : _outerControlPadding.Y;

            var lastBottom = outerPadY;

            foreach (var child in allChildren) {
                child.Location = new Point((int)outerPadX, (int)lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;
            }
        }

        private void ReflowChildLayout(List<Control> allChildren) {
            var filteredChildren = allChildren.ToList().Where(c => c.GetType() != typeof(Scrollbar)
                                                                && c.Visible);

            switch (_flowDirection) {
                case ControlFlowDirection.LeftToRight:
                    ReflowChildLayoutLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.TopToBottom:
                    ReflowChildLayoutTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.SingleLeftToRight:
                    ReflowChildLayoutSingleLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.SingleTopToBottom:
                    ReflowChildLayoutSingleTopToBottom(filteredChildren);
                    break;
            }
        }

    }
}
