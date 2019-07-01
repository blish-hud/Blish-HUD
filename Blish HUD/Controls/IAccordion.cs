using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {
    public interface IAccordion {

        bool Collapsed { get; set; }

        bool ToggleAccordionState();

        void Expand();

        void Collapse();

    }
}
