using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public struct WindowTab {
        public string    Name     { get; set; }
        public Texture2D Icon     { get; set; }
        public int       Priority { get; set; }

        public WindowTab(string name, Texture2D icon) : this(name, icon, name.Length) { /* NOOP */ }

        public WindowTab(string name, Texture2D icon, int priority) {
            this.Name     = name;
            this.Icon     = icon;
            this.Priority = priority;
        }

        public static bool operator ==(WindowTab tab1, WindowTab tab2) {
            return tab1.Name == tab2.Name && tab1.Icon == tab2.Icon;
        }

        public static bool operator !=(WindowTab tab1, WindowTab tab2) {
            return !(tab1 == tab2);
        }
    }

}
