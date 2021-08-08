using Blish_HUD.Settings;

namespace Blish_HUD.Modules.Managers {
    public class SettingsManager {

        public ISettingCollection ModuleSettings { get; }

        private SettingsManager(ModuleManager module) {
            if (module.State.Settings == null) {
                module.State.Settings = new SettingCollection();
            }

            this.ModuleSettings = module.State.Settings;
        }

        public static SettingsManager GetModuleInstance(ModuleManager module) {
            return new SettingsManager(module);
        }

    }
}
