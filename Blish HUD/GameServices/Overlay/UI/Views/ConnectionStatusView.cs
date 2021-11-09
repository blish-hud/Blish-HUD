using System.Timers;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Presenters;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Overlay.UI.Views {
    public class ConnectionStatusView : View<IConnectionStatusPresenter> {

        private const int UPDATE_INTERVAL = 1500;

        private Label _connectionNameLabel;
        private Image _connectionStatusImage;

        private readonly Timer _updateTimer;

        public ConnectionStatusView() {
            _updateTimer = new Timer(UPDATE_INTERVAL);
            _updateTimer.Elapsed += UiUpdateTimerElapsed;
            _updateTimer.Start();
        }

        private void UiUpdateTimerElapsed(object sender, ElapsedEventArgs e) => OnPresenterAssigned(this.Presenter);

        protected override void Build(Container buildPanel) {
            _connectionStatusImage = new Image() {
                Size     = new Point(16, 16),
                Location = new Point(2,  2),
                Texture  = GameService.Content.GetTexture(@"157330-cantint"),
                Parent   = buildPanel
            };

            _connectionNameLabel = new Label() {
                Size       = new Point(buildPanel.Width - 20, buildPanel.Height),
                Location   = new Point(20,                    0),
                ShowShadow = true,
                Parent     = buildPanel
            };

            OnPresenterAssigned(this.Presenter);
        }

        protected override void OnPresenterAssigned(IConnectionStatusPresenter presenter) {
            if (_connectionNameLabel == null || _connectionStatusImage == null) return;

            _connectionNameLabel.Text = presenter == null
                                            ? string.Empty
                                            : presenter.ConnectionName;

            _connectionStatusImage.Tint = !(presenter is { Connected: true })
                                              ? Color.White
                                              : Color.LightGreen;

            _connectionStatusImage.BasicTooltipText = presenter == null
                                                          ? string.Empty
                                                          : presenter.ConnectionDetails;

            _connectionNameLabel.BasicTooltipText = _connectionStatusImage.BasicTooltipText;
        }

        protected override void Unload() {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= UiUpdateTimerElapsed;
            _updateTimer.Dispose();
        }

    }
}
