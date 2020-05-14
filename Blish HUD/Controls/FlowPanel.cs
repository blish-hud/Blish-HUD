using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {

    public enum ControlFlowDirection {

        LeftToRight,
        TopToBottom

    }

    public class FlowPanel : Panel {

        public Vector2 ControlPaddingInBetween {
            get => _controlPaddingInBetween;
            set => SetProperty(ref _controlPaddingInBetween, value, true);
        }
        public Vector2 ControlPaddingOuterBounds {
            get => _controlPaddingOuterBounds;
            set => SetProperty(ref _controlPaddingOuterBounds, value, true);
        }
        public ControlFlowDirection FlowDirection {
            get => _flowDirection;
            set => SetProperty(ref _flowDirection, value, true);
        }

        protected Vector2 _controlPaddingInBetween = Vector2.Zero;

        protected Vector2 _controlPaddingOuterBounds = Vector2.Zero;

        protected ControlFlowDirection _flowDirection = ControlFlowDirection.LeftToRight;

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
            List<TControl> tempChildren = _children.Cast<TControl>().ToList();
            tempChildren.Sort(comparison);

            _children = tempChildren.Cast<Control>().ToList();

            ReflowChildLayout(_children);
        }

        private void ReflowChildLayoutLeftToRight(List<Control> allChildren) {
            float nextBottom    = _controlPaddingOuterBounds.Y;
            float currentBottom = _controlPaddingOuterBounds.Y;
            float lastRight     = _controlPaddingOuterBounds.X;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next line
                if (child.Width >= this.Width - lastRight) {
                    // TODO: Consider a more graceful alternative (like just stick it on its own line)
                    // Prevent stack overflow
                    if (child.Width > this.ContentRegion.Width) throw new Exception("Control is too large to flow in FlowPanel");

                    currentBottom = nextBottom + _controlPaddingInBetween.Y;
                    lastRight     = _controlPaddingOuterBounds.X;
                }

                child.Location = new Point((int) lastRight, (int) currentBottom);

                lastRight = child.Right + _controlPaddingInBetween.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(List<Control> allChildren) {
            // TODO: Implement FlowPanel FlowDirection.TopToBottom
        }

        private void ReflowChildLayout(List<Control> allChildren) {
            if (this.FlowDirection == ControlFlowDirection.LeftToRight)
                ReflowChildLayoutLeftToRight(allChildren.Where(c => c.GetType() != typeof(Scrollbar)).ToList());
            else
                ReflowChildLayoutTopToBottom(allChildren.Where(c => c.GetType() != typeof(Scrollbar)).ToList());
        }

    }

}