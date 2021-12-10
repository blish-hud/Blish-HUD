using System.ComponentModel;

namespace Blish_HUD.Graphics {
    public enum DynamicHUDMethod : int {
        [Description("Always Show (Default)")]
        AlwaysShow = 0,

        [Description("Show Only out of Combat")]
        ShowPeaceful = 1,

        [Description("Show Only in Combat")]
        ShowInCombat = 2,

        [Description("Never Show")]
        NeverShow = 3
    }
}
