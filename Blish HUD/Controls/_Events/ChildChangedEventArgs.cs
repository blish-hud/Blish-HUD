using System.Collections.Generic;
using System.ComponentModel;

namespace Blish_HUD.Controls {

    public class ChildChangedEventArgs : CancelEventArgs {
        public Control              ChangedChild      { get; }
        public bool                 Added             { get; }
        public IEnumerable<Control> ResultingChildren { get; }

        public ChildChangedEventArgs(Container sender, Control changedChild, bool adding, IEnumerable<Control> resultingChildren) {
            this.ChangedChild      = changedChild;
            this.Added             = adding;
            this.ResultingChildren = resultingChildren;
        }
    }

}
