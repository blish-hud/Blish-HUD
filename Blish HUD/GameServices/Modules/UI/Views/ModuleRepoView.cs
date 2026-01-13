using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Graphics.UI.Exceptions;
using Blish_HUD.Modules.Pkgs;
using Blish_HUD.Modules.UI.Presenters;
using Blish_HUD.Strings.GameServices;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public class ModuleRepoView : View {

        public FlowPanel        RepoFlowPanel { get; private set; }
        public ContextMenuStrip SettingsMenu  { get; private set; }

        private TextBox        _searchbox;
        private StandardButton _restartBlishHud;
        private Label          _restartBlishHudWarning;

        public bool DirtyAssemblyStateExists {
            get => (_restartBlishHud ?? throw new ViewNotBuiltException()).Visible;
            set => _restartBlishHudWarning.Visible = (_restartBlishHud ?? throw new ViewNotBuiltException()).Visible = value;
        }

        public ModuleRepoView() { /* NOOP */ }

        public ModuleRepoView(IPkgRepoProvider pkgRepoProvider) {
            this.WithPresenter(new ModuleRepoPresenter(this, pkgRepoProvider));
        }

        protected override void Build(Container buildPanel) {
            _searchbox = new TextBox {
                PlaceholderText = Strings.Common.PlaceholderSearch,
                Width           = buildPanel.Width - 56,
                Parent          = buildPanel
            };

            var settingsButton = new GlowButton {
                Location         = new Point(_searchbox.Right + 4, _searchbox.Top),
                Icon             = AsyncTexture2D.FromAssetId(157109),
                ActiveIcon       = AsyncTexture2D.FromAssetId(157110),
                Visible          = true,
                BasicTooltipText = Strings.Common.Options,
                Parent           = buildPanel
            };

            this.SettingsMenu = new ContextMenuStrip();

            this.RepoFlowPanel = new FlowPanel {
                Width               = buildPanel.Width,
                Height              = buildPanel.Height - _searchbox.Bottom - 44,
                Top                 = _searchbox.Bottom                     + 12,
                CanScroll           = true,
                ControlPadding      = new Vector2(0, 5),
                OuterControlPadding = new Vector2(5, 5),
                Parent              = buildPanel
            };

            _restartBlishHud = new StandardButton {
                Text    = string.Format(Strings.Common.Action_Restart, Strings.Common.BlishHUD),
                Width   = 132,
                Top     = this.RepoFlowPanel.Bottom + 5,
                Right   = this.RepoFlowPanel.Right  - 23,
                Visible = false,
                Parent  = buildPanel,
            };

            _restartBlishHudWarning = new Label {
                Text              = ModulesService.PkgManagement_ModulesNeedRestart,
                AutoSizeWidth     = true,
                AutoSizeHeight    = false,
                VerticalAlignment = VerticalAlignment.Middle,
                TextColor         = Control.StandardColors.Yellow,
                Height            = _restartBlishHud.Height,
                Top               = _restartBlishHud.Top,
                Right             = _restartBlishHud.Left - 4,
                Visible           = false,
                Parent            = buildPanel
            };

            _searchbox.TextChanged += SearchboxOnTextChanged;

            _restartBlishHud.Click += (sender, args) => {
                GameService.Overlay.Restart();
            };

            settingsButton.Click += (sender, args) => {
                SettingsMenu.Show((Control) sender);
            };
        }

        private void SearchboxOnTextChanged(object sender, EventArgs e) {
            this.RepoFlowPanel.FilterChildren<ViewContainer>(viewContainer => PkgParamFilter(viewContainer, PkgNeedsUpdateFilter, PkgSearchFilter));
        }

        private bool PkgSearchFilter(ViewContainer viewContainer) {
            var pkgView = viewContainer.CurrentView as ManagePkgView;

            var searchText = _searchbox.Text.ToLowerInvariant();
            return pkgView.ModuleName.ToLowerInvariant().Contains(searchText) || pkgView.ModuleDescription.ToLowerInvariant().Contains(searchText);
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
