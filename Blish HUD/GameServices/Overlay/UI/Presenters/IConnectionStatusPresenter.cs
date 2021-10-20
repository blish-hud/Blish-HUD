using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Overlay.UI.Presenters {
    public interface IConnectionStatusPresenter : IPresenter {

        string ConnectionName { get; }

        bool Connected { get; }

        string ConnectionDetails { get; }

    }
}
