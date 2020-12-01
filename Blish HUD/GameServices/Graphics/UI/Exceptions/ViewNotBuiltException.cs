using System;

namespace Blish_HUD.Graphics.UI.Exceptions {
    public class ViewNotBuiltException : InvalidOperationException {

        public ViewNotBuiltException() : base("View must be built before this operation can take place.") { /* NOOP */ }

    }
}
