using System;
using System.Collections.Generic;

namespace Blish_HUD.Controls {

    public enum MenuItemType {
        Root,
        Item
    }

    public interface IMenuItem {

        public event EventHandler<ControlActivatedEventArgs> ItemSelected;

        public int MenuItemHeight { get; set; }

        public bool Selected { get; }

        public MenuItem SelectedMenuItem { get; }

        public bool ShouldShift { get; set; }

        public void Select();

        public void Select(MenuItem menuItem);

        public void Select(MenuItem menuItem, List<IMenuItem> itemPath);

        public void Deselect();

    }

}
