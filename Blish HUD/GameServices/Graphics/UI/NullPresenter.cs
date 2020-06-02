using System;
using System.Threading.Tasks;

namespace Blish_HUD.Graphics.UI {

    /// <summary>
    /// A presenter which does not manage a model or the view.
    /// </summary>
    public sealed class NullPresenter : IPresenter {

        public Task<bool> DoLoad(IProgress<string> progress) {
            return Task.FromResult(true);
        }

        public void DoUpdateView() { /* NOOP */ }

        public void DoUnload() { /* NOOP */ }

    }

}
