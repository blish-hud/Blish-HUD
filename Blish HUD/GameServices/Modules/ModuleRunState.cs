namespace Blish_HUD.Modules {

    public enum ModuleRunState {
        /// <summary>
        /// The module is not loaded.
        /// </summary>
        Unloaded,

        /// <summary>
        /// The module is currently still working to complete its initial <see cref="Module.LoadAsync"/>.
        /// </summary>
        Loading,

        /// <summary>
        /// The module has completed loading and is enabled.
        /// </summary>
        Loaded,

        /// <summary>
        /// The module has been disabled and is currently unloading the resources it has.
        /// </summary>
        Unloading,

        /// <summary>
        /// The module has experienced an error that it can not recover from.
        /// </summary>
        FatalError
    }

}
