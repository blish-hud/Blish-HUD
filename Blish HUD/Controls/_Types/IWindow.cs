namespace Blish_HUD.Controls {
    /// <summary>
    /// Implemented by controls intended to be treated as windows.  Use with <see cref="WindowBase2.RegisterWindow"/>.
    /// </summary>
    public interface IWindow {

        /// <summary>
        /// Returns true if the window is currently visible.
        /// </summary>
        bool Visible { get; }

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
        /// If <c>true</c> the window can support closing itself with the X icon.  Otherwise, an external action will be required to close it.
        /// </summary>
        bool CanClose { get; }

        /// <summary>
        /// If <c>true</c> the window can support closing itself with pressing Escape.  Otherwise, an external action will be required to close it.
        /// </summary>
        bool CanCloseWithEscape { get; }

        /// <summary>
        /// Hides the window.
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();


    }
}
