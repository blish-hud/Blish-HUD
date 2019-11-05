using System;
using System.Threading.Tasks;
using Blish_HUD.Common;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views.SettingViews;

namespace Blish_HUD.Settings.UI.Presenters {

    public class SettingPresenter<TSetting> : Presenter<SettingView<TSetting>, SettingEntry<TSetting>> {

        /// <inheritdoc />
        public SettingPresenter(SettingView<TSetting> view, SettingEntry<TSetting> model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            this.Model.SettingChanged += SettingOnValueChanged;
            this.View.ValueChanged    += ViewOnValueChanged;

            return base.Load(progress);
        }

        private void SettingOnValueChanged(object sender, ValueChangedEventArgs<TSetting> e) {
            this.View.Value = e.NewValue;
        }

        private void ViewOnValueChanged(object sender, EventValueArgs<TSetting> e) {
            this.Model.Value = e.Value;
        }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.DisplayName = this.Model.DisplayName;
            this.View.Description = this.Model.Description;
            this.View.Value       = this.Model.Value;
        }

        /// <inheritdoc />
        protected override void Unload() {
            this.Model.SettingChanged -= SettingOnValueChanged;
            this.View.ValueChanged    -= ViewOnValueChanged;
        }

    }
}
