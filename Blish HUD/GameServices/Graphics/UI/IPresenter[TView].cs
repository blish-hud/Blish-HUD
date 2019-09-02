namespace Blish_HUD.Graphics.UI {
    /// <inheritdoc cref="IPresenter"/>
    /// <typeparam name="TView">The type of <see cref="IView"/> that will be presented to.</typeparam>
    public interface IPresenter<out TView> : IPresenter where TView : class, IView {

        /// <summary>
        /// The <see cref="IView"/> this <see cref="IPresenter{TView}"/> will be presenting to.
        /// </summary>
        TView View { get; }

    }
}
