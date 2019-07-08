using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.WebApi.V2.Models;
namespace Blish_HUD.Modules {

    public struct ModuleInfo {

        public readonly string Name;
        public readonly Texture2D Icon;
        public readonly string Author;
        public readonly string Version;
        public readonly string Description;
        public readonly string Namespace;
        public readonly TokenPermission[] Permissions; 

        public readonly bool EnabledWithoutGw2;

        /// <param name="name">The name of the module. This is the name that users will see when your module is listed.</param>
        /// <param name="icon">An icon that represents your module.  It should be no larger than 32x32.</param>
        /// <param name="namespace">The namespace of your module.  This should almost always be 'typeof(YourModuleClass).FullName'."/></param>
        /// <param name="description">The description of your module that will be shown to users.</param>
        /// <param name="author">Your online tag, GW2 username, or your name.</param>
        /// <param name="version">The current version of your module.</param>
        /// <param name="permissions">Optional: Required Guild Wars 2 API permissions.</param>
        /// <param name="enabledWithoutGw2">If enabled, your module will not be unloaded when GW2 is closed and left in the tray.</param>
        public ModuleInfo(string name, Texture2D icon, string @namespace, string description, string author, string version, bool enabledWithoutGw2 = false, TokenPermission[] permissions = null) {
            this.Name = name;
            this.Icon = icon;
            this.Namespace = @namespace;
            this.Description = description;
            this.Author = author;
            this.Version = version;
            this.Permissions = permissions;

            this.EnabledWithoutGw2 = enabledWithoutGw2;
        }

    }

    public abstract class Module : IModule {

        public abstract ModuleInfo GetModuleInfo();
        public abstract void DefineSettings(Settings settings);

        public Settings Settings { get; set; }

        protected bool _enabled = false;
        
        private List<WindowTab> TabsAdded = new List<WindowTab>();

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
            this.Settings = GameService
                           .Settings
                           .RegisterSettings(GetModuleInfo().Namespace, true);

            DefineSettings(this.Settings);
        }

        public virtual void OnLoad() { Loaded = true; }
        public virtual void OnEnabled() {
            if (!Loaded) OnLoad();
        }

        public virtual void OnDisabled() {
            // Clear out any tabs that were made (that the module didn't clean up)
            foreach (var windowTab2 in TabsAdded) {
                GameService.Director.BlishHudWindow.RemoveTab(windowTab2);
            }
        }
        public virtual void Update(GameTime gameTime) { /* NOOP */ }

        // Module Options

        protected void AddSectionTab(string tabName, Texture2D icon, Panel panel) {
            TabsAdded.Add(GameService.Director.BlishHudWindow.AddTab(tabName, icon, panel));
        }

        protected void AddSettingsTab() {

        }

    }
}
