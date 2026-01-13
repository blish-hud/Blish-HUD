namespace Blish_HUD.GameIntegration.Gw2Instance {

    public enum Gw2GraphicsApi {
        /// <summary>
        /// Indicates that the graphics API used could not be detected or that the main game window hasn't been opened yet.
        /// </summary>
        Unknown,
        /// <summary>
        /// Indicates that Guild Wars 2 is running with DirectX 9 enabled.
        /// </summary>
        DX9,

        /// <summary>
        /// Indicates that Guild Wars 2 is running with DirectX 11 enabled.
        /// </summary>
        DX11
    }

}