using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.TacO.Origin {
    public enum POIBehavior {
        AlwaysVisible,
        ReappearOnMapChange,
        ReappearOnDailyReset,
        OnlyVisibleBeforeActivation,
        ReappearAfterTimer,
        ReappearOnMapReset,
        OncePerInstance,
        DailyPerChar,
        OncePerInstancePerChar,
        WvWObjective,
    }
}
