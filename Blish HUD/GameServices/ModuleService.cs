using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Modules;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class ModuleService : GameService {

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";

        private const string MODULES_DIRECTORY = "modules";

        private string ModulesDirectory => Path.Combine(GameService.FileSrv.BasePath, MODULES_DIRECTORY);

        public SettingEntry<Dictionary<string, bool>> ModuleStates { get; private set; }

        public List<IModule> AvailableModules { get; private set; }

        [ImportMany]
        public List<IModule> ExternalModules { get; set; }

        protected override void Initialize() {
            this.AvailableModules = new List<IModule>();

            this.ModuleStates = Settings.CoreSettings.DefineSetting(
                                                                    MODULESTATES_CORE_SETTING,
                                                                    new Dictionary<string, bool>(), 
                                                                    new Dictionary<string, bool>()
                                                                   );
        }

        public void RegisterModule(IModule module) {
            this.AvailableModules.Add(module);

            // All modules are disabled by default, currently to allow for users to select which modules they would like to enable
            if (this.ModuleStates.Value.ContainsKey(module.GetModuleInfo().Namespace)) {
                module.Enabled = this.ModuleStates.Value[module.GetModuleInfo().Namespace];
            } else {
                this.ModuleStates.Value.Add(module.GetModuleInfo().Namespace, false);
            }
        }

        private void ComposeModulesFromNamespace() {
            AssemblyCatalog      catalog   = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            CompositionContainer container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);
        }

        private void ComposeModulesFromDirectory(string directory) {
            DirectoryCatalog catalog = new DirectoryCatalog(directory);
            CompositionContainer container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);
        }

        protected override void Load() {
            if (!Directory.Exists(this.ModulesDirectory)) Directory.CreateDirectory(this.ModulesDirectory);

            // TODO: Load modules dynamically
            RegisterModule(new Modules.DebugText());
            RegisterModule(new Modules.DiscordRichPresence());
            RegisterModule(new Modules.BeetleRacing.BeetleRacing());
            RegisterModule(new Modules.EventTimers.EventTimers());
            RegisterModule(new Modules.Compass());
            // RegisterModule(new Modules.RangeCircles());
            RegisterModule(new Modules.PoiLookup.PoiLookup());
            // RegisterModule(new Modules.MouseUsability.MouseUsability());
            RegisterModule(new Modules.MarkersAndPaths.MarkersAndPaths());

            //ComposeModulesFromNamespace();
#if DEBUG
            ComposeModulesFromDirectory(Directory.GetCurrentDirectory());
#else
            ComposeModulesFromDirectory(this.ModulesDirectory);
#endif

            foreach (var externalModule in this.ExternalModules) {
                Console.WriteLine($"Registering external module: {externalModule.GetModuleInfo().Name}");
                RegisterModule(externalModule);
            }
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
