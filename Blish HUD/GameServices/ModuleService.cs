using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class ModuleService : GameService {

        public SettingEntry<Dictionary<string, bool>> ModuleStates { get; private set; }

        private List<Modules.Module> _availableModules;
        public List<Modules.Module> AvailableModules => _availableModules;

        protected override void Initialize() {
            _availableModules = new List<Modules.Module>();

            this.ModuleStates = GameService.Settings.CoreSettings.DefineSetting("ModuleStates",
                new Dictionary<string, bool>(), new Dictionary<string, bool>());
        }

        public void RegisterModule(Modules.Module module) {
            this.AvailableModules.Add(module);

            // All modules are disabled by default, currently to allow for users to select which modules they would like to enable
            if (this.ModuleStates.Value.ContainsKey(module.GetModuleInfo().Namespace)) {
                module.Enabled = this.ModuleStates.Value[module.GetModuleInfo().Namespace];
            } else {
                this.ModuleStates.Value.Add(module.GetModuleInfo().Namespace, false);
            }
        }

        protected override void Load() {
            // TODO: Load modules dynamically
            RegisterModule(new Modules.DebugText());
            RegisterModule(new Modules.DiscordRichPresence());
            RegisterModule(new Modules.BeetleRacing.BeetleRacing());
            RegisterModule(new Modules.EventTimers.EventTimers());
            RegisterModule(new Modules.Compass());
            // RegisterModule(new Modules.RangeCircles());
            RegisterModule(new Modules.PoiLookup.PoiLookup());
            // RegisterModule(new Modules.MouseUsability.MouseUsability());
            // RegisterModule(new Modules.MarkersAndPaths.MarkersAndPaths());
        }

        protected override void Unload() {

        }

        protected override void Update(GameTime gameTime) {
            this.AvailableModules.ForEach(s => {
                //try {
                    if (s.Enabled) s.Update(gameTime);
                //} catch (Exception ex) {
                //    Console.WriteLine($"{s.GetModuleInfo().Name} module had an error:");
                //    Console.WriteLine(ex.Message);
                //}
            });
        }
    }
}
