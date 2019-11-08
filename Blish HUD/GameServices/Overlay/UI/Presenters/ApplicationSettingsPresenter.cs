using System.Collections.Generic;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Overlay.UI.Presenters {
    public class ApplicationSettingsPresenter : Presenter<RepeatedView<IEnumerable<ApplicationSettingsPresenter.SettingsCategory>>, IEnumerable<ApplicationSettingsPresenter.SettingsCategory>> {

        public struct SettingsCategory {

            private readonly string            _name;
            private readonly SettingCollection _settings;

            public string            Name     => _name;
            public SettingCollection Settings => _settings;

            public SettingsCategory(string name, SettingCollection settings) {
                _name     = name;
                _settings = settings;
            }

        }

        /// <inheritdoc />
        public ApplicationSettingsPresenter(RepeatedView<IEnumerable<SettingsCategory>> view, IEnumerable<SettingsCategory> model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override void UpdateView() {
            foreach (var settingCategory in this.Model) {
                var settingView = new SettingsView(settingCategory.Settings);
                this.View.Views.Add(settingView);

                settingView.Built += delegate {
                    settingView.CategoryTitle = settingCategory.Name;
                };
            }
        }

    }
}
