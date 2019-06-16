using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.Managers {
    public class SettingsManager {

        private ModuleManager _module;

        private SettingCollection _settingRoot;

        public SettingCollection ModuleSettings => _settingRoot;

        public SettingsManager(ModuleManager module) {
            _module = module;

            if (_module.State.Settings == null) {
                _module.State.Settings = new SettingCollection(true);
            }

            _settingRoot = _module.State.Settings;
        }

        public static SettingsManager GetModuleInstance(ModuleManager module) {
            return new SettingsManager(module);
        }

    }
}
