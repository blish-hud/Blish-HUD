using System;

namespace Blish_HUD.Settings {

    /// <summary>
    /// An interface for a setting entry, used for storing and retrieving custom UI settings in Blish HUD.
    /// </summary>
    public interface IUiSettingEntry<T> : ISettingEntry<T> {

        /// <summary>
        /// Gets or sets the func that resolves the setting description.
        /// </summary>
        Func<string> GetDescriptionFunc { get; set; }

        /// <summary>
        /// Gets or sets the func that resolves the setting display name.
        /// </summary>
        Func<string> GetDisplayNameFunc { get; set; }


        /// <summary>
        /// Gets the setting description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the setting display name.
        /// </summary>
        string DisplayName { get; }
    }
}
