using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Controls {
    public interface IViewContainer {

        /// <summary>
        /// The current state of the view.
        /// </summary>
        ViewState ViewState { get; }

        /// <summary>
        /// The <see cref="IView"/> this container is currently displaying.
        /// </summary>
        IView CurrentView { get; }

    }
}
