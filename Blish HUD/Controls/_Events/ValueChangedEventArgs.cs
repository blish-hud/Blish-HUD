using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    public class ValueChangedEventArgs : EventArgs {
        public string PreviousValue { get; }
        public string CurrentValue  { get; }

        public ValueChangedEventArgs(string previousValue, string currentValue) {
            this.PreviousValue = previousValue;
            this.CurrentValue  = currentValue;
        }
    }

    public class ValueChangedEventArgs<T> : EventArgs {

        private T _previousValue;
        private T _newValue;

        public T PrevousValue => _previousValue;
        public T NewValue     => _newValue;

        public ValueChangedEventArgs(T previousValue, T newValue) {
            _previousValue = previousValue;
            _newValue      = newValue;
        }

    }

}
