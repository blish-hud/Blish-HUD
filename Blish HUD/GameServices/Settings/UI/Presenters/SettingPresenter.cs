using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Settings.UI.Presenters {
    public class SettingPresenter<TSetting> : Presenter<SettingView<TSetting>, SettingEntry<TSetting>> {

        private bool _changeReady = false;

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
            if (!_changeReady) return;

            this.Model.Value = e.Value;

            GameService.Settings.Save();
        }

        protected override void UpdateView() {
            UpdateViewComplianceRequisite();
            UpdateViewDetails();

            _changeReady = true;
        }

        private void UpdateViewComplianceRequisite() {
            IEnumerable<IComplianceRequisite> complianceRequisites = this.Model.GetComplianceRequisite();

            foreach (var complianceRequisite in complianceRequisites) {
                if (!this.View.HandleComplianceRequisite(complianceRequisite)) {
                    this.View.HandleBaseComplianceRequisite(complianceRequisite);
                }
            }
        }

        private void UpdateViewDetails() {
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
