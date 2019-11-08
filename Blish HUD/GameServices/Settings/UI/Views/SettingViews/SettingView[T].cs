using System;
using Blish_HUD.Common;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Presenters;

namespace Blish_HUD.Settings.UI.Views.SettingViews {

    public abstract class SettingView<TSetting> : View<SettingPresenter<TSetting>> {

        public event EventHandler<EventValueArgs<TSetting>> ValueChanged;

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

        public TSetting Value {
            get => _value;
            set {
                if (Equals(_value, value)) return;

                RefreshValue(_value = value);
            }
        }

        protected int DefinedWidth => _definedWidth;

        protected SettingView(SettingEntry<TSetting> setting, int definedWidth) {
            _definedWidth = definedWidth;

            this.Presenter = new SettingPresenter<TSetting>(this, setting);
        }

        /// <inheritdoc />
        protected sealed override void Build(Panel buildPanel) {
            if (_definedWidth > 0) {
                buildPanel.Width = _definedWidth;
            }

            BuildSetting(buildPanel);

            Refresh();
        }

        protected abstract void BuildSetting(Panel buildPanel);

        protected void OnValueChanged(EventValueArgs<TSetting> e) {
            this.ValueChanged?.Invoke(this, e);
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