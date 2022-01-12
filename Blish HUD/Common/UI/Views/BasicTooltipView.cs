using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Views {
    public class BasicTooltipView : View, ITooltipView {
        
        private const int MAX_WIDTH = 500;

        private readonly Label _tooltipLabel;

        public string Text {
            get => _tooltipLabel.Text;
            set => UpdateLabelValueAndWidth(value);
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

        private void UpdateLabelValueAndWidth(string value) {
            // A bit of a kludge until we get proper MaxWidth and MaxHeight properties.
            _tooltipLabel.WrapText      = false;
            _tooltipLabel.AutoSizeWidth = true;
            _tooltipLabel.Text          = value;

            if (_tooltipLabel.Width > MAX_WIDTH) {
                _tooltipLabel.AutoSizeWidth = false;
                _tooltipLabel.WrapText      = true;
                _tooltipLabel.Width         = MAX_WIDTH;
            }
        }

    }
}
