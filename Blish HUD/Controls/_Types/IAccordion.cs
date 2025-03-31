using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {
    public interface IAccordion {

        public bool Collapsed { get; set; }

        public bool ToggleAccordionState();

        public void Expand();

        public void Collapse();

    }
}
