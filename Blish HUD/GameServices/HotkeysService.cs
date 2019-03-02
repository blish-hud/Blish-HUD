using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.BHUDControls.Hotkeys;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD {

    public class Hotkey {

        public List<Keys> Keys { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }


        public Hotkey(string name, string description, string category, IEnumerable<Keys> hotkeys) {
            this.Keys = new List<Keys>(hotkeys.ToArray());

            this.Name = name;
            this.Description = description;
            this.Category = category;
        }

    }

    public class HotkeysService : GameService {

        protected override void Initialize() { }

        protected override void Load() { }

        protected override void Unload() { }

        protected override void Update(GameTime gameTime) { }

        internal Panel BuildHotkeysPanel(Window wndw) {
            var hkPanel = new Panel();

            var backButton = new BackButton(wndw) {
                Text     = "Settings",
                NavTitle = "Hotkeys",
                Parent   = hkPanel,
                Location = new Point(20, 20),
            };

            int nameWidth = 183;

            var thotkey1 = new Hotkey("Test Hotkey 1", "This hotkey is used for example purposes only.", "Debug", new[] { Keys.LeftShift, Keys.LeftAlt, Keys.L });
            var thotkey2 = new Hotkey("Test Hotkey 2", "This hotkey is used for example purposes only.", "Debug", new[] { Keys.LeftShift, Keys.LeftAlt, Keys.V });

            var thk1 = new HotkeyAssigner(thotkey1) {
                NameWidth = nameWidth,
                Location  = new Point(10, backButton.Bottom + 25),
                Width     = wndw.ContentRegion.Width / 2,
                Parent    = hkPanel,
            };

            var thk2 = new HotkeyAssigner(thotkey2) {
                NameWidth = nameWidth,
                Left = thk1.Left,
                Top = thk1.Bottom + 2,
                Width = wndw.ContentRegion.Width / 2,
                Parent    = hkPanel,
            };



            return hkPanel;
        }

    }

}
