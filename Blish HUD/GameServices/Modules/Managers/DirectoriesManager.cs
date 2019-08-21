using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Modules.Managers {

    public class DirectoriesManager {

        protected static readonly Logger Logger = Logger.GetLogger(typeof(DirectoriesManager));

        private readonly HashSet<string>            _directoryNames;
        private readonly Dictionary<string, string> _directoryPaths;

        public IReadOnlyList<string> RegisteredDirectories => _directoryNames.ToList();

        private DirectoriesManager(IEnumerable<string> directories) {
            _directoryNames = new HashSet<string>(directories, StringComparer.OrdinalIgnoreCase);
            _directoryPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            PrepareDirectories();
        }

        public static DirectoriesManager GetModuleInstance(ModuleManager module) {
            return new DirectoriesManager(module.Manifest.Directories ?? new List<string>(0));
        }

        private void PrepareDirectories() {
            foreach (string directoryName in _directoryNames) {
                string registeredDirectory = DirectoryUtil.RegisterDirectory(directoryName);

                Logger.Info("Directory {directoryName} ({$registeredPath}) was registered.", directoryName, registeredDirectory);

                _directoryPaths.Add(directoryName, registeredDirectory);
            }
        }

        public string GetFullDirectoryPath(string directoryName) {
            if (!_directoryNames.Contains(directoryName)) return null;

            return _directoryPaths[directoryName];
        }

    }

}
