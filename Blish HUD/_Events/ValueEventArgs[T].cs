using System;

namespace Blish_HUD {
    public class ValueEventArgs<T> : EventArgs {

        /// <summary>
        /// The value of the property that triggered the event.
        /// </summary>
        public T Value { get; }

        public ValueEventArgs(T value) {
            this.Value = value;
        }

    }
}
