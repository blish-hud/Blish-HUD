using System;

namespace Blish_HUD.Gw2Mumble {
    internal static class MumbleEventImpl {

        internal static void CheckAndHandleEvent<T>(ref T previousValue, T currentValue, Action<ValueEventArgs<T>> eventRef) {
            if (!Equals(previousValue, currentValue)) {
                previousValue = currentValue;
                eventRef(new ValueEventArgs<T>(currentValue));
            }
        }

    }
}
