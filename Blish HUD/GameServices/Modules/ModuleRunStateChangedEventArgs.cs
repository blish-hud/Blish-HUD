using System;

namespace Blish_HUD.Modules {

    public class ModuleRunStateChangedEventArgs : EventArgs {

        public ModuleRunState RunState { get; }

        public ModuleRunStateChangedEventArgs(ModuleRunState runState) {
            this.RunState = runState;
        }

    }

}
