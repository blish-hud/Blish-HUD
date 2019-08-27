using System;
using System.Threading.Tasks;

namespace Blish_HUD.Graphics.UI {
    /// <summary>
    /// Implements the presenter logic used to update an <see cref="IView"/> based on a model.
    /// </summary>
    public interface IPresenter {

        /// <summary>
        /// The time to load anything that will be needed for the <see cref="IPresenter"/>.
        /// This runs just before the <see cref="IView"/> has loaded.
        /// </summary>
        /// <param name="progress">If reported to, will show the loading status in the <see cref="Controls.ViewContainer"/>.</param>
        /// <returns>A <c>bool</c> indicating if the <see cref="IPresenter"/> loaded successfully or not.</returns>
        Task<bool> DoLoad(IProgress<string> progress);

        /// <summary>
        /// Runs after the <see cref="IView"/> has been built for the first time.
        /// This is a good time to update the view to match the model state.
        /// </summary>
        void DoUpdateView();

        /// <summary>
        /// Unload any resources that need to be manually unloaded
        /// as this <see cref="IPresenter"/> will no longer be used.
        /// </summary>
        void DoUnload();

    }
}
