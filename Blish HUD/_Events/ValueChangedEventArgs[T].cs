using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {

    public class ValueChangedEventArgs<T> : EventArgs {

        /// <summary>
        /// The value of the property before it was changed.
        /// </summary>
        public T PrevousValue { get; }

        /// <summary>
        /// The value of the property now that it has been changed.
        /// </summary>
        public T NewValue { get; }

        public ValueChangedEventArgs(T previousValue, T newValue) {
            this.PrevousValue = previousValue;
            this.NewValue     = newValue;
        }

    }

}
