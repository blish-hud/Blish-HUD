using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Settings.UI.Presenters;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views.SettingViews;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public class SettingsView : View<SettingsPresenter> {
        
        private const int SETTING_PADDING = 8;

        private readonly Dictionary<SettingEntry, ViewContainer> _settingViews = new Dictionary<SettingEntry, ViewContainer>();

        public string CategoryTitle {
            get => this.ViewTarget.Title;
            set => this.ViewTarget.Title = value;
        }

        public SettingsView(SettingCollection settings) {
            this.Presenter = new SettingsPresenter(this, settings);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            
        }

        private void RefreshLayout() {
            int lastBottom = SETTING_PADDING;

            foreach (var viewSet in _settingViews) {
                viewSet.Value.Location = new Point(SETTING_PADDING, lastBottom);

                lastBottom = viewSet.Value.Bottom + SETTING_PADDING;
            }

            this.ViewTarget.Height = lastBottom + (this.ViewTarget.Height - this.ViewTarget.ContentRegion.Height);
        }

        public ViewContainer AddSettingView(SettingEntry setting) {
            var settingContainer = new ViewContainer() {
                Width = this.ViewTarget.Width
            };

            IView settingView;

            if ((settingView = SettingView.FromType(setting, settingContainer.Width)) == null) return null;

            settingContainer.Show(settingView);
            settingContainer.Parent = this.ViewTarget;

            _settingViews.Add(setting, settingContainer);

            RefreshLayout();

            return settingContainer;
        }

        public void RemoveSettingView(SettingEntry setting) {
            if (_settingViews.TryGetValue(setting, out var display)) {
                display.Dispose();

                RefreshLayout();
            }
        }

    }
}
