using System;

namespace Blish_HUD.Controls {

    public class ControlActivatedEventArgs : EventArgs {
        public Control ActivatedControl { get; }

        public ControlActivatedEventArgs(Control activatedControl) {
            this.ActivatedControl = activatedControl;
        }

    }

}
