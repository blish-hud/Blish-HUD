using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Settings.UI.Presenters {

    public class SettingsPresenter : Presenter<SettingsView, SettingCollection> {

        /// <inheritdoc />
        public SettingsPresenter(SettingsView view, SettingCollection model) : base(view, model) { }

        /// <inheritdoc />
        protected override void UpdateView() {
            for (int i = 0; i < this.Model.Entries.Count; i++) {
                if (this.Model.Entries[i] != null && !string.IsNullOrEmpty(this.Model.Entries[i].DisplayName)) {
                    this.View.AddSettingView(this.Model.Entries[i]);
                }
            }
        }

    }
}
