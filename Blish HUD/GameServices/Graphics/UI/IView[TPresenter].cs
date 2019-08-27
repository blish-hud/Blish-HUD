namespace Blish_HUD.Graphics.UI {
    /// <inheritdoc cref="IView"/>
    /// <typeparam name="TPresenter">The type of <see cref="IPresenter"/> used to manage this <see cref="IView"/>.</typeparam>
    public interface IView<TPresenter> : IView where TPresenter : class, IPresenter {

        /// <summary>
        /// The <see cref="IPresenter"/> used to update this <see cref="IView{TPresenter}"/>.
        /// </summary>
        TPresenter Presenter { get; set; }

    }
}
