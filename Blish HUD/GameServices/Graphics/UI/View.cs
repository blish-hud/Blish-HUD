using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Graphics.UI {
    public class View : IView {

        private static readonly NullPresenter _sharedNullPresenter = new NullPresenter();

        public event EventHandler<EventArgs> Loaded;

        public event EventHandler<EventArgs> Built;

        public event EventHandler<EventArgs> Unloaded;

        public IPresenter Presenter { get; private set; } = _sharedNullPresenter;

        protected ViewContainer ViewTarget { get; private set; }

        protected View() { /* NOOP */ }

        protected View(IPresenter presenter) {
            Presenter = presenter;
        }

        public View WithPresenter(IPresenter presenter) {
            Presenter = presenter
                      ?? throw new ArgumentNullException(nameof(presenter));

            this.ViewTarget?.Show(this);

            return this;
        }

        public async Task<bool> DoLoad(IProgress<string> progress) {
            bool loadResult = await Presenter.DoLoad(progress) && await Load(progress);

            if (loadResult) {
                this.Loaded?.Invoke(this, EventArgs.Empty);
            }

            return loadResult;
        }

        public void DoBuild(Panel buildPanel) {
            this.ViewTarget = (ViewContainer)buildPanel;

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


        protected virtual void Build(Panel buildPanel) { /* NOOP */ }

        protected virtual void Unload() { /* NOOP */ }

    }
}
