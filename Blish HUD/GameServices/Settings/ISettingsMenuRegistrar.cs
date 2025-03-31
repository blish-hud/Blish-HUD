using System;
using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings {

    public interface ISettingsMenuRegistrar {

        /// <summary>
        /// Occurs when the list of root menus is updated.
        /// </summary>
        public event EventHandler<EventArgs> RegistrarListChanged;

        /// <summary>
        /// Returns a list of all root level menu items for the settings menu.
        /// </summary>
        public IEnumerable<MenuItem> GetSettingMenus();

        /// <summary>
        /// Gets the view associated with a menu item.
        /// </summary>
        public IView GetMenuItemView(MenuItem selectedMenuItem);

    }

}