using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Presenters {

    /// <summary>
    /// A presenter that does nothing.  Can be used when testing views or when the view
    /// is capable of managing its own state without needing to update an associated model.
    /// </summary>
    public class NullPresenter : IPresenter {

        /// <inheritdoc />
        public Task<bool> DoLoad(IProgress<string> progress) {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void DoUpdateView() { /* NOOP */ }

        /// <inheritdoc />
        public void DoUnload() { /* NOOP */ }

    }
}
