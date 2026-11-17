namespace Blish_HUD.Controls {
    /// <summary>
    /// Determines which point of a <see cref="Control"/> is aligned to the position
    /// calculated by <see cref="Control.XPercent"/> and <see cref="Control.YPercent"/>.
    /// </summary>
    public enum AnchorPoint {
        /// <summary>
        /// The top-left corner of the control is aligned to the calculated position.
        /// </summary>
        TopLeft,

        /// <summary>
        /// The top-center point of the control is aligned to the calculated position.
        /// </summary>
        Top,

        /// <summary>
        /// The top-right corner of the control is aligned to the calculated position.
        /// </summary>
        TopRight,

        /// <summary>
        /// The middle-left point of the control is aligned to the calculated position.
        /// </summary>
        Left,

        /// <summary>
        /// The center of the control is aligned to the calculated position.
        /// </summary>
        Center,

        /// <summary>
        /// The middle-right point of the control is aligned to the calculated position.
        /// </summary>
        Right,

        /// <summary>
        /// The bottom-left corner of the control is aligned to the calculated position.
        /// </summary>
        BottomLeft,

        /// <summary>
        /// The bottom-center point of the control is aligned to the calculated position.
        /// </summary>
        Bottom,

        /// <summary>
        /// The bottom-right corner of the control is aligned to the calculated position.
        /// </summary>
        BottomRight
    }
}
