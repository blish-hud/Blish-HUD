using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    public static class CheckableReference {



    }

    public interface ICheckable {
        
        event EventHandler<CheckChangedEvent> CheckedChanged;

        bool Checked { get; set; }

    }

    public class CheckChangedEvent:EventArgs {
        public bool Checked { get; }

        public CheckChangedEvent(bool @checked) {
            this.Checked = @checked;
        }
    }
}
