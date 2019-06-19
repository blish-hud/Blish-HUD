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

        protected Vector2 _controlPadding = Vector2.Zero;
        public Vector2 ControlPadding {
            get => _controlPadding;
            set => SetProperty(ref _controlPadding, value, true);
        }

        protected bool _padLeftBeforeControl = false;
        public bool PadLeftBeforeControl {
            get => _padLeftBeforeControl;
            set => SetProperty(ref _padLeftBeforeControl, value, true);
        }

        protected bool _padTopBeforeControl = false;
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
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);
            OnChildrenChanged(e);
        }

        private void OnChildrenChanged(ChildChangedEventArgs e) {
            ReflowChildLayout(e.ResultingChildren);
        }

        public override void RecalculateLayout() {
            ReflowChildLayout(_children);
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

        private void ReflowChildLayoutLeftToRight(List<Control> allChildren) {
            float nextBottom    = _padTopBeforeControl ? _controlPadding.Y : 0;
            float currentBottom = 0;
            float lastRight     = _padLeftBeforeControl ? _controlPadding.X : 0;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                // Need to flow over to the next line
                if (child.Width >= this.Width - lastRight) {
                    // TODO: Consider a more graceful alternative (like just stick it on its own line)
                    // Prevent stack overflow
                    if (child.Width > this.ContentRegion.Width)
                        throw new Exception("Control is too large to flow in FlowPanel");

                    currentBottom = nextBottom + _controlPadding.Y;
                    lastRight     = _padLeftBeforeControl ? _controlPadding.X : 0;
                }

                child.Location = new Point((int)lastRight, (int)currentBottom);

                lastRight = child.Right + _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(List<Control> allChildren) {
            // TODO: Implement FlowPanel FlowDirection.TopToBottom
        }

        private void ReflowChildLayout(List<Control> allChildren) {
            if (this.FlowDirection == ControlFlowDirection.LeftToRight) {
                ReflowChildLayoutLeftToRight(allChildren.Where(c => c.GetType() != typeof(Scrollbar)).ToList());
            } else {
                ReflowChildLayoutTopToBottom(allChildren.Where(c => c.GetType() != typeof(Scrollbar)).ToList());
            }
        }

    }
}
