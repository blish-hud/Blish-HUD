using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Views {
    public class BasicTooltipView : View, ITooltipView {
        
        private readonly Label _tooltipLabel;

        public string Text {
            get => _tooltipLabel.Text;
            set => _tooltipLabel.Text = value;
        }

        public BasicTooltipView(string text) {
            _tooltipLabel = new Label() {
                ShowShadow     = true,
                AutoSizeHeight = true,
                AutoSizeWidth  = true
            };

            this.Text = text;
        }

        protected override void Build(Container buildPanel) {
            _tooltipLabel.Parent = buildPanel;

            buildPanel.Hidden += (sender, args) => buildPanel.Dispose();
        }

    }
}
