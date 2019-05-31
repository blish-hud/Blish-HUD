using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    public class ControlChangedEventArgs : EventArgs {
        public Control ActivatedControl { get; }

        public ControlChangedEventArgs(Control activatedControl) {
            this.ActivatedControl = activatedControl;
        }

    }

}
