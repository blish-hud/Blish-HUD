namespace Blish_HUD.Controls {
    /// <summary>
    /// Implemented by controls which can have <see cref="Tab"/>s.
    /// </summary>
    public interface ITabOwner {
        
        /// <summary>
        /// A collection of <see cref="Tab"/> controls, in the order they will be displayed in the window.
        /// </summary>
        TabCollection Tabs { get; }

        /// <summary>
        /// The actively selected tab.
        /// </summary>
        Tab SelectedTab { get; set; }
        
    }
}
