using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Blish_HUD.Settings {

    /// <summary>
    /// An interface for a setting collection, used for storing and retrieving custom settings in Blish HUD.
    /// </summary>
    public interface ISettingCollection : INotifyPropertyChanged { 

        /// <summary>
        /// Whether this setting collection should be rendered in the UI.
        /// </summary>
        bool RenderInUi { get; }

        /// <summary>
        /// Adds a new sub collection for storing settings.
        /// </summary>
        /// <param name="collectionKey">The subc ollection key.</param>
        /// <param name="renderInUi">Whether the sub collection should be rendered in the UI.</param>
        /// <returns>The newly created sub collection.</returns>
        ISettingCollection AddSubCollection(string collectionKey, bool renderInUi = false);

        /// <summary>
        /// Checks whether this setting collection contains a sub collection with the given key, and sets <paramref name="collection"/> to that sub collection.
        /// </summary>
        /// <param name="collectionKey">The subc ollection key.</param>
        /// <param name="collection">The sub collection if found, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> if the subc ollection exists, or <see langword="false"/> otherwise.</returns>
        bool TryGetSubCollection(string collectionKey, out ISettingCollection collection);

        /// <summary>
        /// Gets a sub collection with the given name from this setting collection.
        /// </summary>
        /// <param name="collectionKey">The sub collection key.</param>
        /// <returns>The sub collection.</returns>
        /// <exception cref="KeyNotFoundException">The setting collection does not contain a sub collection with the given key.</exception>
        ISettingCollection GetSubCollection(string collectionKey);


        /// <summary>
        /// Defines a new setting.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="entryKey">The setting key.</param>
        /// <param name="defaultValue">The default setting value.</param>
        /// <returns>The newly created setting.</returns>
        ISettingEntry<T> DefineSetting<T>(string entryKey, T defaultValue);

        /// <summary>
        /// Defines a new UI setting.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="entryKey">The setting key.</param>
        /// <param name="defaultValue">The default setting value.</param>
        /// <param name="displayNameFunc">The <see cref="Func{string}"/> that returns the setting display name.</param>
        /// <param name="descriptionFunc">The <see cref="Func{string}"/> that returns the setting description.</param>
        /// <returns>The newly created setting.</returns>
        IUiSettingEntry<T> DefineUiSetting<T>(string entryKey, T defaultValue,
            Func<string> displayNameFunc = null, Func<string> descriptionFunc = null);

        /// <summary>
        /// Defines a new setting.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="entryKey">The setting key.</param>
        /// <param name="defaultValue">The default setting value.</param>
        /// <param name="displayName">The setting display name.</param>
        /// <param name="description">The setting description.</param>
        /// <param name="renderer">The renderer.</param>
        /// <returns>The newly created setting.</returns>
        [Obsolete("This function does not produce a localization friendly SettingEntry.")]
        IUiSettingEntry<T> DefineSetting<T>(string entryKey, T defaultValue, 
            string displayName, string description, SettingsService.SettingTypeRendererDelegate renderer = null);

        /// <summary>
        /// Undefines a setting.
        /// </summary>
        /// <param name="entryKey">The setting name.</param>
        void UndefineSetting(string entryKey);


        /// <summary>
        /// Checks whether this setting collection contains a setting with the given key.
        /// </summary>
        /// <param name="entryKey">The setting key.</param>
        /// <returns><see langword="true"/> if the setting exists, or <see langword="false"/> otherwise.</returns>
        bool ContainsSetting(string entryKey);

        /// <summary>
        /// Checks whether this setting collection contains a setting with the given key and type, and sets <paramref name="settingEntry"/> to that setting.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="entryKey">The setting key.</param>
        /// <param name="settingEntry">The setting if found, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> if the setting exists, or <see langword="false"/> otherwise.</returns>
        bool TryGetSetting<T>(string entryKey, out ISettingEntry<T> settingEntry);

        /// <summary>
        /// Gets a setting with the given name and type from this setting collection.
        /// </summary>
        /// <typeparam name="T">The setting type.</typeparam>
        /// <param name="entryKey">The setting key.</param>
        /// <returns>The setting.</returns>
        /// <exception cref="KeyNotFoundException">The setting collection does not contain a setting with the given key.</exception>
        ISettingEntry<T> GetSetting<T>(string entryKey);

        /// <summary>
        /// Gets all defined settings.
        /// </summary>
        /// <param name="uiOnly">Whether to only get the UI settings.</param>
        /// <returns>An enumerable of all defined settings.</returns>
        IEnumerable<ISettingEntry> GetDefinedSettings(bool uiOnly = false);
    }
}
