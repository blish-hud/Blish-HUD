using System.ComponentModel;

namespace Blish_HUD.Graphics {
    public enum DpiMethod : int {
        [Description("Automatic - Sync With Game")]
        SyncWithGame = 0,

        [Description("Enabled - Use Game DPI")]
        UseGameDpi = 1,

        [Description("Disabled - Never Scale")]
        NoScaling = 2
    }
}
