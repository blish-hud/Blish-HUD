using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings {
    public class RegisteredSettingsMenuItem {

        public event EventHandler<ControlActivatedEventArgs> MenuItemSelected;

        private Func<View> _view;

    }
}
