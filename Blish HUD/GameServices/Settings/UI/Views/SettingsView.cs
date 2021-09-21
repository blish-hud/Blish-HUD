using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public class SettingsView : SettingView<SettingCollection> {

        private FlowPanel _settingFlowPanel;

        private readonly SettingCollection _settings;

        private bool _lockBounds = true;

        public bool LockBounds {
            get => _lockBounds;
            set {
                if (_lockBounds == value) return;

                _lockBounds = value;

                UpdateBoundsLocking(_lockBounds);
            }
        }

        private ViewContainer _lastSettingContainer;

        public SettingsView(SettingEntry<SettingCollection> setting, int definedWidth = -1) : base(setting, definedWidth) {
            _settings = setting.Value;
        }
        public SettingsView(SettingCollection settings, int definedWidth = -1)
            : this(new SettingEntry<SettingCollection>() { Value = settings }, definedWidth) { /* NOOP */ }

        private void UpdateBoundsLocking(bool locked) {
            if (_settingFlowPanel == null) return;

            _settingFlowPanel.ShowBorder  = !locked;
            _settingFlowPanel.CanCollapse = !locked;
        }

        protected override void BuildSetting(Container buildPanel) {
            _settingFlowPanel = new FlowPanel() {
                Size                = buildPanel.Size,
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                ControlPadding      = new Vector2(5,  2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode     = SizingMode.Fill,
                HeightSizingMode    = SizingMode.AutoSize,
                AutoSizePadding     = new Point(0, 15),
                Parent              = buildPanel
            };

            foreach (var setting in _settings.Where(s => s.SessionDefined)) {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null) {
                    _lastSettingContainer = new ViewContainer() {
                        WidthSizingMode   = SizingMode.Fill,
                        HeightSizingMode  = SizingMode.AutoSize,
                        Parent            = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView) {
                        subSettingsView.LockBounds = false;
                    }
                }
            }

            UpdateBoundsLocking(_lockBounds);
        }

        protected override void RefreshDisplayName(string displayName) {
            _settingFlowPanel.Title = displayName;
        }

        protected override void RefreshDescription(string description) {
            _settingFlowPanel.BasicTooltipText = description;
        }

        protected override void RefreshValue(SettingCollection value) { /* NOOP */ }

    }
}
