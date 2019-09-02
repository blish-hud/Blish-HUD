namespace Blish_HUD.Contexts {

    /// <summary>
    /// The current state of the <see cref="Context"/>.
    /// </summary>
    public enum ContextState {
        /// <summary>
        /// The <see cref="Context"/> has not loaded yet.
        /// </summary>
        None,

        /// <summary>
        /// The <see cref="Context"/> is currently loading.
        /// </summary>
        Loading,

        /// <summary>
        /// The <see cref="Context"/> has loaded.
        /// </summary>
        Ready,

        /// <summary>
        /// The <see cref="Context"/> has been unregistered and should no longer be used or referenced.
        /// </summary>
        Expired
    }

}