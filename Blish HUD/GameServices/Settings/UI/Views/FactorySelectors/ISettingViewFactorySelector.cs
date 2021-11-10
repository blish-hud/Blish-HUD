namespace Blish_HUD.Settings.UI.Views {
    using System;

    public interface ISettingViewFactorySelector {
        /// <summary>
        /// Gets a factory for a <see cref="SettingEntry{T}"/> 
        /// </summary>
        /// <typeparam name="T">The type of setting factory to retrieve.</typeparam>
        /// <returns>An <see cref="ISettingViewFactory{T}"/>, or <see langword="null"/> if a suitable <see cref="ISettingViewFactory{T}"/> was not found.</returns>
        ISettingViewFactory<T> GetFactoryForType<T>();

        /// <summary>
        /// Gets a factory for a given Type.
        /// </summary>
        /// <typeparam name="T">The type of setting factory to retrieve.</typeparam>
        /// <returns>An <see cref="ISettingViewFactory"/>, or <see langword="null"/> if a suitable <see cref="ISettingViewFactory"/> was not found.</returns>
        ISettingViewFactory GetFactoryForType(Type t);
    }
}
