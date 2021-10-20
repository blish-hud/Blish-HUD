using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Graphics.UI {
    public abstract class View<TPresenter> : IView where TPresenter : IPresenter {

        public event EventHandler<EventArgs> Loaded;

        public event EventHandler<EventArgs> Built;

        public event EventHandler<EventArgs> Unloaded;

        private TPresenter _presenter;
        public TPresenter Presenter {
            get => _presenter;
            protected set {
                _presenter = value;
                OnPresenterAssigned(value);
            }
        }

        protected Container ViewTarget { get; private set; }

        protected View() { /* NOOP */ }

        public View<TPresenter> WithPresenter(TPresenter presenter) {
            this.Presenter = presenter
                          ?? throw new ArgumentNullException(nameof(presenter));

            return this;
        }

        protected virtual void OnPresenterAssigned(TPresenter presenter) { /* NOOP */ }

        public async Task<bool> DoLoad(IProgress<string> progress) {
            bool loadResult = await Presenter.DoLoad(progress)
                           && await Load(progress);

            if (loadResult) {
                this.Loaded?.Invoke(this, EventArgs.Empty);
            }

            return loadResult;
        }

        public void DoBuild(Container buildPanel) {
            this.ViewTarget = buildPanel;

            Build(buildPanel);

            this.Built?.Invoke(this, EventArgs.Empty);

            Presenter.DoUpdateView();
        }

        public void DoUnload() {
            Presenter.DoUnload();
            Unload();

            this.Unloaded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual async Task<bool> Load(IProgress<string> progress) {
            return await Task.FromResult(true);
        }

        protected virtual void Build(Container buildPanel) { /* NOOP */ }

        protected virtual void Unload() { /* NOOP */ }

    }
}
