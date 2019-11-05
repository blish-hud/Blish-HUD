using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Graphics.UI {
    /// <inheritdoc cref="IView{TPresenter}"/>
    /// <typeparam name="TPresenter">The type of <see cref="IPresenter"/> used to manage this view.</typeparam>
    public abstract class View<TPresenter> : IView<TPresenter> where TPresenter : class, IPresenter {

        /// <inheritdoc />
        public event EventHandler<EventArgs> Loaded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Built;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Unloaded;

        private TPresenter _presenter;

        /// <inheritdoc cref="IView{TPresenter}.Presenter"/>
        public TPresenter Presenter {
            get => _presenter;
            set => _presenter = value;
        }

        protected Panel ViewTarget { get; private set; }

        protected View(TPresenter presenter) {
            _presenter = presenter;
        }

        protected View() { /* NOOP */ }

        /// <inheritdoc />
        public async Task<bool> DoLoad(IProgress<string> progress) {
            bool loadResult = await _presenter.DoLoad(progress) && await Load(progress);

            if (loadResult) {
                this.Loaded?.Invoke(this, EventArgs.Empty);
            }

            return loadResult;
        }

        /// <inheritdoc />
        public void DoBuild(Panel buildPanel) {
            this.ViewTarget = buildPanel;

            Build(buildPanel);

            this.Built?.Invoke(this, EventArgs.Empty);

            _presenter.DoUpdateView();
        }

        /// <inheritdoc />
        public void DoUnload() {
            _presenter.DoUnload();
            Unload();

            this.Unloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc cref="IView.DoLoad"/>
        protected virtual async Task<bool> Load(IProgress<string> progress) {
            return await Task.FromResult(true);
        }


        /// <inheritdoc cref="IView.DoBuild"/>
        protected virtual void Build(Panel buildPanel) { /* NOOP */ }

        /// <inheritdoc cref="IView.DoUnload"/>
        protected virtual void Unload() { /* NOOP */ }

    }
}
