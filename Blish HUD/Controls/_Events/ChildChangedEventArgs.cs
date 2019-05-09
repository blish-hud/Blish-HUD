using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    public class ChildChangedEventArgs:CancelEventArgs {
        public Control       ChangedChild      { get; }
        public bool          Added             { get; }
        public List<Control> ResultingChildren { get; }

        public ChildChangedEventArgs(Container sender, Control changedChild, bool adding, List<Control> resultingChildren) {
            this.ChangedChild      = changedChild;
            this.Added             = adding;
            this.ResultingChildren = resultingChildren.ToList();
        }
    }

}
