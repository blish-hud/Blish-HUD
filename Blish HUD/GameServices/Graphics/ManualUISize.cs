using System.ComponentModel;

namespace Blish_HUD.Graphics {
    public enum ManualUISize : int {
        [Description("Automatic - Sync With Game")]
        SyncWithGame = 0,

        [Description("Small")]
        Small = 1,

        [Description("Normal")]
        Normal = 2,

        [Description("Large")]
        Large = 3,

        [Description("Larger")]
        Larger = 4
    }
}
