using System;
using Blish_HUD.Content;

namespace Blish_HUD.Controls {

    [Obsolete("This control will be removed in the future.  Use TabbedWindow2 and Tab class instead.")]
    public class WindowTab {
        public string         Name     { get; set; }
        public AsyncTexture2D Icon     { get; set; }
        public int            Priority { get; set; }

        public WindowTab(string name, AsyncTexture2D icon) : this(name, icon, name.GetHashCode()) { /* NOOP */ }

        public WindowTab(string name, AsyncTexture2D icon, int priority) {
            this.Name     = name;
            this.Icon     = icon;
            this.Priority = priority;
        }
    }

}
