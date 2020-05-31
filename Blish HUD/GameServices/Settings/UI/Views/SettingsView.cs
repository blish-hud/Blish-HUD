using System;
using System.ComponentModel;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Presenters;
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

        public SettingsView(SettingCollection settings, int definedWidth = -1)
            : base(new SettingEntry<SettingCollection>() { Value = settings }, definedWidth) {
            _settings = settings;
        }

        public SettingsView(SettingEntry<SettingCollection> setting, int definedWidth = -1) : base(setting, definedWidth) {
            _settings = setting.Value;
        }

        private void UpdateBoundsLocking(bool locked) {
            if (_settingFlowPanel == null) return;

            //_settingFlowPanel.CanScroll   = locked;
            _settingFlowPanel.ShowBorder  = !locked;
            _settingFlowPanel.CanCollapse = !locked;

            UnclampView();

            if (locked) {
                this.ViewTarget.ContentResized += BuildPanelOnResized;
            } else {
                _settingFlowPanel.Resized += SettingFlowPanelOnResized;

                if (_lastSettingContainer != null) {
                    _settingFlowPanel.Height = _lastSettingContainer.Bottom + _settingFlowPanel.ContentRegion.Top + Panel.BOTTOM_PADDING;

                    //this.ViewTarget.Height = this.ViewTarget.ContentRegion.Height + this.ViewTarget.ContentRegion.Top + Panel.BOTTOM_PADDING * 2;
                }
            }
        }

        private void UnclampView() {
            this.ViewTarget.ContentResized -= BuildPanelOnResized;
            _settingFlowPanel.Resized      -= SettingFlowPanelOnResized;
        }

        protected override void BuildSetting(Panel buildPanel) {
            _settingFlowPanel = new FlowPanel() {
                Size                = buildPanel.Size,
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(0, 5),
                //BackgroundColor     = new Color(RandomUtil.GetRandom(0, 250), RandomUtil.GetRandom(0, 250), RandomUtil.GetRandom(0, 250)),
                Parent              = buildPanel
            };

            foreach (var setting in _settings) {
                IView settingView = null;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null) {
                    _lastSettingContainer = new ViewContainer() {
                        Size = new Point(_settingFlowPanel.Width - 1, 5),
                        Parent = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView) {
                        subSettingsView.LockBounds = false;
                    }
                }
            }

            UpdateBoundsLocking(_lockBounds);
        }

        private void BuildPanelOnResized(object sender, RegionChangedEventArgs e) {
            _settingFlowPanel.Size = e.CurrentRegion.Size;
        }

        private void SettingFlowPanelOnResized(object sender, ResizedEventArgs e) {
            this.ViewTarget.Size = _settingFlowPanel.Size;
        }

        protected override void RefreshDisplayName(string displayName) {
            _settingFlowPanel.Title = displayName;
        }

        protected override void RefreshDescription(string description) {
            _settingFlowPanel.BasicTooltipText = description;
        }

        protected override void RefreshValue(SettingCollection value) { /* NOOP */ }

        protected override void Unload() {
            UnclampView();
        }

    }
}
