namespace Blish_HUD.Graphics.UI {
    public abstract class View : View<IPresenter> {

        private static readonly NullPresenter _sharedNullPresenter = new NullPresenter();

        protected View() {
            this.Presenter = _sharedNullPresenter;
        }

        protected View(IPresenter presenter) {
            Presenter = presenter ?? _sharedNullPresenter;
        }

        // BREAKME: Avoids a breaking change, but is not necessary for anything recompiled.
        public new View WithPresenter(IPresenter presenter) {
            return base.WithPresenter(presenter) as View;
        }

    }
}
