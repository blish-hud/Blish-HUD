using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {
    public interface IMenuItem {

        event EventHandler<EventArgs> ItemSelected;

        int MenuItemHeight { get; set; }

        bool Selected { get; set; }

        bool ShouldShift { get; set; }

    }

}
