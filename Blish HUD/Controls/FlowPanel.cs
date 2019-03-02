using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {
    public class FlowPanel : Panel {

        public enum ControlFlowDirection {
            LeftToRight,
            TopToBottom
        }

        private int _controlPadding = 0;
        public int ControlPadding {
            get => _controlPadding;
            set {
                if (_controlPadding == value) return;

                _controlPadding = value;
                OnPropertyChanged();
            }
        }

        private ControlFlowDirection _flowDirection = ControlFlowDirection.LeftToRight;
        public ControlFlowDirection FlowDirection {
            get => _flowDirection;
            set {
                if (_flowDirection == value) return;

                _flowDirection = value;
                OnPropertyChanged();
            }
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
            List<Control> allChildren = this.Children.ToList();

            if (e.Added) {
                allChildren.Add(e.ChangedChild);
            } else {
                allChildren.Remove(e.ChangedChild);
            }

            UpdateLayout(allChildren);
        }

        private void UpdateLayout(List<Control> allChildren) {
            // TODO: Implement TopToBottom ControlFlowDirection
            if (this.FlowDirection == ControlFlowDirection.LeftToRight) {
                int nextBottom = 0;
                int currentBottom = 0;
                int lastRight = 0;

                foreach (var child in allChildren) {
                    // Need to flow over to the next line
                    if (child.Width > this.Width - lastRight) {
                        // TODO: Consider a more graceful alternative (like just stick it on its own line)
                        // Prevent stack overflow
                        if (child.Width > this.ContentRegion.Width)
                            throw new Exception("Control is too large to flow in FlowPanel");

                        currentBottom = nextBottom + this.ControlPadding;
                        lastRight = 0;
                    }

                    child.Location = new Point(lastRight, currentBottom);

                    lastRight = child.Right + this.ControlPadding;

                    // Ensure rows don't overlap
                    nextBottom = Math.Max(nextBottom, child.Bottom);
                }
            }
        }

    }
}
