namespace Blish_HUD.Input {

    public enum MouseEventType {

        /// <summary>
        /// Occurs when the mouse has moved.
        /// </summary>
        MouseMoved = 512,

        /// <summary>
        /// Occurs when the left-mouse button is pressed.
        /// </summary>
        LeftMouseButtonPressed = 513,

        /// <summary>
        /// Occurs when the left-mouse button is released.
        /// </summary>
        LeftMouseButtonReleased = 514,

        /// <summary>
        /// Occurs when the right-mouse button is pressed.
        /// </summary>
        RightMouseButtonPressed = 516,

        /// <summary>
        /// Occurs when the right-mouse button is released.
        /// </summary>
        RightMouseButtonReleased = 517,

        /// <summary>
        /// Occurs when the mouse-wheel is scrolled.
        /// </summary>
        MouseWheelScrolled = 522,

        /// <summary>
        /// Occurs when the mouse enters the bounds of the control.
        /// </summary>
        /// <remarks>
        /// Exclusive to mouse events on <see cref="Controls"/>.
        /// </remarks>
        MouseEntered,

        /// <summary>
        /// Occurs when the mouse leaves the bounds of the control.
        /// </summary>
        /// <remarks>
        /// Exclusive to mouse events on <see cref="Controls"/>.
        /// </remarks>
        MouseLeft

    }

}
