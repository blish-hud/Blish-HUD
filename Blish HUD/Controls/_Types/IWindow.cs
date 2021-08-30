namespace Blish_HUD.Controls {
    public interface IWindow {

        /// <summary>
        /// If the window should be forced on top of all other windows.
        /// </summary>
        bool TopMost { get; }

        /// <summary>
        /// The last time the window was made active from a click or shown.
        /// </summary>
        double LastInteraction { get; }

        /// <summary>
        /// Brings the window to the front of all other windows.
        /// </summary>
        void BringWindowToFront();

        /// <summary>
        /// If <c>true</c> the window can support closing itself (X icon or pressing ESC).  Otherwise, an external action will be required to close it.
        /// </summary>
        bool CanClose { get; }

    }
}
