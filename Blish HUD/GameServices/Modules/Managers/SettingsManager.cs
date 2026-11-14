using Blish_HUD.Settings;

namespace Blish_HUD.Modules.Managers {
    public class SettingsManager {

        public SettingCollection ModuleSettings { get; }

        private SettingsManager(ModuleManager module) {
            SettingCollection settings = GameService.Settings.LoadModuleSettings(module.Manifest.Namespace)
                                      ?? new SettingCollection(true);

            module.State.Settings = settings;
            this.ModuleSettings   = settings;

            // Register so that the periodic save includes this module's settings
            GameService.Settings.RegisterModuleSettings(module.Manifest.Namespace, settings);
        }

        internal static SettingsManager GetModuleInstance(ModuleManager module) {
            return new SettingsManager(module);
        }

    }
}
