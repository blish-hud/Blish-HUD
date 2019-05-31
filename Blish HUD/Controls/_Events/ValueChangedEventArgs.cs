using System;
using System.Collections.Generic;
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

}
