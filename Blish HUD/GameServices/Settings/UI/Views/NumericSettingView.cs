using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public abstract class NumericSettingView<T> : SettingView<T>
        where T : IComparable<T> {
        
        private const int CONTROL_PADDING = 5;

        private const int TRACKBAR_WIDTH  = 277;
        private const int TRACKBAR_HEIGHT = 16;
        
        protected Label    _displayNameLabel;
        protected TrackBar _valueTrackBar;

        protected NumericSettingView(SettingEntry<T> setting, int definedWidth = -1) : base(setting, definedWidth) { /* NOOP */ }

        protected override void BuildSetting(Panel buildPanel) {
            _displayNameLabel = new Label() {
                AutoSizeWidth = true,
                Location      = new Point(CONTROL_PADDING, 0),
                Parent        = buildPanel
            };

            _valueTrackBar = new TrackBar() {
                Size   = new Point(TRACKBAR_WIDTH, TRACKBAR_HEIGHT),
                Parent = buildPanel
            };

            _valueTrackBar.ValueChanged += HandleTrackBarChanged;
        }

        protected abstract void HandleTrackBarChanged(object sender, ValueEventArgs<float> e);

        private void UpdateSizeAndLayout() {
            this.ViewTarget.Height   = _valueTrackBar.Bottom + CONTROL_PADDING;
            _displayNameLabel.Height = this.ViewTarget.Height;

            if (this.DefinedWidth > 0) {
                _valueTrackBar.Right  = this.DefinedWidth - CONTROL_PADDING - _valueTrackBar.Width;
                this.ViewTarget.Width = _valueTrackBar.Right                + CONTROL_PADDING;
            } else {
                _valueTrackBar.Location = new Point(this.ViewTarget.Width - CONTROL_PADDING - TRACKBAR_WIDTH, 0);
            }
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
