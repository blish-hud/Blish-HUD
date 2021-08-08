using System;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Presenters;

namespace Blish_HUD.Settings.UI.Views {
    public abstract class SettingView<TSetting> : View {

        public event EventHandler<ValueEventArgs<TSetting>> ValueChanged;

        protected void OnValueChanged(ValueEventArgs<TSetting> e) => this.ValueChanged?.Invoke(this, e);

        private readonly int _definedWidth;

        private string   _displayName;
        private string   _description;
        private TSetting _value;

        public string DisplayName {
            get => _displayName;
            set {
                if (_displayName == value) return;

                RefreshDisplayName(_displayName = value);
            }
        }

        public string Description {
            get => _description;
            set {
                if (_description == value) return;

                RefreshDescription(_description = value);
            }
        }

        protected int DefinedWidth => _definedWidth;

        public TSetting Value {
            get => _value;
            set {
                if (Equals(_value, value)) return;

                RefreshValue(_value = value);
            }
        }

        protected SettingView(IUiSettingEntry<TSetting> setting, int definedWidth) {
            _definedWidth = definedWidth;

            this.WithPresenter(new SettingPresenter<TSetting>(this, setting));
        }

        protected sealed override void Build(Panel buildPanel) {
            if (_definedWidth > 0) {
                buildPanel.Width = _definedWidth;
            }

            BuildSetting(buildPanel);

            Refresh();
        }

        protected abstract void BuildSetting(Panel buildPanel);

        public virtual bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite) {
            return false;
        }

        public void HandleBaseComplianceRequisite(IComplianceRequisite complianceRequisite) {
            /* Currently none - NOOP */
        }

        private void Refresh() {
            RefreshDisplayName(_displayName);
            RefreshDescription(_description);
            RefreshValue(_value);
        }

        protected abstract void RefreshDisplayName(string displayName);

        protected abstract void RefreshDescription(string description);

        protected abstract void RefreshValue(TSetting value);

    }
}
