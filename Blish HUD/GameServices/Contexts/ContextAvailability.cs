namespace Blish_HUD.Contexts {
    /// <summary>
    /// Describes the availability of a <see cref="Context"/>'s endpoint after it is called.
    /// </summary>
    public enum ContextAvailability {
        /// <summary>
        /// The value provided by this context is not available.
        /// <see cref="ContextResult{T}.Status"/> should detail why it
        /// is not available (e.g. missing dependency).
        /// </summary>
        Unavailable,

        /// <summary>
        /// The value provided by this context is not ready or the
        /// context has been unloaded.
        /// </summary>
        NotReady,

        /// <summary>
        /// The value provided by this context is available.
        /// </summary>
        Available,

        /// <summary>
        /// The value provided by this context is not available because something failed.
        /// <see cref="ContextResult{T}.Status"/> should detail what failed.
        /// </summary>
        Failed
    }
}
