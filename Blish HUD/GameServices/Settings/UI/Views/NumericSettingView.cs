using Blish_HUD.Controls;
using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public abstract class NumericSettingView<T> : SettingView<T>
        where T : IComparable<T> {
        
        private const int CONTROL_PADDING = 5;

        private const int TRACKBAR_WIDTH  = 277;
        private const int TRACKBAR_HEIGHT = 16;
        
        protected Label    _displayNameLabel;
        protected TrackBar _valueTrackBar;

        protected NumericSettingView(IUiSettingEntry<T> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Panel buildPanel) {
            _displayNameLabel = new Label() {
                AutoSizeWidth = true,
                Location      = new Point(CONTROL_PADDING, 0),
                Parent        = buildPanel
            };

            _valueTrackBar = new TrackBar() {
                Size   = new Point(TRACKBAR_WIDTH, TRACKBAR_HEIGHT),
                Left   = 185,
                Parent = buildPanel
            };

            _valueTrackBar.ValueChanged += HandleTrackBarChanged;
        }

        protected abstract void HandleTrackBarChanged(object sender, ValueEventArgs<float> e);

        private void UpdateSizeAndLayout() {
            this.ViewTarget.Height   = _valueTrackBar.Bottom + CONTROL_PADDING;
            _displayNameLabel.Height = this.ViewTarget.Height;
        }

        protected override void RefreshDisplayName(string displayName) {
            _displayNameLabel.Text = displayName;

            UpdateSizeAndLayout();
        }

        protected override void RefreshDescription(string description) {
            _valueTrackBar.BasicTooltipText = description;
        }

    }
}
