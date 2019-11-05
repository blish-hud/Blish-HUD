using System;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Presenters;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Views {

    public interface IControlView<out TControl> where TControl : Control {

        TControl Control { get; }

    }

    public static class ControlView {

        public static ControlView<TControl> FromControl<TControl>(TControl control) where TControl : Control {
            return new ControlView<TControl>(control);
        }

        public static ControlView<TControl> FromControl<TControl>(TControl control, IControlPresenter presenter) where TControl : Control {
            return new ControlView<TControl>(control, presenter);
        }

    }

    public class ControlView<TControl> : IControlView<TControl>, IView<IControlPresenter>
        where TControl : Control {

        /// <inheritdoc />
        public event EventHandler<EventArgs> Loaded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Built;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Unloaded;

        private readonly TControl _control;

        private IControlPresenter _presenter;

        /// <inheritdoc />
        public TControl Control => _control;

        /// <inheritdoc cref="IView{TPresenter}.Presenter"/>
        public IControlPresenter Presenter {
            get => _presenter;
            set {
                if (_presenter == value) return;

                _presenter?.DoUnload();

                _presenter = value;

                _ = DoLoad(new Progress<string>((report) => { /* NOOP */ }));
            }
        }

        public ControlView(TControl control) {
            _control = control;
        }

        public ControlView(TControl control, IControlPresenter presenter) : this(control) {
            this.Presenter = presenter;
        }

        /// <inheritdoc />
        public async Task<bool> DoLoad(IProgress<string> progress) {
            bool loadResult = await _presenter.DoLoad(progress);

            if (loadResult) {
                this.Loaded?.Invoke(this, EventArgs.Empty);

                this.Built?.Invoke(this, EventArgs.Empty);

                _presenter.DoUpdateView();
            }

            return loadResult;
        }

        /// <inheritdoc />
        public void DoBuild(Panel buildPanel) {
            _control.Parent = buildPanel;
        }

        /// <inheritdoc />
        public void DoUnload() {
            _control.Dispose();
        }

        public static implicit operator TControl(ControlView<TControl> controlView) {
            return controlView._control;
        }

    }
}
