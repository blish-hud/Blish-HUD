using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.GameServices;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;

namespace Blish_HUD.Overlay {
    public sealed class OverlaySettingsTab : ServiceModule<OverlayService>, ISettingsMenuRegistrar {

        public event EventHandler<EventArgs> RegistrarListChanged;

        private readonly List<(MenuItem MenuItem, Func<MenuItem, IView> ViewFunc, int Index)> _registeredMenuItems = new List<(MenuItem MenuItem, Func<MenuItem, IView> ViewFunc, int Index)>();

        public OverlaySettingsTab(OverlayService service) : base(service) { }

        public IView GetMenuItemView(MenuItem selectedMenuItem) {
            foreach (var (menuItem, viewFunc, _) in _registeredMenuItems) {
                if (menuItem == selectedMenuItem || menuItem.GetDescendants().Contains(selectedMenuItem)) {
                    return viewFunc(selectedMenuItem);
                }
            }

            return null;
        }

        public IEnumerable<MenuItem> GetSettingMenus() => _registeredMenuItems.OrderBy(mi => mi.Index).Select(mi => mi.MenuItem);

        public void RegisterSettingMenu(MenuItem menuItem, Func<MenuItem, IView> viewFunc, int index = 0) {
            _registeredMenuItems.Add((menuItem, viewFunc, index));

            this.RegistrarListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveSettingMenu(MenuItem menuItem) {
            _registeredMenuItems.RemoveAll(r => r.MenuItem == menuItem);

            this.RegistrarListChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}
