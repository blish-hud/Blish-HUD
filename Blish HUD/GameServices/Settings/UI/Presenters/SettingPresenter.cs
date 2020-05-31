using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Settings.UI.Presenters {
    public class SettingPresenter<TSetting> : Presenter<SettingView<TSetting>, SettingEntry<TSetting>> {

        public SettingPresenter(SettingView<TSetting> view, SettingEntry<TSetting> model) : base(view, model) { /* NOOP */ }

        protected override Task<bool> Load(IProgress<string> progress) {
            this.Model.SettingChanged += ModelOnSettingChanged;
            this.View.ValueChanged    += ViewOnValueChanged;

            return base.Load(progress);
        }

        private void ModelOnSettingChanged(object sender, ValueChangedEventArgs<TSetting> e) {
            this.View.Value = e.NewValue;
        }

        private void ViewOnValueChanged(object sender, ValueEventArgs<TSetting> e) {
            this.Model.Value = e.Value;
        }

        protected override void UpdateView() {
            this.View.DisplayName = !string.IsNullOrEmpty(this.Model.DisplayName)
                                        ? this.Model.DisplayName
                                        : this.Model.EntryKey;

            this.View.Description = this.Model.Description;
            this.View.Value       = this.Model.Value;
        }

        protected override void Unload() {
            this.Model.SettingChanged -= ModelOnSettingChanged;
            this.View.ValueChanged    -= ViewOnValueChanged;
        }

    }
}
