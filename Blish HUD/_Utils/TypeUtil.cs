using System;

namespace Blish_HUD {
    public static class TypeUtil {

        /// <summary>
        /// Returns true if <see cref="potentialDescendant"/> inherits or is an instance of <see cref="potentialBase"/>.
        /// </summary>
        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) {
            return potentialDescendant.IsSubclassOf(potentialBase)
                || potentialDescendant == potentialBase;
        }

    }
}
