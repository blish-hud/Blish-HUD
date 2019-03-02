using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Modules {

    public struct ModuleInfo {

        public readonly string Name;
        public readonly string Author;
        public readonly string Version;
        public readonly string Description;
        public readonly string Namespace;

        public ModuleInfo(string name, string @namespace, string description, string author, string version) {
            this.Name = name;
            this.Namespace = @namespace;
            this.Description = description;
            this.Author = author;
            this.Version = version;
        }

    }

    public abstract class Module {

        public abstract ModuleInfo GetModuleInfo();
        public abstract void DefineSettings(Settings settings);

        public readonly Settings Settings;

        protected bool _enabled = false;
        
        private List<WindowTab2> TabsAdded = new List<WindowTab2>();

        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;

                _enabled = value;

                if (_enabled) OnEnabled();
                else OnDisabled();
            }
        }

        protected bool Loaded = false;

        public Module() {
            this.Settings = GameServices.GetService<SettingsService>()
                .RegisterSettings(this.GetModuleInfo().Namespace, true);

            this.DefineSettings(this.Settings);
        }

        protected virtual void OnLoad() { Loaded = true; }
        protected virtual void OnEnabled() {
            if (!Loaded) OnLoad();
        }

        protected virtual void OnDisabled() {

            // Clear out any tabs that were made
            foreach (var windowTab2 in TabsAdded) {
                GameServices.GetService<DirectorService>().BlishHudWindow.RemoveTab(windowTab2);
            }
        }
        public virtual void Update(GameTime gameTime) { /* NOOP */ }

        // Module Options

        protected void AddSectionTab(string tabName, string icon, Panel panel) {
            TabsAdded.Add(GameService.Director.BlishHudWindow.AddTab(tabName, icon, panel));
        }

        protected void AddSettingsTab() {

        }

    }
}
