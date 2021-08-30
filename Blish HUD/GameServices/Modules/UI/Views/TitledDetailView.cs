using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views {
    public abstract class TitledDetailView : View {

        public enum DetailLevel {
            Info,
            Warning
        }

        private Panel      _rootPanel;
        private GlowButton _warningIcon;
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

        protected sealed override void Build(Container viewTarget) {
            _rootPanel = new Panel() {
                Size       = viewTarget.ContentRegion.Size,
                ShowBorder = true,
                CanScroll  = true,
                Parent     = viewTarget
            };

            _warningIcon = new GlowButton() {
                Size        = new Point(32,  32),
                Location    = new Point(-10, -15),
                Icon       = GameService.Content.GetTexture("common/440023"),
                ActiveIcon = GameService.Content.GetTexture("common/440024"),
                Visible     = false,
                ClipsBounds = false,
                Parent      = viewTarget
            };

            _menuButton = new GlowButton() {
                Location         = new Point(viewTarget.ContentRegion.Width - 42, 3),
                Icon             = GameService.Content.GetTexture("common/157109"),
                ActiveIcon       = GameService.Content.GetTexture("common/157110"),
                BasicTooltipText = Strings.Common.Options,
                Visible          = false,
                Parent           = viewTarget
            };

            _menuButton.Click += MenuButtonOnClick;

            BuildDetailView(_rootPanel);
        }

        private void MenuButtonOnClick(object sender, MouseEventArgs e) {
            this.Menu?.Show(_menuButton);
        }

        public void SetDetails(string status, DetailLevel level) {
            switch (level) {
                case DetailLevel.Info:
                    _warningIcon.Icon       = GameService.Content.GetTexture("common/440023");
                    _warningIcon.ActiveIcon = GameService.Content.GetTexture("common/440024");
                    break;
                case DetailLevel.Warning:
                    _warningIcon.Icon       = GameService.Content.GetTexture("common/482924");
                    _warningIcon.ActiveIcon = GameService.Content.GetTexture("common/482925");
                    break;
            }

            _warningIcon.BasicTooltipText = status;
            _warningIcon.Show();
        }

        public void ClearDetails() {
            _warningIcon.Hide();
        }

        protected abstract void BuildDetailView(Panel buildPanel);

    }
}
