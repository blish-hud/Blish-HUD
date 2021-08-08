using System;
using System.ComponentModel;

namespace Blish_HUD.Settings {

    /// <summary>
    /// An interface for a setting entry, used for storing and retrieving custom settings in Blish HUD.
    /// </summary>
    public interface ISettingEntry {

        /// <summary>
        /// Gets or sets the unique setting key used to identify the setting in the setting collection.
        /// </summary>
        string EntryKey { get; set; }

        /// <summary>
        /// The event that is triggered when the setting is updated.
        /// </summary>
        event EventHandler SettingUpdated;
    }

    /// <summary>
    /// An interface for a setting entry, used for storing and retrieving custom settings in Blish HUD.
    /// </summary>
    public interface ISettingEntry<T> : ISettingEntry, INotifyPropertyChanged {

        /// <summary>
        /// Gets or sets the setting value.
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// The event that is triggered when the setting values changes.
        /// </summary>
        event EventHandler<ValueChangedEventArgs<T>> SettingChanged;
    }
}
