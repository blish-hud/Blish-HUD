using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Presenters;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {

    /// <summary>
    /// Typically used with a <see cref="Presenters.SettingsMenuPresenter"/> to create
    /// a settings tab view where menu items can be registered.
    /// </summary>
    public class SettingsMenuView : View {

        public event EventHandler<ControlActivatedEventArgs> MenuItemSelected;

        private Menu          _menuSettingsList;
        private ViewContainer _settingViewContainer;

        public SettingsMenuView() { /* NOOP */ }

        public SettingsMenuView(ISettingsMenuRegistrar settingsMenuRegistrar) {
            this.WithPresenter(new SettingsMenuPresenter(this, settingsMenuRegistrar));
        }

        protected override void Build(Panel buildPanel) {
            var settingsMenuSection = new Panel() {
                ShowBorder = true,
                Size       = new Point(265, 680),
                Location   = new Point(9,   10),
                Title      = Strings.GameServices.SettingsService.SettingsTab,
                Parent     = buildPanel,
                CanScroll  = true,
            };

            _menuSettingsList = new Menu() {
                Size           = settingsMenuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent         = settingsMenuSection,
                CanSelect      = true,
            };

            _menuSettingsList.ItemSelected += SettingsListMenuOnItemSelected;

            _settingViewContainer = new ViewContainer() {
                FadeView = true,
                Size     = new Point(718, buildPanel.Size.Y - 24 * 2),
                Location = new Point(buildPanel.Width       - 740, 24),
                Parent   = buildPanel
            };
        }

        public void SetSettingView(View view) {
            _settingViewContainer.Show(view);
        }

        public void SetMenuItems(IEnumerable<MenuItem> menuItems) {
            if (_menuSettingsList == null) return;

            var selectedMenuItem = _menuSettingsList.SelectedMenuItem;

            _menuSettingsList.ClearChildren();

            foreach (var menuItem in menuItems) {
                menuItem.Parent = _menuSettingsList;
            }

            if (selectedMenuItem?.Parent != _menuSettingsList) {
                _menuSettingsList.Select(_menuSettingsList.First() as MenuItem);
            }
        }

        private void SettingsListMenuOnItemSelected(object sender, ControlActivatedEventArgs e) {
            this.MenuItemSelected?.Invoke(this, e);
        }

    }
}
