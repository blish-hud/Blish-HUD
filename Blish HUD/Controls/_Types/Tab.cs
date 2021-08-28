using System;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Controls {
    public class Tab : ITab {

        public AsyncTexture2D Icon { get; set; }

        public int Priority { get; }

        public Tooltip Tooltip { get; set; }

        public Func<IView> View { get; set; }

        IView ITab.View => this.View();

        public Tab(AsyncTexture2D icon, Func<IView> view, Tooltip tooltip, int? priority = null) {
            this.Icon     = icon;
            this.Tooltip  = tooltip;
            this.Priority = priority ?? icon.GetHashCode();
            this.View     = view;
        }

        public Tab(AsyncTexture2D icon, Func<IView> view, string tooltip = "", int? priority = null)
            : this(icon, view, new Tooltip(new BasicTooltipView(tooltip)), priority) { }

    }
}
