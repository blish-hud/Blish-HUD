using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {
    public class KeyedValueChangedEventArgs<TKey, TValue> : EventArgs {

        /// <summary>
        /// The key of the property that triggered the event.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// The value of the property that triggered the event.
        /// </summary>
        public TValue Value { get; }

        public KeyedValueChangedEventArgs(TKey key, TValue value) {
            this.Key   = key;
            this.Value = value;
        }

    }
}
