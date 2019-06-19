using System.Collections.Generic;
using System.Linq;
using Blish_HUD.BHUDControls.Hotkeys;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace Blish_HUD {

    public class Hotkey {

        [JsonProperty]
        public List<Keys> Keys { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }


        public Hotkey(string name, string description, string category, IEnumerable<Keys> keys) {
            this.Keys = new List<Keys>(keys.ToArray());

            this.Name = name;
            this.Description = description;
            this.Category = category;
        }

    }

    public class HotkeysService : GameService {

        private const string HOTKEY_SETTINGS = "HotkeyConfiguration";

        internal SettingCollection _hotkeySettings;

        protected override void Initialize() {
            _hotkeySettings = Settings.RegisterRootSettingCollection(HOTKEY_SETTINGS);

            DefineSettings(_hotkeySettings);
        }

        private void DefineSettings(SettingCollection settings) {
            settings.DefineSetting("Test hotkey", new Hotkey("Test hotkey2", "This is the description", "cate", new[] { Keys.LeftShift, Keys.LeftAlt, Keys.L }));
            settings.DefineSetting("Test hotkey2", new Hotkey("Test hotkey3", "This is thasfe description", "catase", new[] { Keys.LeftShift, Keys.LeftAlt, Keys.K }));
        }

        protected override void Load() {
            
        }

        protected override void Unload() { }

        protected override void Update(GameTime gameTime) { }

        internal Panel BuildHotkeysPanel(WindowBase wndw) {
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
