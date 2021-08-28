using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Controls {
    public interface ITab {

        AsyncTexture2D Icon { get; }

        int Priority { get; }

        Tooltip Tooltip { get; }

        IView View { get; }

    }
}
