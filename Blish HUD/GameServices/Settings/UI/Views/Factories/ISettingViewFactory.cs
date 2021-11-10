using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Settings.UI.Views {
    public interface ISettingViewFactory {
        /// <summary>
        /// Creates an <see cref="IView"/> for a given <see cref="SettingEntry"/>.
        /// </summary>
        /// <param name="setting">The setting to create the view for.</param>
        /// <param name="definedWidth">The desired with, or width of the parent panel.</param>
        /// <returns>The <see cref="IView"/> for the <see cref="SettingEntry"/>, or <see langword="null"/> if no view was created.</returns>
        IView CreateView(SettingEntry setting, int definedWidth);
    }
}
