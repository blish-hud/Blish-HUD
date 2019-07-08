using System;

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
