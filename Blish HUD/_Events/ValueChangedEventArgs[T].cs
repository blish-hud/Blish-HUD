using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {

    public class ValueChangedEventArgs<T> : EventArgs {

        private readonly T _previousValue;
        private readonly T _newValue;

        /// <summary>
        /// The value of the property before it was changed.
        /// </summary>
        public T PreviousValue => _previousValue;

        [Obsolete("Typo.  Use 'PreviousValue' instead.")]
        public T PrevousValue => _previousValue;

        /// <summary>
        /// The value of the property now that it has been changed.
        /// </summary>
        public T NewValue => _newValue;

        public ValueChangedEventArgs(T previousValue, T newValue) {
            _previousValue = previousValue;
            _newValue      = newValue;
        }

    }

}
