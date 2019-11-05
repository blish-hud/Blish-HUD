using System;

namespace Blish_HUD.Common {
    public class EventValueArgs<T> : EventArgs {

        private readonly T _value;

        public T Value => _value;

        public EventValueArgs(T value) {
            _value = value;
        }

    }
}
