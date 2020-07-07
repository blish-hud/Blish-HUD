using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public abstract class TitledDetailView : View {

        private Panel      _rootPanel;
        private Image      _warningIcon;
        private GlowButton _menuButton;

        private ContextMenuStrip _menu;

        public ContextMenuStrip Menu {
            get => _menu;
            set {
                _menu = value;

                _menuButton.Visible = _menu != null;
            }
        }

        protected string Title {
            get => _rootPanel.Title;
            set => _rootPanel.Title = value;
        }

        protected sealed override void Build(Panel buildPanel) {
            _rootPanel = new Panel() {
                Size       = buildPanel.ContentRegion.Size,
                ShowBorder = true,
                CanScroll  = true,
                Parent     = buildPanel
            };

            _warningIcon = new Image(GameService.Content.GetTexture("common/1444522")) {
                Size        = new Point(32,  32),
                Location    = new Point(-10, -15),
                Visible     = false,
                ClipsBounds = false,
                Parent      = buildPanel
            };

            _menuButton = new GlowButton() {
                Location         = new Point(buildPanel.ContentRegion.Width - 42, 3),
                Icon             = GameService.Content.GetTexture("common/157109"),
                ActiveIcon       = GameService.Content.GetTexture("common/157110"),
                BasicTooltipText = Strings.Common.Options,
                Visible          = false,
                Parent           = buildPanel
            };

            _menuButton.Click += MenuButtonOnClick;

            BuildDetailView(_rootPanel);
        }

        private void MenuButtonOnClick(object sender, MouseEventArgs e) {
            this.Menu?.Show(_menuButton);
        }

        public void SetWarning(string status) {
            _warningIcon.BasicTooltipText = status;
            _warningIcon.Show();
        }

        public void ClearWarning() {
            _warningIcon.Hide();
        }

        protected abstract void BuildDetailView(Panel buildPanel);

    }
}
