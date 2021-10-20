using System;
using System.Threading.Tasks;

namespace Blish_HUD.Overlay.UI.Presenters {
    public class ConnectionStatusPresenter : IConnectionStatusPresenter {

        private readonly Func<string> _connectionName;
        private readonly Func<bool>   _connected;
        private readonly Func<string> _connectionDetails;

        public ConnectionStatusPresenter(Func<string> connectionName, Func<bool> connected, Func<string> connectionDetails) {
            _connectionName    = connectionName    ?? throw new ArgumentNullException(nameof(connectionName));
            _connected         = connected         ?? throw new ArgumentNullException(nameof(connected));
            _connectionDetails = connectionDetails ?? throw new ArgumentNullException(nameof(connectionDetails));
        }

        public Task<bool> DoLoad(IProgress<string> progress) => Task.FromResult(true);

        public void DoUpdateView() { /* NOOP */ }

        public void DoUnload() { /* NOOP */ }

        public string ConnectionName    => _connectionName.Invoke();
        public bool   Connected         => _connected.Invoke();
        public string ConnectionDetails => _connectionDetails.Invoke();

    }
}
