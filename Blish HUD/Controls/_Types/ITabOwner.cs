namespace Blish_HUD.Controls {
    /// <summary>
    /// Implemented by controls which can have <see cref="Tab"/>s.
    /// </summary>
    public interface ITabOwner {

        /// <summary>
        /// A collection of <see cref="Tab"/> controls, in the order they will be displayed in the window.
        /// </summary>
        public TabCollection Tabs { get; }

        /// <summary>
        /// The actively selected tab.
        /// </summary>
        public Tab SelectedTab { get; set; }

    }
}
