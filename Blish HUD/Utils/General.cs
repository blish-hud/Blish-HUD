using System;

namespace Blish_HUD.Utils {
    public static class General {

        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) {
            return potentialDescendant.IsSubclassOf(potentialBase)
                || potentialDescendant == potentialBase;
        }

    }
}
