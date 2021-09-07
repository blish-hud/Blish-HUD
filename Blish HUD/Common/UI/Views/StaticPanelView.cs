using System;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Common.UI.Views {

    /// <summary>
    /// Allows you to pass a static panel as a view.  Primarily used as a stopgap.
    /// </summary>
    public class StaticPanelView : View {

        private readonly Panel _panel;

        public StaticPanelView(Panel panel) {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
        }

        protected override void Build(Container buildPanel) {
            _panel.Location         = Point.Zero;
            _panel.HeightSizingMode = SizingMode.Fill;
            _panel.WidthSizingMode  = SizingMode.Fill;
            _panel.Parent           = buildPanel;
        }

    }
}
