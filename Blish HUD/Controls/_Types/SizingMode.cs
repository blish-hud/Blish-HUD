namespace Blish_HUD.Controls {
    public enum SizingMode {
        /// <summary>
        /// The size is specified manually.
        /// </summary>
        Standard,

        /// <summary>
        /// The size will update automatically to
        /// fit the children within it.
        /// </summary>
        AutoSize,

        /// <summary>
        /// The size will update automatically to
        /// fill the remainder of the space.
        /// </summary>
        Fill
    }
}
