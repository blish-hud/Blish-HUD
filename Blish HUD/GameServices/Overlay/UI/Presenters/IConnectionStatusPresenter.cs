using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Overlay.UI.Presenters {
    public interface IConnectionStatusPresenter : IPresenter {

        public string ConnectionName { get; }

        public bool Connected { get; }

        public string ConnectionDetails { get; }
    }
}
