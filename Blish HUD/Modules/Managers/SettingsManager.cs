using Blish_HUD.Settings;

namespace Blish_HUD.Modules.Managers {
    public class SettingsManager {

        private readonly SettingCollection _settingRoot;

        public SettingCollection ModuleSettings => _settingRoot;

        public SettingsManager(ModuleManager module) {
            if (module.State.Settings == null) {
                module.State.Settings = new SettingCollection(true);
            }

            _settingRoot = module.State.Settings;
        }

        public static SettingsManager GetModuleInstance(ModuleManager module) {
            return new SettingsManager(module);
        }

    }
}
