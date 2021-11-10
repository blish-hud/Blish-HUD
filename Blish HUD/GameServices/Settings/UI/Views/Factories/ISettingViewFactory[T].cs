using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public interface ISettingViewFactory<T> : ISettingViewFactory {
        /// <summary>
        /// Creates an <see cref="IView"/> for a given <see cref="SettingEntry{T}"/>.
        /// </summary>
        /// <param name="setting">The setting to create the view for.</param>
        /// <param name="definedWidth">The desired with, or width of the parent panel.</param>
        /// <returns>The <see cref="IView"/> for the <see cref="SettingEntry{T}"/>, or <see langword="null"/> if no view was created.</returns>
        IView CreateView(SettingEntry<T> setting, int definedWidth);
    }
}
