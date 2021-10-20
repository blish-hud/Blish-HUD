namespace Blish_HUD.Graphics.UI {
    public abstract class View : View<IPresenter> {

        private static readonly NullPresenter _sharedNullPresenter = new NullPresenter();

        protected View() {
            this.Presenter = _sharedNullPresenter;
        }

        protected View(IPresenter presenter) {
            Presenter = presenter ?? _sharedNullPresenter;
        }

    }
}
