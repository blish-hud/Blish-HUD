using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Graphics.UI {
    /// <summary>
    /// Implements a view which will display some kind of UI.
    /// </summary>
    public interface IView {

        /// <summary>
        /// Occurs once the <see cref="IView"/> and the <see cref="IPresenter"/> have loaded.
        /// </summary>
        event EventHandler<EventArgs> Loaded;

        /// <summary>
        /// Occurs once the <see cref="IView"/> has built its UI.
        /// </summary>
        event EventHandler<EventArgs> Built;

        /// <summary>
        /// Occurs once the <see cref="IView"/> and the <see cref="IPresenter"/> have unloaded.
        /// </summary>
        event EventHandler<EventArgs> Unloaded;

        /// <summary>
        /// The time to load anything that will be needed for the <see cref="IView"/>.
        /// This runs just after the <see cref="IPresenter"/> has loaded.
        /// </summary>
        /// <param name="progress">If reported to, will show the loading status in the <see cref="Controls.ViewContainer"/>.</param>
        /// <returns>A <c>bool</c> indicating if the <see cref="IView"/> loaded successfully or not.</returns>
        Task<bool> DoLoad(IProgress<string> progress);

        /// <summary>
        /// Builds out the UI components utilized by this <see cref="IView"/>.
        /// </summary>
        /// <param name="buildPanel">The destination <see cref="Container"/> (<see cref="IViewContainer"/>) this <see cref="IView"/> will be shown in.</param>
        void DoBuild(Container buildPanel);

        /// <summary>
        /// Unload any resources that need to be manually unloaded
        /// as this <see cref="IView"/> will no longer be used.
        /// </summary>
        void DoUnload();

    }
}
