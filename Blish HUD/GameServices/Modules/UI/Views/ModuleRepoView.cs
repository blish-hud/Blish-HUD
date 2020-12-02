using System;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Presenters;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleRepoView : View {

        public FlowPanel RepoFlowPanel { get; private set; }

        public ContextMenuStrip SettingsMenu { get; private set; }

        private TextBox _searchbox;

        public ModuleRepoView() { /* NOOP */ }

        public ModuleRepoView(IPkgRepoProvider pkgRepoProvider) {
            this.WithPresenter(new ModuleRepoPresenter(this, pkgRepoProvider));
        }

        protected override void Build(Panel buildPanel) {
            _searchbox = new TextBox() {
                PlaceholderText = "Search...",
                Width           = buildPanel.Width - (32 + 24),
                Parent          = buildPanel
            };

            var settingsButton = new GlowButton() {
                Location         = new Point(_searchbox.Right + 4, _searchbox.Top),
                Icon             = GameService.Content.GetTexture("common/157109"),
                ActiveIcon       = GameService.Content.GetTexture("common/157110"),
                Visible          = true,
                BasicTooltipText = Strings.Common.Options,
                Parent           = buildPanel
            };

            this.SettingsMenu = new ContextMenuStrip();

            this.RepoFlowPanel = new FlowPanel() {
                Width               = buildPanel.Width,
                Height              = buildPanel.Height - _searchbox.Bottom - 12,
                Top                 = _searchbox.Bottom                     + 12,
                CanScroll           = true,
                ControlPadding      = new Vector2(0, 5),
                OuterControlPadding = new Vector2(5, 5),
                Parent              = buildPanel
            };

            _searchbox.TextChanged += SearchboxOnTextChanged;

            settingsButton.Click += delegate(object sender, MouseEventArgs args) {
                SettingsMenu.Show((Control) sender);
            };
        }

        private void SearchboxOnTextChanged(object sender, EventArgs e) {
            this.RepoFlowPanel.FilterChildren<ViewContainer>((viewContainer) => PkgParamFilter(viewContainer, PkgNeedsUpdateFilter, PkgSearchFilter));
        }

        private bool PkgSearchFilter(ViewContainer viewContainer) {
            var pkgView = viewContainer.CurrentView as ManagePkgView;

            return pkgView.ModuleName.ToLowerInvariant().Contains(_searchbox.Text.ToLowerInvariant());
        }

        private bool PkgNeedsUpdateFilter(ViewContainer viewContainer) {
            var pkgView = viewContainer.CurrentView as ManagePkgView;

            return true;
        }

        private bool PkgParamFilter(ViewContainer viewContainer, params Func<ViewContainer, bool>[] filters) {
            for (int i = 0; i < filters.Length; i++) {
                if (!filters[i](viewContainer)) {
                    return false;
                }
            }

            return true;
        }

    }
}
